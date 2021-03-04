using System.Collections.Generic;
using Messaging;
using JiraRest;

namespace Bender.Data.Supplying.Convert
{
    internal abstract class PackageConverterBase<TItemType> : IPackageConverter<TItemType> 
        
    {
        //[Microsoft.Practices.Unity.Dependency]
        public string SubjectPrefix { get; set; } = "[Jira] Unprocessed Issues ";

        public Message[] ToMessages(IEnumerable<Package<BenderSendsLetter, TItemType>> packages)
        {
            return Common<TItemType>.ToMessage(packages, StickThemesToSingleHtml, SubjectPrefix);
        }

        public abstract HttpRequest[] ToHttpRequests(
            IEnumerable<Package<BenderMakesUpdateHimself, TItemType>> packages);

        protected internal abstract string StickThemesToSingleHtml(IEnumerable<Package<BenderSendsLetter, TItemType>> packages);
    }
}