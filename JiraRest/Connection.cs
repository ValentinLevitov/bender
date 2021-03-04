using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

namespace JiraRest
{
    public class Connection : IDisposable
    {
        public ISerializer Serializer { get; set; } = new NewtownsoftSerializer();

        // how to mock HttpClient: http://stackoverflow.com/questions/22223223/how-to-pass-in-a-mocked-httpclient-in-a-net-test
        internal HttpClient Client { get; set; } = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        internal Uri RootUri { get; }

        public Connection(string rootUri, string user, string password)
        {
            RootUri = new Uri($"{rootUri}{(rootUri.EndsWith("/") ? string.Empty : "/")}");
            var cred = Encoding.UTF8.GetBytes($"{user}:{password}");
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(cred));
        }

        public dynamic GetIssuesFromJql(string query, int? maxResults, bool includeHistory = false)
        {
            var actualUri =
                new UriBuilder()
                    .SetRoot(RootUri)
                    .AddRelativePath("rest/api/2/search")
                    .AddParam("jql", query, true)
                    .AddParam("maxResults", maxResults ?? int.MaxValue) // Jira API returns 50 items if the parameter is not set
                    .AddParamIf(includeHistory, "expand", "changelog")
                ;

            return Decode<dynamic>(actualUri.Build());
        }

        public dynamic GetIssueWorklogs(string issueKey)
        {
            //https://jira.example.com/jira/rest/api/2/issue/BENDER-3384/worklog
            var uri =
                new Uri(RootUri,
                        string.Format("rest/api/2/issue/{0}/worklog", issueKey));
            return Decode<dynamic>(uri);
        }

        public dynamic GetIssueComments(string issueKey)
        {
            //https://jira.example.com/jira/rest/api/2/issue/BENDER-3384/comment
            var uri =
                new Uri(RootUri,
                        string.Format("rest/api/2/issue/{0}/comment", issueKey));
            return Decode<dynamic>(uri);
        }

        public dynamic GetIssue(string issueKey)
        {
            //https://jira.example.com/jira/rest/api/2/issue/BENDER-3384
            var uri =
                new Uri(RootUri,
                        string.Format("rest/api/2/issue/{0}", issueKey));
            return Decode<dynamic>(uri);
        }

        public dynamic GetBuilds(string projectCode)
        {
            //https://jira.example.com/jira/rest/api/2/project/BENDER/versions
            var uri = new Uri(RootUri, $"rest/api/2/project/{projectCode}/versions");
            return Decode<dynamic>(uri);
        }

        public string[] GetIssuesInStructure(string structId)
        {
            //https://jira.example.com/jira/rest/structure/1.0/structure/{0}/forest - old style
            //https://jira.example.com/jira/rest/structure/2.0/forest/latest?s={%22structureId%22:576} - new style
            var uri =
                new Uri(RootUri,
                    $"rest/structure/2.0/forest/latest?s={{%22structureId%22:{structId}}}");

            return ((string)Decode<dynamic>(uri).formula)
                .Split(',')
                .Select(r => r.Split(':').Last())
                .Distinct()
                .ToArray()
                ;
        }

        public string GetPureResponse(Uri url)
        {
            return HandleResponse(Client.GetAsync(url), url);
        }

        private static string HandleResponse(Task<HttpResponseMessage> responseTask, Uri url)
        {
            var responseText =
                    responseTask
                        .ContinueWith(task =>
                        {
                            var httpResponse = task.WaitAndUnwrapException();
                            var txt = httpResponse.Content.ReadAsStringAsync().WaitAndUnwrapException();
                            try
                            {
                                httpResponse.EnsureSuccessStatusCode();
                                return txt;
                            }
                            catch (HttpRequestException ex)
                            {
                                throw new InvalidOperationException($"Server returns error '{txt}' from url '{url}'", ex);
                            }
                        })
                        .WaitAndUnwrapException()
                ;
            return responseText;
        }

        private T Decode<T>(Uri uri)
        {
            return (Serializer ?? new CustomSerializer()).Deserialize<T>(GetPureResponse(uri));
        }

        public void Dispose()
        {
            Client.Dispose();
        }

    public void HandleRequest(HttpRequest r)
        {
            //Debug.Write(r.Body);

            var content = new StringContent(r.Body, Encoding.UTF8, "application/json");

            switch (r.Verb)
            {
                case "PUT":
                    HandleResponse(Client.PutAsync(r.Uri, content), r.Uri);
                    break;
                case "POST":
                    HandleResponse(Client.PostAsync(r.Uri, content), r.Uri);
                    break;
                default:
                    throw new NotSupportedException($"Method {r.Verb} is not supported");

            }
        }
    }
}
