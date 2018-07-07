using System.Threading.Tasks;

namespace RelayTask.Abstract
{
    public interface IInvalidLetterQueue
    {
        Task ReceiveMsg(Message msg);
    }
}