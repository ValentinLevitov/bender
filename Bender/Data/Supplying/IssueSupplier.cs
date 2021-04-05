using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bender.Configuration;

namespace Bender.Data.Supplying
{
    internal abstract class IssueSupplier<TRule> : IPackageSupplier
        where TRule : Rule 
    {
        protected IJiraService JiraService { get; }
        protected IEnumerable<TRule> Rules { get; }

        protected IssueSupplier(IJiraService jiraService, IEnumerable<TRule> rules)
        {
            Rules = rules;
            JiraService = jiraService;
        }

        protected bool IsIssueInAccordanceWithPredicate(string additionalPredicateName, Issue issue)
        {
            return (bool) typeof (ExtendedFilteringPredicates)!
                .GetMethod(additionalPredicateName, BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] {JiraService, issue})!;
        }

        protected string ReplaceMarkersByRealAddresses(string[] metaAddressees, IssueStaff staff)
        {
            var markedStaff = new Dictionary<string, User?>
                          {
                              {"assignee", staff.Assignee},
                              {"reporter", staff.Reporter},
                              {"creator", staff.Creator}
                          };
                          //.ToCaseInsensitiveDictionary();

            var defaultUser = new User();
            
            string GetUserMail(User? u) => (u ?? defaultUser).Email.ToLower();

            return string.Join(",", metaAddressees
                .Where(m => !markedStaff.ContainsKey(m))
                .Union(metaAddressees
                        .Where(markedStaff.ContainsKey)
                        .Select(m => GetUserMail(markedStaff[m])))
                .Distinct()
                .OrderBy(m => m)
                .ToArray());
        }

        public virtual PackageBase[] GetPackages()
        {
            var uncategorizedSet =
            (
                from rule in Rules
                let issues =
                (
                    from issue in GetIssues(rule)
                    where string.IsNullOrWhiteSpace(rule.AdditionalPredicateName)
                          || IsIssueInAccordanceWithPredicate(rule.AdditionalPredicateName, issue)
                    select issue
                ).ToArray()
                where issues.Any()
                select new
                {
                    rule,
                    issues
                }
            ).ToArray();

            var notificationPackages =
                (
                    from set in uncategorizedSet
                    from issue in set.issues
                    where set.rule.HowToNotify != null
                    group new {issue, set.rule} by new
                    {
                        To = ReplaceMarkersByRealAddresses(set.rule.HowToNotify!.MetaAddressers, issue.Staff),
                        Cc = ReplaceMarkersByRealAddresses(set.rule.HowToNotify.MetaCarbonCopy, issue.Staff),
                        set.rule.HowToNotify.Subject
                    }
                    into ag
                    let basePackage = new Package<BenderSendsLetter, Issue>
                    {
                        Items = ag.Select(a => a.issue).ToArray(),
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
                    }
                    select Enrich(basePackage, ag.First().rule)
                )
                .ToArray();

            var actionPackages =
                (
                    from set in uncategorizedSet
                    from updateAction in set.rule.HowToUpdate
                    let package = new Package<BenderMakesUpdateHimself, Issue>
                    {
                        Reaction = new BenderMakesUpdateHimself
                        {
                            BodyPattern = updateAction.BodyPattern,
                            UrlPattern = updateAction.UrlPattern,
                            Verb = updateAction.Verb
                        },
                        Items = set.issues
                    }
                    select package
                )
                .Cast<PackageBase>()
                .ToArray();

            return notificationPackages.Union(actionPackages).ToArray();

        }

        protected internal virtual PackageBase Enrich(PackageBase basePackage, TRule rule)
        {
            return basePackage;
        }

        protected abstract Issue[] GetIssues(TRule rule);
    }
}
