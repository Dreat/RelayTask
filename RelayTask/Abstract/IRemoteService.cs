using System.Net;
using System.Threading.Tasks;

namespace RelayTask.Abstract
{
    public interface IRemoteService
    {
        Task<HttpStatusCode> ReceiveMsg(Message message);
    }
}