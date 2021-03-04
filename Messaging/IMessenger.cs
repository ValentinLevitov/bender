using System.Collections.Generic;

namespace Messaging
{
    public interface IMessenger
    {
        void Send(Message data);
        void SendAll(IEnumerable<Message> messages);
    }
}