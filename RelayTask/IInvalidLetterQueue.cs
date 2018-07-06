using System.Threading.Tasks;

namespace RelayTask
{
    public interface IInvalidLetterQueue
    {
        Task ReceiveMsg(Message msg);
    }
}