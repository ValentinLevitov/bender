using static System.String;

namespace Bender.Configuration
{
    public class BuildRule : Rule
    {
        public int RemainingDays { get; set; }
        /// <summary>
        /// Contains regex pattern.
        /// </summary>
        public string Mask { get; set; } = Empty;
        public bool ExpiredOnly { get; set; }
        public string ProjectCode { get; set; } = Empty;
    }
}
