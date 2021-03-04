using static System.String;

namespace Bender.Data.Supplying
{
    internal class BenderSendsLetter
    {
        public Addressees Addressees { get; set; } = new Addressees();
        public string Subject { get; set; } = Empty;
        public string? Recommendations { get; set; }
    }
}
