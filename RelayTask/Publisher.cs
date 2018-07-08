using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RelayTask
{
    public class Publisher
    {
        private readonly Relay _relay;
        private readonly Queue<SystemEventArgs> _systemEvents;
        private bool _backpressureNeeded;

        public Publisher(Relay relay)
        {
            _relay = relay;
            _relay.BackPressureNeeded += BackpressureHandler;
            MessagePublished += _relay.HandleMessagePublished;
            _systemEvents = new Queue<SystemEventArgs>();
        }

        public event EventHandler<Message> MessagePublished;

        public void Run()
        {
            new TaskFactory().StartNew(() =>
            {
                while (true)
                {
                    // Send messages only if we have something to send and Relay is ready to receive messages
                    if (_backpressureNeeded || _systemEvents.Count < 1)
                    {
                        // wait for load on relay to slow down
                        Thread.Sleep(2000);
                        continue;
                    }
                    var systemEvent = _systemEvents.Dequeue();

                    Thread.Sleep(100);

                    Console.WriteLine($"Passing message with command {systemEvent.Command} of type {systemEvent.MessageType} to relay");
                    MessagePublished?.Invoke(this, new Message
                    {
                        Command = systemEvent.Command,
                        MessageType = systemEvent.MessageType
                    });
                }
            });
        }

        public void SystemMessageEmitted(object sender, SystemEventArgs e)
        {
            // Queue ensures that order will be kept - as we send messages by dequeueing in loop
            _systemEvents.Enqueue(e);
        }

        private void BackpressureHandler(object sender, bool e)
        {
            _backpressureNeeded = e;
            var message = e ? "Backpressure issued!" : "Backpressure released!";
            Console.WriteLine(message);
        }
    }
}