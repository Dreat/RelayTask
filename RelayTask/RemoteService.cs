using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RelayTask.Abstract;
using RelayTask.Helpers;

namespace RelayTask
{
    public class RemoteService : IRemoteService, IPrinter
    {
        public readonly Queue<Message> HandledMessages = new Queue<Message>();

        public Task<HttpStatusCode> ReceiveMsg(Message message)
        {
            HandledMessages.Enqueue(message);
            // Let's assume this RemoteService will always succeed
            // And will take some time in completing web operation
            Thread.Sleep(500);
            return Task.FromResult(HttpStatusCode.OK);
        }

        public void Print()
        {
            Console.WriteLine($"Number of messages handled by RemoteService: {HandledMessages.Count}");
        }
    }
}