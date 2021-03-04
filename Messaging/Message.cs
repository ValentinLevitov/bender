using static System.String;

namespace Messaging
{
    public class Message
    {
        public string To { get; set; } = Empty;
        public string Cc { get; set; } = Empty;
        public string Bcc { get; set; } = Empty;
        public string Subject { get; set; } = Empty;
        public string Body { get; set; } = Empty;
        public string Importance { get; set; } = Empty;
        public bool IsBodyHtml { get; set; }
        public string LogoFileName { get; set; } = Empty;
    }
}