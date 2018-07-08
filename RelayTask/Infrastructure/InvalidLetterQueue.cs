using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RelayTask.Helpers;
using RelayTask.Infrastructure.Abstract;
using RelayTask.Messages;

namespace RelayTask.Infrastructure
{
    public class InvalidLetterQueue : IInvalidLetterQueue, IPrinter
    {
        // I decided not to go with ConcurrentQueue as we will only write to this Queue during the "usual" system lifecycle
        // It will be read from only to check for invalid messages during the tests
        // Of course in real applications this would be handled in some way
        public readonly Queue<Message> InvalidMessages = new Queue<Message>();

        public Task ReceiveMsg(Message msg)
        {
            InvalidMessages.Enqueue(msg);
            return Task.CompletedTask;
        }

        public void Print()
        {
            Console.WriteLine($"Messages in InvalidLetterQueue: {InvalidMessages.Count}");
        }
    }
}