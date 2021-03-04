using System;
using System.Linq;
using JiraRest.Data;

namespace Bender.Data.Convert
{
    internal static class Service
    {
        public static User? ToUser(JiraRest.Data.User user)
        {
            return user == null ? null : new User {DisplayName = user.displayName, Email = user.emailAddress};
        }

        public static Issue ToIssue (JiraRest.Data.Issue issue)
        {
            return new Issue
            {
                Key = issue.key,
                Summary = issue.fields.summary,
                BuildFixed = issue.fields.fixVersions.Select(v => v.name).ToArray(),
                BuildFound = issue.fields.versions.Select(v => v.name).ToArray(),
                Components = string.Join(", ", issue.fields.components.Select(c => c.name)),
                Labels = string.Join(", ", issue.fields.labels),
                Priority = issue.fields.priority.name,
                Staff = new IssueStaff
                {
                    Assignee = ToUser(issue.fields.assignee),
                    Reporter = ToUser(issue.fields.reporter),
                    Creator = ToUser(issue.fields.creator),
                },
                Status = issue.fields.status.name,
                TimeSpent = TimeSpan.FromSeconds(issue.fields.timespent ?? 0),
                Type = issue.fields.issuetype.name,
                DueDate = issue.fields.duedate,
                CreatedDate = issue.fields.created
            };
        }
    }
}