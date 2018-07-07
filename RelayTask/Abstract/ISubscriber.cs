using System.Threading.Tasks;

namespace RelayTask.Abstract
{
    public interface ISubscriber
    {
        Task<bool> ReceiveMsg(Message message);
    }
}