using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RelayTask.Abstract;
using Xunit;

namespace RelayTask.Tests
{
    public class RelayTests
    {
        public event EventHandler<SystemEventArgs> SystemEventRaised;

        [Fact]
        public void Test()
        {
            var states = new List<bool>();
            var deadLetterQueue = new DeadLetterQueue();
            var invalidLetterQueue = new InvalidLetterQueue();
            var relay = new Relay(deadLetterQueue, invalidLetterQueue);
            var publisher = new Publisher(relay);
            SystemEventRaised += publisher.SystemMessageEmitted;

            new TaskFactory().StartNew(() => publisher.Run());

            SystemEventRaised?.Invoke(this, new SystemEventArgs
            {
                Command = "test command",
                MessageType = MessageType.WebOperation
            });

            SystemEventRaised?.Invoke(this, new SystemEventArgs
            {
                Command = "test command",
                MessageType = MessageType.WebOperation
            });

            // Check state changes - if backpressure was issued we will have "true" in that list
            relay.BackPressureNeeded += (sender, e) => states.Add(e);

            Thread.Sleep(10000);
        }
    }
}