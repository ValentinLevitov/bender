using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Messaging;
using JiraRest;
using Bender;
using Bender.Configuration;
using Bender.Configuration.Action;
using Bender.Data;
using Bender.Data.Supplying;
using Bender.Data.Supplying.Convert;
using Bender.Notification;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Serilog;
using System.Xml.Linq;

using static System.String;

namespace End2End.Tests
{
    [TestFixture]
    public class BenderUpdatesIssuesHimself
    {
        #region "Raw Data"
        private const string JsonTypicalIssue =
@"{
	""issues"": [{
		""key"": ""BENDER-961"",
		""fields"": {
			""status"": {
				
			},
			""issuetype"": {
				
			},
			""created"": ""2018-08-14T13:50:59.000+0000"",
            ""assignee"": {
                ""name"": ""alice""
            },
            ""reporter"": {
                ""name"": ""bob""
            }
		}
	}]
}";

        #endregion

        [Test]
        public void EnsureRequestToUpdateFormedCorrectlyAndRan()
        {
            var responseWhenGetIssues = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonTypicalIssue)
            };

            var responseWhenDoPost = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("")
            };

            var responseHandler = new Mock<DelegatingHandler>();
            responseHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(responseWhenGetIssues))
                .Returns(Task.FromResult(responseWhenDoPost))
                ;

            var connection = new Connection("http://jira", "any", "any")
            {
                Client = new HttpClient(responseHandler.Object)
            };

            var svc = new HttpJiraService("http://jira", Empty, Empty)
            {
                Connection = connection
            };

            var jqlRule = new JqlRule
            {
                Jql = "any",
                HowToUpdate = new Update
                {
                    UrlPattern = "{{@jiraRoot}}/rest/api/2/issue/{{@issueKey}}/transitions",
                    BodyPattern = @"{
    ""update"": 
	{
        ""comment"": [
            {
                ""add"": {
                    ""body"": ""Issue {{@issueKey}} Closed automatically because of activity absence""
                }
            }
        ]
    },
  
    ""transition"": {
        ""id"": ""61""
    }
}",
                    Verb = "POST"
                }
            };

            var jqlSupplier = new JqlSupplier(svc, new[] { jqlRule }, new Mock<ILogger>().Object);

            var messenger = new Mock<IMessenger>();

            var pipe = new ReactionPipe<Issue>
            {
                PackageSupplier = jqlSupplier,
                PackageConverter = new IssuePackageConverter("http://jira"),
                Messenger = messenger.Object,
                HttpHandler = svc
            };
            pipe.Run();
            
            responseHandler
                .Protected()
                
                .Verify<Task<HttpResponseMessage>>("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => 
                            r.RequestUri == new Uri("http://jira/rest/api/2/issue/BENDER-961/transitions")
                             && r.Method == HttpMethod.Post
                            && r.Content!.ReadAsStringAsync().Result.Contains("Issue BENDER-961 Closed automatically because of activity absence")
                            
                    ),
                    ItExpr.IsAny<CancellationToken>());

        }

        [Test]
        public void CheckExpressionsEvaluationInUrlAndBody()
		{
			// Setup
			var package = new Package<BenderMakesUpdateHimself, Issue>()
			{
				Items = new[] { new Issue
                {
                     Key = "T-1"
				}
				},
				Reaction = new BenderMakesUpdateHimself
				{
					UrlPattern = "{{@jiraRoot}}/rest/api/<<c# \"{{@jiraRoot}}\".Contains(\"jiraeu\") ? \"2\" : \"1\" #>>/issue/{{@issueKey}}/transitions",
					BodyPattern = "datetime: \"<<c# new System.DateTime(2019, 03, 18).ToString(\"yyyy-MM-dd\") #>>\""
				}

			};
			var converter1 = new IssuePackageConverter("http://jiraeu");
			var converter2 = new IssuePackageConverter("http://jira");

			// Experiment
			var result1 = converter1.ToHttpRequests(new[] { package });
            var result2 = converter2.ToHttpRequests(new[] { package });

			// Check results
			Assert.AreEqual(
				"http://jiraeu/rest/api/2/issue/T-1/transitions",   // Expected
				result1.Single().Uri.ToString()                     // Actual
				);
			Assert.AreEqual(
				"http://jira/rest/api/1/issue/T-1/transitions",     // Expected
				result2.Single().Uri.ToString()                     // Actual
				);

			Assert.AreEqual(
				"datetime: \"2019-03-18\"",                         // Expected
				result1.Single().Body                               // Actual
				);
		}

		[Test]
		public void CheckMultilineExpressionEvaluation()
		{
			// Setup
			var package = new Package<BenderMakesUpdateHimself, Issue>()
			{
				Items = new[] { new Issue
				{
					 Key = "T-1"
				}
				},
				Reaction = new BenderMakesUpdateHimself
				{
					UrlPattern = "http://any",
					BodyPattern = @" ""datetime"":[{ ""<<c#(

						new System.DateTime(2020, 03, 20) +
						System.TimeSpan.FromDays(
							(new System.DateTime(2020, 03, 20)).Hour < 16 ? 0
							: System.DateTime.Today.DayOfWeek == System.DayOfWeek.Friday ? 3
							: 1
						)
                    )
                    .ToString(""yyyy-MM-dd"")#>>"" }]"
				}

			};
			var converter = new IssuePackageConverter(string.Empty);

			// Experiment
			var result = converter.ToHttpRequests(new[] { package });

			// Check results
			Assert.AreEqual(
				@" ""datetime"":[{ ""2020-03-20"" }]",                // Expected
				result.Single().Body                                 // Actual
				);
		}

        [Test]
        public void CheckAssigneeAndReporterReplacement()
        {
            // Setup
            var rule = 
@"<configuration>
  <jqlRule group=""test"">
    <jql>any</jql>
    <callRest verb=""PUT"" urlPattern=""https://jira.example.com/swap-assignee-and-reporter-where/?assignee={{@assignee.name}}&amp;reporter={{@reporter.name}}"">
                    <body><![CDATA[
                        {
                            ""update"": {
                                ""assignee"": [{""set"": {""name"": ""{{@reporter.name}}""}}],
                                ""reporter"": [{""set"": {""name"": ""{{@assignee.name}}""}}]
                            }
                        }
                    ]]>
                </body>
    </callRest>
  </jqlRule>
</configuration>";

            var rulesConfig = new XmlRulesConfig(XDocument.Parse(rule), new Mock<ILogger>().Object);

            var responseWhenGetIssues = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonTypicalIssue)
            };

            var responseWhenDoPut = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("")
            };

            var responseHandler = new Mock<DelegatingHandler>();
            responseHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(responseWhenGetIssues))
                .Returns(Task.FromResult(responseWhenDoPut))
                ;

            var connection = new Connection("http://jira", Empty, Empty)
            {
                Client = new HttpClient(responseHandler.Object)
            };

            var jiraService = new HttpJiraService("http://jira", Empty, Empty)
            {
                Connection = connection
            };

            var packageSupplier = new JqlSupplier(jiraService, rulesConfig.GetJqlRules("test"), new Mock<ILogger>().Object);
            var pipe = new ReactionPipe<Issue>()
                {
                    PackageSupplier = packageSupplier,
                    PackageConverter = new IssuePackageConverter(Empty),
                    HttpHandler = jiraService
                };

            // Experiment
            pipe.Run();

            // Check results
            responseHandler
                .Protected()
                .Verify<Task<HttpResponseMessage>>("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        r => 
                            r.RequestUri == new Uri("https://jira.example.com/swap-assignee-and-reporter-where/?assignee=alice&reporter=bob")
                                && r.Method == HttpMethod.Put
                                && r.Content!.ReadAsStringAsync().Result.Contains(@"""assignee"": [{""set"": {""name"": ""bob""}}]")
                                && r.Content!.ReadAsStringAsync().Result.Contains(@"""reporter"": [{""set"": {""name"": ""alice""}}]")
                    ),
                    ItExpr.IsAny<CancellationToken>());


        }
	}
}