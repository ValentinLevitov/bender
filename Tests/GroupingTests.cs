using System;
using System.Linq;
using Bender.Configuration;
using Bender.Configuration.Action;
using Bender.Data;
using Bender.Data.Supplying;
using Bender.Data.Supplying.Convert;
using NUnit.Framework;
using Moq;
using Serilog;

namespace Tests
{
    [TestFixture]
    public class GroupingTests
    {
        [Test]
        public void GroupBuilds()
        {
            var jira = new Mock<Bender.IJiraService>();
            jira
                .Setup(j => j.GetBuilds(It.IsAny<string>()))
                .Returns(
                    new[]
                    {
                        new Build
                        {
                            Name = "9.0.0.1",
                            ReleaseDate = DateTime.Now.AddDays(1)
                        },
                        new Build
                        {
                            Name = "9.0.0.2",
                            ReleaseDate = DateTime.Now.AddDays(1)
                        }
                    }
                );

            var rules = new[]
            {
                new BuildRule
                {
                    Mask = @"^9\.0\.0\.1",
                    RemainingDays = 1,
                    HowToNotify = new Notify
                    {
                        Subject = "Subject",
                        MetaAddressers = new[] {"teammate1@express.ship", "teammate2@express.ship"},
                        MetaCarbonCopy = new[] {"teammate1@express.ship", "teammate2@express.ship"}
                    }
                },
                new BuildRule
                {
                    Mask = @"^9\.0\.0\.2",
                    RemainingDays = 1,
                    HowToNotify = new Notify
                    {
                        Subject = "Subject",
                        MetaAddressers = new[] {"teammate2@express.ship", "teammate1@express.ship"},
                        MetaCarbonCopy = new[] {"teammate2@express.ship", "teammate1@express.ship"}
                    }
                },
                new BuildRule
                {
                    Mask = @"^9\.0\.0\.2",
                    RemainingDays = 1,
                    HowToNotify = new Notify
                    {
                        Subject = "DifferentSubject",

                        MetaAddressers = new[] {"teammate2@express.ship", "teammate1@express.ship"},
                        MetaCarbonCopy = new[] {"teammate2@express.ship", "teammate1@express.ship"}
                    }
                }
            };

            var packages = new BuildSupplier(jira.Object, rules).GetPackages().Cast<Package<BenderSendsLetter, Build>>().ToArray();
            Assert.AreEqual(2, packages.Count());
            Assert.AreEqual(2, packages.Single(p => p.Reaction.Subject == "Subject").Items.Count());
            var messages = new BuildPackageConverter().ToMessages(packages);
            Assert.AreEqual(2, messages.Count());
            var actualBody1 = messages.First().Body;
            var actualBody2 = messages.ElementAt(1).Body;

            var tomorrow = DateTime.Now.AddDays(1).ToString("dd.MM.yyyy");

            var expectedBody1 = $@"<h3>Subject</h3>
<table border=""1"" cellspacing=""0"" cellpadding=""4"">
	<tr align=""center"" bgcolor=""#999999"">
		<td>Name</td>
		<td>StartDate</td>
		<td>ReleaseDate</td>
		<td>Description</td>
	</tr>
	<tr>
		<td>9.0.0.1</td>
		<td>empty</td>
		<td><font color=""red""><b>{tomorrow}<b></font></td>
		<td></td>
	</tr>
	<tr>
		<td>9.0.0.2</td>
		<td>empty</td>
		<td><font color=""red""><b>{tomorrow}<b></font></td>
		<td></td>
	</tr>
</table>
";
            var expectedBody2 = $@"<h3>DifferentSubject</h3>
<table border=""1"" cellspacing=""0"" cellpadding=""4"">
	<tr align=""center"" bgcolor=""#999999"">
		<td>Name</td>
		<td>StartDate</td>
		<td>ReleaseDate</td>
		<td>Description</td>
	</tr>
	<tr>
		<td>9.0.0.2</td>
		<td>empty</td>
		<td><font color=""red""><b>{tomorrow}<b></font></td>
		<td></td>
	</tr>
</table>
";

            Assert.AreEqual(expectedBody1, actualBody1);
            Assert.AreEqual(expectedBody2, actualBody2);
        }

        [Test]
        public void GroupIssues()
        {
            var jira = new Mock<Bender.IJiraService>();
            jira
                .Setup(j => j.GetIssuesForJql(It.IsAny<string>()))
                .Returns(
                    new[]
                    {
                        new Issue
                        {
                            BuildFixed = new[] {"1"},
                            BuildFound = new string[] {},
                            Staff = new IssueStaff
                                    {
                                        Assignee = null
                                    }
                        }
                    }
                );

            var rules = new[]
            {
                new JqlRule
                {
                    HowToNotify = new Notify
                    {
                        Subject = "Subject",
                        MetaAddressers = new[] {"teammate1@express.ship", "teammate2@express.ship"},
                        MetaCarbonCopy = new[] {"teammate1@express.ship", "teammate2@express.ship"}
                    }
                },
                new JqlRule
                {
                    HowToNotify = new Notify
                    {
                        Subject = "Subject",
                        MetaAddressers = new[] {"teammate1@express.ship", "teammate2@express.ship"},
                        MetaCarbonCopy = new[] {"teammate2@express.ship", "teammate1@express.ship"}
                    }
                },
                new JqlRule
                {
                    HowToNotify = new Notify
                    {
                        Subject = "DifferentSubject",
                        MetaAddressers = new[] {"teammate1@express.ship", "teammate2@express.ship"},
                        MetaCarbonCopy = new[] {"teammate1@express.ship", "teammate2@express.ship"}
                    }
                }
            };

            var logger = new Mock<ILogger>();

            var packages = new JqlSupplier(jira.Object, rules, logger.Object)
                .GetPackages()
                .Cast<Package<BenderSendsLetter, Issue>>()
                .ToArray();

            Assert.AreEqual(2, packages.Count());
            Assert.AreEqual(2, packages.Single(p => p.Reaction.Subject == "Subject").Items.Count());
        }
    }
}
