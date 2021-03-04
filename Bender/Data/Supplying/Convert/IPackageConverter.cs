using System.Collections.Generic;
using Messaging;
using JiraRest;

namespace Bender.Data.Supplying.Convert
{
    internal interface IPackageConverter<TIssueType> 
    {
        Message[] ToMessages(IEnumerable<Package<BenderSendsLetter, TIssueType>> packages);
        HttpRequest[] ToHttpRequests(IEnumerable<Package<BenderMakesUpdateHimself, TIssueType>> packages);
    }
}
