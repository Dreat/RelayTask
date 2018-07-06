using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RelayTask
{
    public class Relay
    {
        private readonly List<IRemoteService> _remoteServices = new List<IRemoteService>();
        private readonly List<ISubscriber> _subscribers = new List<ISubscriber>();
        private readonly IDeadLetterQueue _deadLetterQueue;
        private readonly IInvalidLetterQueue _invalidLetterQueue;
        private const int MaxTries = 5;
        private uint _currentMessagesHandled = 0;
        private const uint BackpressureNotNeededTreshold = 5;
        private const uint BackpressureNeededTreshold = 30;

        public Relay(IDeadLetterQueue deadLetterQueue, IInvalidLetterQueue invalidLetterQueue)
        {
            _deadLetterQueue = deadLetterQueue;
            _invalidLetterQueue = invalidLetterQueue;
        }

        public void RegisterSubsriber(ISubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void RegisterRemoteService(IRemoteService remoteService)
        {
            _remoteServices.Add(remoteService);
        }

        public event EventHandler<bool> BackPressureNeeded;

        private Task HandleLocalOperation(Message message)
        {
            // It will be more beneficial when we add more subscribers.
            // The longest case here is trying to resend MaxTries times
            // With regular foreach worst case is MaxTries * NumberOfSubcribers
            // And here we only care if the operation failed (so we need to resend) or not
            Parallel.ForEach(_subscribers, subscriber =>
            {
                // I started on 1 for clarity reasons - that way we still have MaxTries iterations, but IF down below is more clear
                // if(i == MaxTries) instead of if(i == MaxTries - 1)
                for (var i = 1; i <= MaxTries; i++)
                {
                    var subscriberReceiveResult = subscriber.ReceiveMsg(message).Result;
                    // I assumed that if ReceiveMsg returns true, that means message delivery and handling succeeded
                    // If false is returned, I assume some error happened, so we have to try to resend a message
                    if (subscriberReceiveResult)
                    {
                        break;
                    }

                    // This is the last loop iteration - we failed MaxTries times, so we assume that message cannot be delivered
                    // So it belongs in the Dead Letter Qeueue - place for messages that cannot be delivered
                    if (i == MaxTries)
                    {
                        _deadLetterQueue.ReceiveMsg(message);
                    }
                }
            });

            return Task.CompletedTask;
        }

        private Task HandleInvalidMessage(Message message)
        {
            // this is the message we cannot handle, so we send it to InvalidLetterQueue
            return new TaskFactory().StartNew(() => _invalidLetterQueue.ReceiveMsg(message));
        }

        private Task HandleWebOperation(Message message)
        {
            // It will be more beneficial when we add more remoteServices.
            // The longest case here is trying to resend MaxTries times
            // With regular foreach worst case is MaxTries * NumberOfRemoteServices
            // And here we only care if the operation failed (so we need to resend) or not
            Parallel.ForEach(_remoteServices, remoteService =>
            {
                // I started on 1 for clarity reasons - that way we still have MaxTries iterations, but IF down below is more clear
                // if(i == MaxTries) instead of if(i == MaxTries - 1)
                for (var i = 1; i <= MaxTries; i++)
                {
                    // I decided to check only 2 HttpStatuses as an example
                    // In real application I would use some method to determine valid range
                    // I assumed that anything other than succes HttpStatusCode means we need to try to resend the message
                    var remoteServiceReceiveResult = remoteService.ReceiveMsg(message).Result;
                    if (remoteServiceReceiveResult == HttpStatusCode.Accepted || remoteServiceReceiveResult == HttpStatusCode.OK)
                    {
                        break;
                    }

                    // This is the last loop iteration - we failed MaxTries times, so we assume that message cannot be delivered
                    // So it belongs in the Dead Letter Qeueue - place for messages that cannot be delivered
                    if (i == MaxTries)
                    {
                        _deadLetterQueue.ReceiveMsg(message);
                    }
                }
            });

            return Task.CompletedTask;
        }

        public void HandleMessagePublished(object sender, Message message)
        {
            // I wanted to make Backpressure mechanism, so when Publisher sends more messages than Relay can process
            // The Relay raises an event to make Publisher stop
            // I know this is not the prettiest mechanism, but it's more to show concept
            _currentMessagesHandled++;
            if (_currentMessagesHandled >= BackpressureNeededTreshold)
            {
                BackPressureNeeded?.Invoke(this, true);
            }

            switch (message.MessageType)
            {
                case MessageType.WebOperation:
                    HandleWebOperation(message);
                    break;
                case MessageType.LocalOperation:
                    HandleLocalOperation(message);
                    break;
                default:
                    HandleInvalidMessage(message);
                    break;
            }

            // We release backpressure when there are fewer messages to process
            _currentMessagesHandled--;
            if (_currentMessagesHandled <= BackpressureNotNeededTreshold)
            {
                BackPressureNeeded?.Invoke(this, false);
            }
        }
    }
}