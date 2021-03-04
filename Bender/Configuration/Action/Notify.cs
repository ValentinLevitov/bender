using static System.String;
namespace Bender.Configuration.Action
{
    public class Notify
    {
        public string Subject { get; set; } = Empty;

        public string? Recommendations { get; set; }
        public string[] MetaAddressers { get; set; } = new string[]{};
        public string[] MetaCarbonCopy { get; set; } = new string[]{};

    }
}