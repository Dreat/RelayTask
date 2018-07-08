using System.Threading.Tasks;
using RelayTask.Messages;

namespace RelayTask.Subscribers.Abstract
{
    public interface ISubscriber
    {
        Task<bool> ReceiveMsg(Message message);
    }
}