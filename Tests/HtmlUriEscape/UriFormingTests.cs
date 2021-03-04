using System;
using NUnit.Framework;
using UriBuilder = JiraRest.UriBuilder;

namespace Tests.HtmlUriEscape
{
    [TestFixture]
    public class UriFormingTests
    {
        [Test, Description("Ensure correct escaping of spaces, braces, quotas, slashes, cyrillic symbols")]
        public void EnsureCorrectUriEscaping()
        {
            const string JQL = @"project=bender and type in (bug, task) and 'Причина возникновения' in (New, 'QA - New') and issue in structure(""Release 2"", ""ancestor in [issueFunction in issueFieldMatch('project=bender and type=epic', 'summary', '^R\\\\d+\\\\. Дефекты$')]"")";

            var actualUri =
                new UriBuilder()
                    .SetRoot("https://jira.example.com/jira/")
                    .AddRelativePath("rest/api/2/search")
                    .AddParam("jql", JQL, true)
                    .AddParam("maxResults", 50)
                    .Build()
                ;
                
            var expectedUri =
                new Uri(
                    "https://jira.example.com/jira/rest/api/2/search?jql=project%3Dbender%20and%20type%20in%20%28bug%2C%20task%29%20and%20%27%D0%9F%D1%80%D0%B8%D1%87%D0%B8%D0%BD%D0%B0%20%D0%B2%D0%BE%D0%B7%D0%BD%D0%B8%D0%BA%D0%BD%D0%BE%D0%B2%D0%B5%D0%BD%D0%B8%D1%8F%27%20in%20%28New%2C%20%27QA%20-%20New%27%29%20and%20issue%20in%20structure%28%22Release%202%22%2C%20%22ancestor%20in%20%5BissueFunction%20in%20issueFieldMatch%28%27project%3Dbender%20and%20type%3Depic%27%2C%20%27summary%27%2C%20%27%5ER%5C%5C%5C%5Cd%2B%5C%5C%5C%5C.%20%D0%94%D0%B5%D1%84%D0%B5%D0%BA%D1%82%D1%8B%24%27%29%5D%22%29&maxResults=50");
            Assert.AreEqual(expectedUri, actualUri);

        }
    }
}
