using System.Threading.Tasks;

namespace RelayTask
{
    public interface ISubscriber
    {
        Task<bool> ReceiveMsg(Message message);
    }
}