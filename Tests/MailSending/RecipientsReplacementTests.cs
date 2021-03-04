using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Bender.Configuration;
using Bender.Data;
using Bender.Data.Supplying;
using Bender.Data.Supplying.Convert;
using Bender.Extensions;
using Bender.Notification;
using Messaging;
using Moq;
using NUnit.Framework;
using Serilog;

namespace Tests.MailSending
{
    [TestFixture]
    public class RecipientsReplacementTests
    {
        private Bender.IJiraService? _jiraService;
        private IRulesConfig? _rulesConfig;
        private ILogger? _logger;

        [SetUp]
        public void Initialize()
        {
            var xmlConfig = XDocument.Parse(
@"<configuration>
    <jqlRule group=""test"">
        <notify 
            subject=""test""
            mailTo=""Admin,assignee,creator""
            cc=""reporter""
            />
        <jql/>
    </jqlRule>

    <jqlRule group=""test-2"">
        <notify 
            subject=""test""
            mailTo=""Team_Claim,reporter""
            />            
        <jql/>
    </jqlRule>

    <jqlRule group=""supervisor-in-To"">
        <notify 
            subject=""""
            mailTo=""supervisor""
            />            
        <jql/>
    </jqlRule>

    <jqlRule group=""supervisor-in-Cc"">
        <notify 
            subject=""""
            mailTo=""anybody""
            cc=""supervisor""
            />            
        <jql/>
    </jqlRule>

    <jqlRule group=""test-for-faired-supervisor"">
        <notify 
            subject=""""
            mailTo=""anybody""
            />            
        <jql/>
    </jqlRule>

    <jqlRule group=""test-for-empty-addressers"">
        <notify 
            subject=""""
            mailTo=""""
            comment=""When neither To nor Cc attributes are set, the message should be send to supervisors or (if supervisor is not set) to maintainers""
            />            
        <jql/>
    </jqlRule>

    <jqlRule group=""faired-suprevisor-in-addressees"">
        <notify 
            subject=""faired_supervisor""
            mailTo=""faired_supervisor""
            comment=""When neither To nor Cc attributes are set, the message should be send to supervisor""
            />            
        <jql/>
    </jqlRule>

    <redirection_rules>
        <rule from=""admin"" to=""administrator""/>
        <rule from=""TEAM_CLAIM"" to=""Zoldberg@express.ship,Amy_Wong@express.ship""/>
        <rule from=""faired_supervisor"" to=""Hubert_Farnsworth@express.ship""/>
    </redirection_rules>
</configuration>
"
            );

            _rulesConfig = new XmlRulesConfig(xmlConfig, new Mock<ILogger>().Object);
           
            var jiraMock = new Mock<Bender.IJiraService>();
            jiraMock
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
                                        Assignee = new User {Email = "assignee@express.ship"},
                                        Reporter = new User {Email = "reporter@express.ship"},
                                        Creator = new User {Email = "creator@express.ship"}
                                    }
                        }
                    }
                );

            jiraMock
                .Setup(j => j.GetBuilds(It.IsAny<string>()))
                .Returns(new Build[] { });
        
            jiraMock
                .Setup(j => j.GetIssueById(It.IsAny<string>()))
                .Returns(new Issue());

            _jiraService = jiraMock.Object;

            _logger = new Mock<ILogger>().Object;
        }



        [Test]
        public void JqlSupplierReplaceMarkersByRealAddresses()
        {
            IPackageSupplier packageSupplier = new JqlSupplier(_jiraService!, _rulesConfig!.GetJqlRules("test"), _logger!);
            var message = new IssuePackageConverter("https://jira.express.ship/jira/")
                .ToMessages(packageSupplier.GetPackages().Cast<Package<BenderSendsLetter, Issue>>())
                .Redirect(new Redirector(_rulesConfig.GetRedirectionMap(), Enumerable.Empty<string>(), Enumerable.Empty<string>()))
                .Single();

            Assert.AreEqual("administrator,assignee@express.ship,creator@express.ship", message.To);
            Assert.AreEqual("reporter@express.ship", message.Cc);
        }

        [Test]
        public void RedirectionRulesAreCaseInsensitive()
        {

            IPackageSupplier packageSupplier = new JqlSupplier(_jiraService!, _rulesConfig!.GetJqlRules("test-2"), _logger!);
            var message = new IssuePackageConverter("https://jira.express.ship/jira/")
                .ToMessages(packageSupplier.GetPackages().Cast<Package<BenderSendsLetter, Issue>>())
                .Redirect(new Redirector(_rulesConfig.GetRedirectionMap(), Enumerable.Empty<string>(), Enumerable.Empty<string>()))
                .Single();

            Assert.AreEqual("reporter@express.ship,Zoldberg@express.ship,Amy_Wong@express.ship", message.To);
        }

        [Test]
        public void CheckThatSupervisorIsNotReplacedEvenIfSheIsInRedirectionMap()
        {
            Message? message = null;
            var messenger = new Mock<IMessenger>();
            messenger
                .Setup(m => m.SendAll(It.IsAny<IEnumerable<Message>>()))
                .Callback<IEnumerable<Message>>(m => message = m.Single());
            
            var pipe = new ReactionPipe<Issue>()
            {
                PackageSupplier = new JqlSupplier(_jiraService!, _rulesConfig!.GetJqlRules("test-for-faired-supervisor"), _logger!),
                PackageConverter = new IssuePackageConverter("https://jira.example.com"),
                Redirector = new Redirector(_rulesConfig.GetRedirectionMap(), new[] { "faired_supervisor" }, Enumerable.Empty<string>()),
                Messenger = messenger.Object
                    
            };

            pipe.Run();

            Assert.AreEqual("anybody", message?.To);
            Assert.AreEqual("faired_supervisor", message?.Cc);
        }

        [Test]
        public void CheckThatSupervisorIsNotDuplicatedInCcIfSheIsInTo()
        {
            Message? message = null;
            var messenger = new Mock<IMessenger>();
            messenger
                .Setup(m => m.SendAll(It.IsAny<IEnumerable<Message>>()))
                .Callback<IEnumerable<Message>>(m => message = m.Single());

            var pipe = new ReactionPipe<Issue>()
            {
                PackageSupplier = new JqlSupplier(_jiraService!, _rulesConfig!.GetJqlRules("supervisor-in-To"), _logger!),
                PackageConverter = new IssuePackageConverter("https://jira.example.com"),
                Redirector = new Redirector(_rulesConfig.GetRedirectionMap(), new[] { "supervisor" }, Enumerable.Empty<string>()),
                Messenger = messenger.Object
            };

            pipe.Run();

            Assert.AreEqual("supervisor", message!.To);
            Assert.AreEqual(string.Empty, message!.Cc);
        }

        [Test]
        public void CheckThatSupervisorInCcIsNotDuplicated()
        {
            Message? message = null;
            var messenger = new Mock<IMessenger>();
            messenger
                .Setup(m => m.SendAll(It.IsAny<IEnumerable<Message>>()))
                .Callback<IEnumerable<Message>>(m => message = m.Single());

            var pipe = new ReactionPipe<Issue>()
            {
                PackageSupplier = new JqlSupplier(_jiraService!, _rulesConfig!.GetJqlRules("supervisor-in-Cc"), _logger!),
                PackageConverter = new IssuePackageConverter("https://jira.example.com"),
                Redirector = new Redirector(_rulesConfig.GetRedirectionMap(), new[] { "supervisor" }, Enumerable.Empty<string>()),
                Messenger = messenger.Object
            };

            pipe.Run();

            Assert.AreEqual("anybody", message?.To);
            Assert.AreEqual("supervisor", message?.Cc);
        }

        [Test, Description("When there are no any main addressers, notifications are sent to Supervisors")]
        public void CheckThatSupervisorIsInToFieldWhenThereAreNoOtherAddressees()
        {
            Message? message = null;
            var messenger = new Mock<IMessenger>();
            messenger
                .Setup(m => m.SendAll(It.IsAny<IEnumerable<Message>>()))
                .Callback<IEnumerable<Message>>(m => message = m.Single());

            var pipe = new ReactionPipe<Issue>()
            {
                PackageSupplier = new JqlSupplier(_jiraService!, _rulesConfig!.GetJqlRules("test-for-empty-addressers"), _logger!),
                PackageConverter = new IssuePackageConverter("https://jira.example.com"),
                Redirector = new Redirector(_rulesConfig.GetRedirectionMap(), new[] { "supervisor" }, Enumerable.Empty<string>()),
                Messenger = messenger.Object
            };

            pipe.Run();

            Assert.AreEqual("supervisor", message!.To);
            Assert.AreEqual(string.Empty, message!.Cc);
        }


        [Test, Description("When Superviser is not configured, messages are sent to Maintainers")]
        public void CheckThatMessagesSentToMaintainerWhenSupervisorIsAbsent()
        {
            Message? message = null;
            var messenger = new Mock<IMessenger>();
            messenger
                .Setup(m => m.SendAll(It.IsAny<IEnumerable<Message>>()))
                .Callback<IEnumerable<Message>>(m => message = m.Single());

            var pipe = new ReactionPipe<Issue>()
            {
                PackageSupplier = new JqlSupplier(_jiraService!, _rulesConfig!.GetJqlRules("test-for-empty-addressers"), _logger!),
                PackageConverter = new IssuePackageConverter("https://jira.example.com"),
                Redirector = new Redirector(_rulesConfig.GetRedirectionMap(), Enumerable.Empty<string>(), new[] { "maintainer" }),
                Messenger = messenger.Object
            };

            pipe.Run();

            Assert.AreEqual("maintainer", message?.To);
            Assert.AreEqual(string.Empty, message?.Cc);
        }

        [Test]
        public void FairedSupervisorInAddressees()
        {
            Message? message = null;
            var messenger = new Mock<IMessenger>();
            messenger
                .Setup(m => m.SendAll(It.IsAny<IEnumerable<Message>>()))
                .Callback<IEnumerable<Message>>(m => message = m.Single());

            var pipe = new ReactionPipe<Issue>()
            {
                PackageSupplier = new JqlSupplier(_jiraService!, _rulesConfig!.GetJqlRules("faired-suprevisor-in-addressees"), _logger!),
                PackageConverter = new IssuePackageConverter("https://jira.example.com"),
                Redirector = new Redirector(_rulesConfig.GetRedirectionMap(), new[] { "faired_supervisor" }, Enumerable.Empty<string>()),
                Messenger = messenger.Object
            };

            pipe.Run();

            Assert.AreEqual("Hubert_Farnsworth@express.ship", message!.To);
            Assert.AreEqual("faired_supervisor", message!.Cc);
        }
    }
}
