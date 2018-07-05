using System;

namespace RelayTask
{
    public class Publisher
    {
        private readonly Relay _relay;

        public Publisher(Relay relay)
        {
            _relay = relay;
        }

        public void SystemMessageEmitted(object sender, SystemEventArgs e)
        {
            _relay.HandleMessage(new Message
            {
                CorrelationId = GenerateCorrelationId()
            });
        }

        private int GenerateCorrelationId()
        {
            throw new NotImplementedException();
        }
    }
}