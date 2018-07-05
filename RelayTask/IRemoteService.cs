using System.Net;
using System.Threading.Tasks;

namespace RelayTask
{
    public interface IRemoteService
    {
        Task<HttpStatusCode> ReceiveMsg(Message message);
    }
}