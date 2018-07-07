using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RelayTask.Abstract;

namespace RelayTask
{
    // Let's assume this behaves like Subscriber and RemoveService
    // I was thinking about some random succes/failure, but it would make proper tests harder to write
    // With limited time I decided to go with 100% succes and 100% failure handlers to show both scenarios
    // After all, if we manage to resend, then we will have succes case with some delay
    public class ComplexSubscriber : ISubscriber, IRemoteService
    {
        Task<bool> ISubscriber.ReceiveMsg(Message message)
        {
            Thread.Sleep(100);
            return Task.FromResult(true);
        }

        Task<HttpStatusCode> IRemoteService.ReceiveMsg(Message message)
        {
            Thread.Sleep(500);
            return Task.FromResult(HttpStatusCode.OK);
        }
    }
}