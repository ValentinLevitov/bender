using System;
using System.Collections.Generic;
using System.Linq;
using Bender.Template;
using JiraRest;
using Bender.Extensions;

namespace Bender.Data.Supplying.Convert
{
    /// <summary>
    /// Converts Package to Mail body
    /// </summary>
    internal class IssuePackageConverter : PackageConverterBase<Issue>
    {
        private readonly string _rootUri;

        public IssuePackageConverter(string rootUri)
        {
            _rootUri = rootUri;
        }

        public override HttpRequest[] ToHttpRequests(IEnumerable<Package<BenderMakesUpdateHimself, Issue>> packages)
        {
            return
            (
                from package in packages
                from issue in package.Items
                select new HttpRequest
                {
                    Verb = package.Reaction.Verb,
                    Body = ReplaceKnownMarkers(package.Reaction.BodyPattern, issue),
                    Uri = new Uri(ReplaceKnownMarkers(package.Reaction.UrlPattern, issue))
                }
            ).ToArray();
        }

        private string ReplaceKnownMarkers(string? template, Issue issue)
        {
            return 
            (template ?? string.Empty)
                .Replace("{{@jiraRoot}}", _rootUri)
                .Replace("{{@issueKey}}", issue.Key)
                //.Replace("{{@assignee.email}}")
                //.Replace("{{@assignee.name}}")
                //.Replace("{{@assignee.displayName}}")
                //.Replace("{{@reporter.email}}")
                //.Replace("{{@reporter.name}}")
                //.Replace("{{@reporter.displayName}}")
                .EvaluateScriptingInjections()
                ;
        }

        protected internal override string StickThemesToSingleHtml(IEnumerable<Package<BenderSendsLetter, Issue>> packages)
        {
            return new IssuePackagesTemplate(packages, _rootUri).TransformText();
        }
    }
}