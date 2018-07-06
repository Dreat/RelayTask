using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RelayTask.Tests
{
    public class RelayTests
    {
        public event EventHandler<SystemEventArgs> MessageHappened;

        [Fact]
        public void Test()
        {
            var deadLetterQueue = new DeadLetterQueue();
            var invalidLetterQueue = new InvalidLetterQueue();
            var relay = new Relay(deadLetterQueue, invalidLetterQueue);
            var publisher = new Publisher(relay);
            MessageHappened += publisher.SystemMessageEmitted;

            var startNew = new TaskFactory().StartNew(() => publisher.Run());

            MessageHappened?.Invoke(this, new SystemEventArgs
            {
                Command = "test command",
                MessageType = MessageType.WebOperation
            });

            MessageHappened?.Invoke(this, new SystemEventArgs
            {
                Command = "test command",
                MessageType = MessageType.WebOperation
            });

            Thread.Sleep(10000);
        }
    }
}