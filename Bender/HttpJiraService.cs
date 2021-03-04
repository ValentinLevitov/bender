using System;
using System.Collections.Generic;
using JiraRest;
using Bender.Data;
using System.Linq;
using Bender.Data.Convert;

namespace Bender
{
    public class HttpJiraService : IJiraService, IHttpHandler
    {
        public int MaxIssueCount { get; set; } = 50;
        internal Connection Connection { get; set; }

        public HttpJiraService(string rootUri, string user, string password)
        {
            Connection = new Connection(rootUri, user, password);
        }

        public virtual Issue[] GetIssuesForJql(string query)
        {
            return CallFuncInConnectionContext(
                jira =>
                ((IEnumerable<dynamic>)
                 jira
                     .GetIssuesFromJql(query, MaxIssueCount == 0 ? null : (int?)MaxIssueCount)
                     .issues)
                    .Select(JToken.ToIssue)
                    .ToArray()
                );
        }

        public virtual string[] GetIssuesInStructure(string structId)
        {
            return CallFuncInConnectionContext(jira => jira.GetIssuesInStructure(structId));
        }

        public virtual Issue GetIssueById(string issueId)
        {
            return CallFuncInConnectionContext(jira => JToken.ToIssue(jira.GetIssue(issueId)));
        }

        public virtual Attachment[] GetIssueAttachments(string issueKey)
        {
            return CallFuncInConnectionContext(
                jira =>
                ((IEnumerable<dynamic>)
                 jira
                     .GetIssue(issueKey)
                     .fields
                     .attachment)
                    .Select(JToken.ToAttachment)
                    .ToArray()
                );
        }

        public virtual Build[] GetBuilds(string projectCode)
        {
            return CallFuncInConnectionContext(jira =>
                ((IEnumerable<dynamic>)
                    jira.GetBuilds(projectCode))
                    .Select(JToken.ToBuild)
                    .Where(b => b != null)
                    .Cast<Build>()
                    .ToArray()
                );
        }

        private T CallFuncInConnectionContext<T>(Func<Connection, T> func)
        {
            return func(Connection);
            //using (var jira = new Connection(_rootUri, _user, _password))
            //{
            //    return func(jira);
            //}
        }

        public virtual void HandleAll(IEnumerable<HttpRequest> requests)
        {
            foreach (var request in requests)
            {
                Connection.HandleRequest(request);
            }
        }
    }
}