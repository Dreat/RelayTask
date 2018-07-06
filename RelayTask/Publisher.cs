using System;
using System.Collections;
using System.Collections.Generic;

namespace RelayTask
{
    public class Publisher
    {
        private readonly Relay _relay;
        private bool _backpressureNeeded = false;

        public event EventHandler<Message> MessagePublished;
        public readonly Queue<SystemEventArgs> SystemEvents;

        public Publisher(Relay relay)
        {
            _relay = relay;
            _relay.BackPressureNeeded += BackpressureHandler;
            MessagePublished += _relay.HandleMessagePublished;
            SystemEvents = new Queue<SystemEventArgs>();
        }

        public void Run()
        {
            while (true)
            {
                if (_backpressureNeeded || SystemEvents.Count < 1)
                {
                    continue;
                }
                var systemEvent = SystemEvents.Dequeue();

                MessagePublished?.Invoke(this, new Message
                {
                    Command = systemEvent.Command,
                    MessageType = systemEvent.MessageType
                });
            }
        }

        public void SystemMessageEmitted(object sender, SystemEventArgs e)
        {
            SystemEvents.Enqueue(e);
        }

        private void BackpressureHandler(object sender, bool e)
        {
            _backpressureNeeded = e;
        }
    }
}