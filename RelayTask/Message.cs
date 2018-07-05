namespace RelayTask
{
    public class Message
    {
        public MessageType MessageType { get; set; }
        public int CorrelationId { get; set; }
    }

    public enum MessageType
    {
        LocalOperation,
        WebOperation,
        NotHandled
    }
}