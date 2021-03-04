using System;
using System.Text.RegularExpressions;
using Bender.Data;

namespace Bender.Configuration
{
    internal static class RulesExtensions
    {
        public static bool IsMatch(this BuildRule buildRule, Build build)
        {
            Func<Build, int> getRemaingDays = b => (b.ReleaseDate.HasValue ? b.ReleaseDate.Value - DateTime.Now : TimeSpan.MaxValue).Days;
            Func<Build, bool> isExpired = b => getRemaingDays(b) < 0;
            Func<Build, bool> isFitToRemaingingDays = b => (getRemaingDays(b) - buildRule.RemainingDays <= 0);
            return !build.Archived
                   && !build.Released
                   && (isExpired(build) && buildRule.ExpiredOnly
                      || !isExpired(build) && !buildRule.ExpiredOnly && isFitToRemaingingDays(build))
                   && new Regex(buildRule.Mask).IsMatch(build.Name);
        }
    }
}
