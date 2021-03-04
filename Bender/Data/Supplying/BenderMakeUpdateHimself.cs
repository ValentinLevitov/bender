using static System.String;

namespace Bender.Data.Supplying
{
    internal class BenderMakesUpdateHimself //: BendersReaction
    {
        public string Verb { get; set; } = Empty;
        public string UrlPattern { get; set; } = Empty;
        public string? BodyPattern { get; set; }

    }
}