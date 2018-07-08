using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RelayTask.Infrastructure.Abstract;
using RelayTask.Messages;
using RelayTask.Subscribers.Abstract;

namespace RelayTask
{
    public class Relay
    {
        private const int MaxTries = 5;
        private const uint BackpressureNotNeededTreshold = 2;
        private const uint BackpressureNeededTreshold = 5;
        private readonly IDeadMessageQueue _deadMessageQueue;
        private readonly IInvalidLetterQueue _invalidLetterQueue;
        private readonly List<IRemoteService> _remoteServices = new List<IRemoteService>();
        private readonly List<ISubscriber> _subscribers = new List<ISubscriber>();
        private uint _currentMessagesHandled = 0;

        public ConcurrentDictionary<string, Message> MessagesByCorrelationId = new ConcurrentDictionary<string, Message>();

        public Relay(IDeadMessageQueue deadMessageQueue, IInvalidLetterQueue invalidLetterQueue)
        {
            _deadMessageQueue = deadMessageQueue;
            _invalidLetterQueue = invalidLetterQueue;
        }

        public void RegisterSubscriber(ISubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void RegisterSubscribers(IEnumerable<ISubscriber> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                RegisterSubscriber(subscriber);
            }
        }

        public void RegisterRemoteService(IRemoteService remoteService)
        {
            _remoteServices.Add(remoteService);
        }

        public void RegisterRemoteServices(IEnumerable<IRemoteService> remoteServices)
        {
            foreach (var remoteService in remoteServices)
            {
                RegisterRemoteService(remoteService);
            }
        }

        public event EventHandler<bool> BackPressureNeeded;

        private void HandleLocalOperation(Message message)
        {
            // It will be more beneficial when we add more subscribers.
            // The longest case here is trying to resend MaxTries times
            // With regular foreach worst case is MaxTries * NumberOfSubcribers
            // And here we only care if the operation failed (so we need to resend) or not
            Parallel.ForEach(_subscribers, async subscriber =>
            {
                // I started on 1 for clarity reasons - that way we still have MaxTries iterations, but IF down below is more clear
                // if(i == MaxTries) instead of if(i == MaxTries - 1)
                for (var i = 1; i <= MaxTries; i++)
                {
                    var subscriberReceiveResult = await subscriber.ReceiveMsg(message);
                    // I assumed that if ReceiveMsg returns true, that means message delivery and handling succeeded
                    // If false is returned, I assume some error happened, so we have to try to resend a message
                    if (subscriberReceiveResult) break;

                    // This is the last loop iteration - we failed MaxTries times, so we assume that message cannot be delivered
                    // So it belongs in the Dead Letter Qeueue - place for messages that cannot be delivered
                    if (i == MaxTries) await _deadMessageQueue.ReceiveMsg(message);
                }
            });
        }

        private void HandleInvalidMessage(Message message)
        {
            // this is the message we cannot handle, so we send it to InvalidLetterQueue
            _invalidLetterQueue.ReceiveMsg(message);
        }

        private void HandleWebOperation(Message message)
        {
            // It will be more beneficial when we add more remoteServices.
            // The longest case here is trying to resend MaxTries times
            // With regular foreach worst case is MaxTries * NumberOfRemoteServices
            // And here we only care if the operation failed (so we need to resend) or not
            Parallel.ForEach(_remoteServices, async remoteService =>
            {
                // I started on 1 for clarity reasons - that way we still have MaxTries iterations, but IF down below is more clear
                // if(i == MaxTries) instead of if(i == MaxTries - 1)
                for (var i = 1; i <= MaxTries; i++)
                {
                    // I decided to check only 2 HttpStatuses as an example
                    // In real application I would use some method to determine valid range
                    // I assumed that anything other than succes HttpStatusCode means we need to try to resend the message
                    var remoteServiceReceiveResult = await remoteService.ReceiveMsg(message);
                    if (remoteServiceReceiveResult == HttpStatusCode.Accepted ||
                        remoteServiceReceiveResult == HttpStatusCode.OK) break;

                    // This is the last loop iteration - we failed MaxTries times, so we assume that message cannot be delivered
                    // So it belongs in the Dead Letter Qeueue - place for messages that cannot be delivered
                    if (i == MaxTries) await _deadMessageQueue.ReceiveMsg(message);
                }
            });
        }

        public void HandleMessagePublished(object sender, Message message)
        {
            new TaskFactory().StartNew(() =>
            {
                // I wanted to make Backpressure mechanism, so when Publisher sends more messages than Relay can process
                // The Relay raises an event to make Publisher stop
                // I know this is not the prettiest mechanism, but it's more to show concept
                _currentMessagesHandled++;
                if (_currentMessagesHandled >= BackpressureNeededTreshold) BackPressureNeeded?.Invoke(this, true);


                Console.WriteLine($"Current messages to process {_currentMessagesHandled}");

                // Generate correlationId and add it to dictionary
                message.CorrelationId = GenerateCorrelationId();
                while (!MessagesByCorrelationId.TryAdd(message.CorrelationId, message))
                { }

                // Artificially slow things down to check if backpressure works correctly
                // I WANT to block this thread, simulating some more complex operations
                Thread.Sleep(100);

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
                if (_currentMessagesHandled <= BackpressureNotNeededTreshold) BackPressureNeeded?.Invoke(this, false);
            });
        }

        private static string GenerateCorrelationId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}