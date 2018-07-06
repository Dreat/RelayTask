using System.Threading.Tasks;

namespace RelayTask
{
    public interface IDeadLetterQueue
    {
        Task ReceiveMsg(Message msg);
    }
}