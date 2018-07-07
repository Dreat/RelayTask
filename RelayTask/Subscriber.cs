using System.Threading;
using System.Threading.Tasks;
using RelayTask.Abstract;

namespace RelayTask
{
    public class Subscriber : ISubscriber
    {
        public Task<bool> ReceiveMsg(Message message)
        {
            // Let's assume this Subscriber will always succeed
            // And will take little time in completing operations
            Thread.Sleep(100);
            return Task.FromResult(true);
        }
    }
}