using System;
using System.Collections.Generic;
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

using static System.String;

namespace End2End.Tests
{
	[TestFixture]
	public class BenderSendsNotificationsWithIssues
	{
		#region "Raw Data"
		private const string JsonIssueWithNullPriority = @"
{
	""issues"" : [{
			""expand"" : ""operations,versionedRepresentations,editmeta,changelog,transitions,renderedFields"",
			""key"" : ""BENDER-2301"",
			""fields"" : {
				""fixVersions"" : [],
				""resolution"" : null,
				""priority"" : null,
				""labels"" : [""collector""],
				""versions"" : [],
				""assignee"" : null,
				""status"" : {
					""name"" : ""Open""
				},
				""components"" : [{
						""name"" : ""Employee Profile & OrgChart""
					}
				],
				""reporter"" : {
					""displayName"" : ""John DiMaggio"",
				},
				""issuetype"" : {
					""name"" : ""Improvement""
				},
				""created"" : ""2017-02-08T07:04:57.000+0000"",
				""summary"" : ""Add option of editing order in which training courses are itemized""
			}
		}
	]
}
";

		private const string ExpectedNotificationWithEmptyPriority =
@"<h3>subject</h3>
<br>
<a href=""http://jira/issues/?jql=any"">JQL</a>
<br>
<table width=""100%"" border=""1"" cellspacing=""0"" cellpadding=""4"">
	<tr align=""center"" bgcolor=""#999999"">
		<td>Type</td>
		<td>Key</td>
		<td>Summary</td>
		<td>Assignee</td>
		<td>Reporter</td>
		<td>Status</td>
		<td>Priority</td>
		<td>Components</td>
		<td>Labels</td>
		<td>Time Spent (hrs)</td>
		<td>Build Found</td>
		<td>Build Fixed</td>
		<td>Due Date</td>
		<td>Created</td>
	</tr>
	<tr>
		<td>Improvement</td>
		<td><a href=""http://jira/browse/BENDER-2301"">BENDER-2301</a></td>
		<td>Add option of editing order in which training courses are itemized</td>
		<td>UNASSIGNED</td>
		<td>John DiMaggio</td>
		<td>Open</td>
		<td></td>
		<td>Employee Profile & OrgChart</td>
		<td>collector</td>
		<td>0</td>
		<td></td>
		<td></td>
		<td></td>
		<td>08.02.2017</td>
	</tr>
</table>
";

        private const string JsonIssueWithNullFixAndAffectedVersion = @"
{
    ""issues"": [
        {
            ""key"": ""BENDER-5753"",
            ""fields"": {
                ""labels"": [],
                ""assignee"": {
                    ""emailAddress"": ""Katey_Sagal@express.ship""
                },
                ""components"": [
                    {
                        ""name"": ""Sale: RFx""
                    }
                ]
            }
        }
    ]
}";

        #endregion

        [Test]
		public void IssueWithEmptyPriorityHandledWithoutException()
		{
			var response =
				new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(JsonIssueWithNullPriority)
				};

			var responseHandler = new Mock<DelegatingHandler>();
			responseHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(response));

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
				HowToNotify = new Notify
				{
					Subject = "subject",
					MetaAddressers = new[] { "assignee" },
					MetaCarbonCopy = new string[] { }
				}
			};

			var jqlSupplier = new JqlSupplier(svc, new[] { jqlRule }, new Mock<ILogger>().Object);

			var messenger = new Mock<IMessenger>();
			var pipe = new ReactionPipe<Issue>
			{
				PackageSupplier = jqlSupplier,
				PackageConverter = new IssuePackageConverter("http://jira"),
				Messenger = messenger.Object,
				HttpHandler = new Mock<IHttpHandler>().Object
			};
			pipe.Run();

			responseHandler
				.Protected()
				.Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
					ItExpr.Is<HttpRequestMessage>(
						r => r.RequestUri == new Uri("http://jira/rest/api/2/search?jql=any&maxResults=50")),
					ItExpr.IsAny<CancellationToken>());

			 messenger.Verify(m => m.SendAll(It.Is<IEnumerable<Message>>(
				msgs => ExpectedNotificationWithEmptyPriority.Equals(msgs.Single().Body))), Times.Once());
		}


		[Test]
		public void IssueWithAbsentFixAndAffectedVersionHandledWithoutException()
		{
			var response =
				new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(JsonIssueWithNullFixAndAffectedVersion)
				};

			var responseHandler = new Mock<DelegatingHandler>();
			responseHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(response));

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
				HowToNotify = new Notify
				{
					Subject = "subject",
					MetaAddressers = new[] { "assignee" },
					MetaCarbonCopy = new string[] { }
				}
			};

			var jqlSupplier = new JqlSupplier(svc, new[] { jqlRule }, new Mock<ILogger>().Object);

			var messenger = new Mock<IMessenger>();
			var pipe = new ReactionPipe<Issue>
			{
				PackageSupplier = jqlSupplier,
				PackageConverter = new IssuePackageConverter("http://jira"),
				Messenger = messenger.Object,
				HttpHandler = new Mock<IHttpHandler>().Object
			};
			pipe.Run();

			responseHandler
				.Protected()
				.Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
					ItExpr.Is<HttpRequestMessage>(
						r => r.RequestUri == new Uri("http://jira/rest/api/2/search?jql=any&maxResults=50")),
					ItExpr.IsAny<CancellationToken>());

		}
	}
}