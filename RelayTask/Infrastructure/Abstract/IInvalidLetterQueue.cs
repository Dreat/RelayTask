using System.Threading.Tasks;
using RelayTask.Messages;

namespace RelayTask.Infrastructure.Abstract
{
    public interface IInvalidLetterQueue
    {
        Task ReceiveMsg(Message msg);
    }
}