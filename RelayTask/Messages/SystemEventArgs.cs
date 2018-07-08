using System;

namespace RelayTask.Messages
{
    // I decided that if the only responses are either bool or HttpStatus code
    // Those are the commands, so they're not returning anything more than information
    // wheather they failed or not
    // I also decided to use simple, generic string for command, just for presentation reasons
    public class SystemEventArgs : EventArgs
    {
        public MessageType MessageType { get; set; }
        public string Command { get; set; }
    }
}