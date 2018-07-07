using System;
using System.Collections.Generic;

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
            while (true)
            {
                // Send messages only if we have something to send and Relay is ready to receive messages
                if (_backpressureNeeded || _systemEvents.Count < 1) continue;
                var systemEvent = _systemEvents.Dequeue();

                MessagePublished?.Invoke(this, new Message
                {
                    Command = systemEvent.Command,
                    MessageType = systemEvent.MessageType
                });
            }
        }

        public void SystemMessageEmitted(object sender, SystemEventArgs e)
        {
            // Queue ensures that order will be kept - as we send messages by dequeueing in loop
            _systemEvents.Enqueue(e);
        }

        private void BackpressureHandler(object sender, bool e)
        {
            _backpressureNeeded = e;
        }
    }
}