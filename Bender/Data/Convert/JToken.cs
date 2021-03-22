using System;
using System.Collections.Generic;
using System.Linq;

namespace Bender.Data.Convert
{
    internal static class JToken
    {
        public static Build? ToBuild(dynamic build)
        {
            return build == null ? null
            : new Build
                {
                    Description = (string) build.description,
                    Name = (string) build.name,
                    Archived = (bool) build.archived,
                    Released = (bool) build.released,
                    StartDate = (DateTime?) build.startDate,
                    ReleaseDate = (DateTime?) build.releaseDate
                };
        }

        public static User? ToUser(dynamic user)
        {
            return user == null ? null :
                new User 
                {
                    DisplayName = user.displayName,
                    Email = user.emailAddress,
                    Name = user.name,
                    Key = user.key
                };
        }

        public static Issue ToIssue(dynamic issue)
        {
            return new Issue
            {
                Key = (string)issue.key,
                Summary = (string)issue.fields.summary,

                BuildFixed = issue.fields.fixVersions == null ? new string[] { } :
                    ((IEnumerable<dynamic>)issue.fields.fixVersions).Select(v => (string)v.name).ToArray(),

                BuildFound = issue.fields.versions == null ? new string[] { } :
                    ((IEnumerable<dynamic>)issue.fields.versions).Select(v => (string)v.name).ToArray(),

                Components = string.Join(", ", ((IEnumerable<dynamic>)issue.fields.components ??
                                Newtonsoft.Json.Linq.JArray.Parse("[]")).Select(c => c.name)),

                Labels = string.Join(", ", issue.fields.labels ?? string.Empty),

                Priority = (issue.fields.priority != null ? 
                    issue.fields.priority : 
                    Newtonsoft.Json.Linq.JToken.FromObject(new {name = (string?)null})
                  ).name,

                Staff = new IssueStaff
                {
                    Assignee = JToken.ToUser(issue.fields.assignee),
                    Reporter = JToken.ToUser(issue.fields.reporter),
                    Creator = JToken.ToUser(issue.fields.creator),
                },

                Status = issue.fields.status.name,
                TimeSpent = TimeSpan.FromSeconds(((double?)issue.fields.timespent) ?? 0),
                Type = issue.fields.issuetype.name,
                DueDate = (DateTime?)issue.fields.duedate,
                CreatedDate = (DateTime)issue.fields.created
            };
        }

        public static Attachment ToAttachment(dynamic attachment)
        {
            return new Attachment
            {
                Filename = (string)attachment.filename
            };
        }
    }
}
