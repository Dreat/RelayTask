using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RelayTask
{
    public class Relay
    {
        private readonly List<IRemoteService> _remoteServices = new List<IRemoteService>();
        private readonly List<ISubscriber> _subscribers = new List<ISubscriber>();

        public void RegisterSubsriber(ISubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void RegisterRemoteService(IRemoteService remoteService)
        {
            _remoteServices.Add(remoteService);
        }

        public Task HandleMessage(Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.WebOperation:
                    return HandleWebOperation(message);
                case MessageType.LocalOperation:
                    return HandleLocalOperation(message);
                default:
                    return HandleInvalidMessage(message);
            }
        }

        private Task HandleLocalOperation(Message message)
        {
            throw new NotImplementedException();
        }

        private Task HandleInvalidMessage(Message message)
        {
            throw new NotImplementedException();
        }

        private Task HandleWebOperation(Message message)
        {
            throw new NotImplementedException();
        }
    }
}