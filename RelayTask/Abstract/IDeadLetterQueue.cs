using System.Threading.Tasks;

namespace RelayTask.Abstract
{
    public interface IDeadLetterQueue
    {
        Task ReceiveMsg(Message msg);
    }
}