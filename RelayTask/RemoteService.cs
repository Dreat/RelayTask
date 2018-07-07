using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RelayTask.Abstract;

namespace RelayTask
{
    public class RemoteService : IRemoteService
    {
        public Task<HttpStatusCode> ReceiveMsg(Message message)
        {
            // Let's assume this RemoteService will always succeed
            // And will take some time in completing web operation
            Thread.Sleep(500);
            return Task.FromResult(HttpStatusCode.OK);
        }
    }
}