using System.Linq;
using Bender.Configuration;
using Bender.Configuration.Action;
using Bender.Data;
using Bender.Data.Supplying;
using NUnit.Framework;
using Moq;
using Serilog;

namespace Tests
{
    [TestFixture]
    public class FilteringPredicateTests
    {
        [Test]
        public void EnsurePredicateIsCalling()
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
                                Staff = new IssueStaff()
                            }
                        }                    
                );

            var rules = 
                new[]
                    {
                        new JqlRule
                        {
                            AdditionalPredicateName = "MoreThanOneFixVersion",
                            Jql = "any jql",
                            HowToNotify = new Notify
                            {
                                MetaAddressers = new[] {"1"},
                                MetaCarbonCopy = new string[] { },
                                Subject = "any subject",
                            }
                        }
                    };

            var logger = new Mock<ILogger>();
            var packages = new JqlSupplier(jira.Object, rules, logger.Object).GetPackages();
            Assert.IsFalse(packages.Any());
        }
   }
}