using System.Collections.Generic;
using System.Threading.Tasks;
using RelayTask.Abstract;

namespace RelayTask
{
    public class DeadLetterQueue : IDeadLetterQueue
    {
        // I decided not to go with ConcurrentQueue as we will only write to this Queue during the "usual" system lifecycle
        // It will be read from only to check for dead messages during the tests, which will be artificially created
        // Of course in real applications this would be handled in some way
        public readonly Queue<Message> DeadMessages = new Queue<Message>();

        public Task ReceiveMsg(Message msg)
        {
            DeadMessages.Enqueue(msg);
            return Task.CompletedTask;
        }
    }
}