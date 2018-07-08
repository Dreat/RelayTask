using System.Threading.Tasks;
using RelayTask.Messages;

namespace RelayTask.Infrastructure.Abstract
{
    public interface IDeadMessageQueue
    {
        Task ReceiveMsg(Message msg);
    }
}