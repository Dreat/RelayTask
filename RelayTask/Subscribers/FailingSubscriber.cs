using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RelayTask.Messages;
using RelayTask.Subscribers.Abstract;

namespace RelayTask.Subscribers
{
    // To show resend and DeadLetterQueue feature I've made FailingSubscriber
    // I will always return false/500 status code to indicate that Relay should try to resend message
    // After some tries relay will redirect message to DeadLetterQueue instead
    public class FailingSubscriber : ISubscriber, IRemoteService
    {
        Task<bool> ISubscriber.ReceiveMsg(Message message)
        {
            Thread.Sleep(100);
            return Task.FromResult(false);
        }

        Task<HttpStatusCode> IRemoteService.ReceiveMsg(Message message)
        {
            Thread.Sleep(500);
            return Task.FromResult(HttpStatusCode.InternalServerError);
        }
    }
}