using System;
using System.Linq;
using System.Xml.Linq;
using Bender.Configuration.Action;

namespace Bender.Configuration.Convert
{
    internal static class XRuleSource
    {
        public static JqlRule ToJqlRule(XElement src)
        {
            var rule = ToIssueRule<JqlRule>(src);
            rule.Jql = src.Element("jql")!.Value;
            return rule;
        }

        public static BuildRule ToBuildRule(XElement src)
        {
            //Contract.Requires<ArgumentNullException>(src.Attribute("projectCode") != null, "BuildRule.projectCode is null");
            var rule = ToIssueRule<BuildRule>(src);
            rule.Mask = src.Attribute("mask")!.Value;
            rule.RemainingDays = (int?)src.Attribute("remainingDays") ?? 0;
            rule.ExpiredOnly = (bool?)src.Attribute("expiredOnly") ?? false;
            rule.ProjectCode = src.Attribute("projectCode")!.Value;
            return rule;
        }

        public static IssueInclusionToStructRule ToInStructRule(XElement src)
        {
            var rule = ToIssueRule<IssueInclusionToStructRule>(src);
            rule.Structures = src.Element("structures")!.Value
                .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .ToArray();
            
            return rule;
        }

        public static TRule ToIssueRule<TRule>(XElement src) where TRule : Rule, new()
        {
            //Contract.Requires(!string.IsNullOrEmpty((string)src.Attribute("mailTo")), "Mandatory attribute 'mailTo' is absent in a rule");

            var rule = new TRule
            {
                AdditionalPredicateName = (string?) src.Attribute("additionalPredicate")
            };

            var notify = src.Element("notify");
            if (notify != null)
            {
                rule.HowToNotify = new Notify
                {
                    Subject = (string) notify.Attribute("subject")!,
                    Recommendations = (string?) notify.Attribute("recommendations"),

                    MetaAddressers = ((string) notify.Attribute("mailTo")!).ToLower()
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToArray(),

                    MetaCarbonCopy = (notify.Attribute("cc")?.Value ?? string.Empty).ToLower()
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToArray()
                };
            }

            var callRest = src.Element("callRest");
            if (callRest != null)
            {
                rule.HowToUpdate = new Update
                {
                    Verb = (string) callRest.Attribute("verb")!,
                    UrlPattern = (string)callRest.Attribute("urlPattern")!,
                    BodyPattern = (string?) callRest.Element("body")
                };
            }

            return rule;
        }
    }
}
