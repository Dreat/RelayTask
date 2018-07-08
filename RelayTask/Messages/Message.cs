namespace RelayTask.Messages
{
    public class Message
    {
        public MessageType MessageType { get; set; }
        public string CorrelationId { get; set; }
        public string Command { get; set; }
    }
}