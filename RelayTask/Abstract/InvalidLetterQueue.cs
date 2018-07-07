using System.Collections.Generic;
using System.Threading.Tasks;

namespace RelayTask.Abstract
{
    public class InvalidLetterQueue : IInvalidLetterQueue
    {
        // I decided not to go with ConcurrentQueue as we will only write to this Queue during the "usual" system lifecycle
        // It will be read from only to check for invalid messages during the tests
        // Of course in real applications this would be handled in some way
        public readonly Queue<Message> InvalidMessaged = new Queue<Message>();

        public Task ReceiveMsg(Message msg)
        {
            InvalidMessaged.Enqueue(msg);
            return Task.CompletedTask;
        }
    }
}