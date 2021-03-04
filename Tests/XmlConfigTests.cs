using System;
using System.Linq;
using System.Xml.Linq;
using Bender.Configuration;
using Moq;
using NUnit.Framework;
using Serilog;

namespace Tests
{
    [TestFixture]
    public class XmlConfigTests
    {
        [Test]
        public void GetIssueRules()
        {
            var xml =
@"<configuration>
    <request group=""test"">
        <notify subject=""test"" mailTo=""admin"" />
        <jql/>
    </request>
</configuration>";

            var config = new XmlRulesConfig(XDocument.Parse(xml), new Mock<ILogger>().Object);

            Rule rule = config.GetJqlRules("test").Single();
            Assert.AreEqual(string.Join(",", rule.HowToNotify!.MetaAddressers), string.Join(",", "admin"));
            Assert.AreEqual(rule.HowToNotify.Subject, "test");
        }

        [Test]
        public void GetBuildRules()
        {
            var xml =
@"<configuration>
    <build
      group=""test""
      mask=""some valid regex""
      projectCode=""BENDER""
      remainingDays=""2"">

        <notify subject=""test"" mailTo=""admin""/>
    </build>
</configuration>";

            var config = new XmlRulesConfig(XDocument.Parse(xml), new Mock<ILogger>().Object);

            var rule = config.GetBuildRules("test").Single();
            Assert.IsFalse(rule.ExpiredOnly);
            Assert.AreEqual(string.Join(",", rule.HowToNotify!.MetaAddressers), string.Join(",", new[] { "admin" }));
            Assert.AreEqual(rule.HowToNotify.Subject, "test");
            Assert.AreEqual(rule.ProjectCode, "BENDER");
        }

        [Test]
        public void GetInStructRules()
        {
            var xml =
@"<configuration>
    <structureAmbiguityRule
      group=""test"">

        <notify
          subject=""Task is present in more than one project structure. Remove it from others.""
          mailTo=""reporter""/>

        <structures>417,462,525,576</structures>
    </structureAmbiguityRule>
</configuration>";

            var config = new XmlRulesConfig(XDocument.Parse(xml), new Mock<ILogger>().Object);

            var rule = config.GetInStructRules("test").Single();
            Assert.AreEqual(string.Join(",", rule.Structures), "417,462,525,576");
            Assert.AreEqual(string.Join(",", rule.HowToNotify!.MetaAddressers), string.Join(",", new[] { "reporter" }));
            Assert.AreEqual(rule.HowToNotify.Subject, "Task is present in more than one project structure. Remove it from others.");
        }

        [Test]
        public void GetRedirectionRules()
        {
            var xml =
@"<configuration>
    <redirection_rules>
        <rule from=""Bender"" to=""Phillip""/>
    </redirection_rules>
</configuration>";

            var config = new XmlRulesConfig(XDocument.Parse(xml), new Mock<ILogger>().Object);

            var redirectionMap = config.GetRedirectionMap();
            string? to;
            Assert.IsTrue(redirectionMap.TryGetValue("Bender", out to));
            Assert.AreEqual(to, "Phillip");
        }

        [Test]
        public void CheckExceptionHandler()
        {
            // Required field 'mask' is absent
            var xml =
@"<configuration>
    <build
      group=""test""
      maskER=""some valid regex""
      projectCode=""BENDER""
      remainingDays=""2"">

        <notify subject=""test"" mailTo=""admin""/>
    </build>
</configuration>";

            var logger = new Mock<ILogger>();
            //logger.Setup(l => l.Error(It.IsAny<Exception>()));
                
                
            var config = new XmlRulesConfig(XDocument.Parse(xml), logger.Object);

            Assert.IsFalse(config.GetBuildRules("test").Any());
            logger.Verify(l => l.Error(It.IsNotNull<Exception>(), It.IsAny<string>()), Times.Once());
        }
    }
}
