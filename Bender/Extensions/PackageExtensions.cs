using System.Collections.Generic;
using Messaging;
using JiraRest;
using Bender.Data.Supplying;
using Bender.Data.Supplying.Convert;

namespace Bender.Extensions
{
    internal static class PackageExtensions
    {
        public static IEnumerable<Message> ToMessages<TIssueType>(this IEnumerable<Package<BenderSendsLetter, TIssueType>> packages, 
            IPackageConverter<TIssueType> converter)
        {
            return converter.ToMessages(packages);
        }
        public static IEnumerable<HttpRequest> ToHttpRequests<TIssueType>(this IEnumerable<Package<BenderMakesUpdateHimself, TIssueType>> packages,
            IPackageConverter<TIssueType> converter)
        {
            return converter.ToHttpRequests(packages);
        }
    }
}
