using System.Net;
using System.Threading.Tasks;
using RelayTask.Messages;

namespace RelayTask.Subscribers.Abstract
{
    public interface IRemoteService
    {
        Task<HttpStatusCode> ReceiveMsg(Message message);
    }
}