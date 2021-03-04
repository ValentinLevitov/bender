using System.Collections.Generic;
using System.Linq;
using Bender.Configuration;

namespace Bender.Data.Supplying
{
    internal class BuildSupplier : IPackageSupplier
    {
        protected IJiraService JiraService { get; }
        protected IEnumerable<BuildRule> Rules { get; }

        public BuildSupplier(IJiraService jiraService, IEnumerable<BuildRule> rules)
        {
            JiraService = jiraService;
            Rules = rules;
        }

        public PackageBase[] GetPackages()
        {
            string ToOrderedString(IEnumerable<string> a) => string.Join(",", a.OrderBy(c => c).ToArray());
            return (from rule in Rules
                    from build in JiraService.GetBuilds(rule.ProjectCode)
                    where rule.IsMatch(build)
                    group new {build, rule} by new
                    {
                        To = ToOrderedString(rule.HowToNotify!.MetaAddressers),
                        Cc = ToOrderedString(rule.HowToNotify.MetaCarbonCopy),
                        rule.HowToNotify.Subject
                    }
                    into ag
                    select new Package<BenderSendsLetter, Build>
                    {
                        Items = ag.Select(a => a.build).ToArray(),
                        Reaction = new BenderSendsLetter
                        {
                            Addressees = new Addressees
                            {
                                To = ag.Key.To.Split(','),
                                Cc = ag.Key.Cc.Split(',')
                            },
                            Subject = ag.Key.Subject,
                            Recommendations = ag.First().rule.HowToNotify!.Recommendations
                        }
                    })
                .Cast<PackageBase>()
                .ToArray();
        }
    }
}