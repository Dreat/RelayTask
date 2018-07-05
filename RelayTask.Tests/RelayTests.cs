using System;
using Xunit;

namespace RelayTask.Tests
{
    public class RelayTests
    {
        public event EventHandler<SystemEventArgs> MessageHappened;
        [Fact]
        public void Test()
        {
            var relay = new Relay();
            var publisher = new Publisher(relay);
            MessageHappened += publisher.SystemMessageEmitted;
            MessageHappened(this, new SystemEventArgs());
        }
       
        protected virtual void OnMessageHappened(SystemEventArgs e)
        {
            MessageHappened?.Invoke(this, e);
        }
    }

}