using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RelayTask.Helpers;
using RelayTask.Messages;
using RelayTask.Subscribers.Abstract;

namespace RelayTask.Subscribers
{
    public class Subscriber : ISubscriber, IPrinter
    {
        public readonly Queue<Message> HandledMessages = new Queue<Message>();

        public Task<bool> ReceiveMsg(Message message)
        {
            HandledMessages.Enqueue(message);
            // Let's assume this Subscriber will always succeed
            // And will take little time in completing operations
            Thread.Sleep(100);
            return Task.FromResult(true);
        }

        public void Print()
        {
            Console.WriteLine($"Number of messages handled by subscriber: {HandledMessages.Count}");
        }
    }
}