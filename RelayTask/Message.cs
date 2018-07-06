namespace RelayTask
{
    public class Message
    {
        public MessageType MessageType { get; set; }
        public string CorrelationId { get; set; }
        public string Command { get; set; }
    }
}