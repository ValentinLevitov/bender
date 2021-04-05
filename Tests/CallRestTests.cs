using System.Collections.Generic;
using Bender;
using Bender.Configuration;
using Bender.Configuration.Action;
using Bender.Data;
using Bender.Data.Supplying;
using Bender.Data.Supplying.Convert;
using Bender.Notification;
using JiraRest;
using Moq;
using NUnit.Framework;
using Serilog;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class CallRestTests
    {
        [Test]
        public void CheckMoreThanOneCallRestAreCalled()
        {
            // Setup
            var rule = new JqlRule
            {
                HowToUpdate = new[]
                {
                    new Update {UrlPattern = "http://example.com"},
                    new Update {UrlPattern = "http://example.com"}
                }
            };

            var issuesSupplier = new Mock<IJiraService>();
            issuesSupplier
                .Setup(s => s.GetIssuesForJql(It.IsAny<string>()))
                .Returns(new[] {new Issue()})
                ;

            var jqlSupplier = new JqlSupplier(issuesSupplier.Object, new[] { rule }, new Mock<ILogger>().Object);

            var httpHandler = new Mock<IHttpHandler>();

            var pipe = new ReactionPipe<Issue>
            {
                PackageSupplier = jqlSupplier,
                PackageConverter = new IssuePackageConverter("http://jira"),
                HttpHandler = httpHandler.Object
            };

            // Experiment
            pipe.Run();

            // Check result
            httpHandler.Verify(h => h.HandleAll(It.Is<IEnumerable<HttpRequest>>(r => r.Count() == 2)));
            
        }
    }
}