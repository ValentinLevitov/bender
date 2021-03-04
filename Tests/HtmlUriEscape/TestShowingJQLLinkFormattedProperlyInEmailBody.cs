using System.Linq;
using Bender.Template;
using Bender.Data;
using Bender.Data.Supplying;
using NUnit.Framework;

namespace Tests.HtmlUriEscape
{
    [TestFixture]
    public class TestShowingJqlLinkFormattedProperlyInEmailBody
    {
        [Test]
        public void Test1()
        {
            var pac = new Package<BenderSendsLetter, Issue>
            {
                Items = Enumerable.Empty<Issue>().ToArray(),
                Properties =
                {
                    {
                        "Jql", @"project = PRJ1 and type = 'Service Request'  and created >= '2018-06-01'
    and(component is empty or component not in componentMatch('^BFB: '))
    and issue not in (structure('PRJ2 Global Structure'), structure('PRJ_Support'))
    and reporter in membersOf(""Org Employees"") and reporter in membersOf(""Project PRJ3"") and reporter in membersOf(""Project PRJ4"")
"
                    }
                },
                Reaction = new BenderSendsLetter { Subject = string.Empty }
            };

            var mailBody = new IssuePackagesTemplate(Enumerable.Repeat(pac, 1), "https://jira.example.com/").TransformText();
            var expectedSubstring = "https://jira.example.com/issues/?jql=project %3D PRJ1 and type %3D %27Service Request%27  and created >%3D %272018-06-01%27%0D%0A    and%28component is empty or component not in componentMatch%28%27^BFB%3A %27%29%29%0D%0A    and issue not in %28structure%28%27PRJ2 Global Structure%27%29%2C structure%28%27PRJ_Support%27%29%29%0D%0A    and reporter in membersOf%28%22Org Employees%22%29 and reporter in membersOf%28%22Project PRJ3%22%29 and reporter in membersOf%28%22Project PRJ4%22%29%0D%0A";
            StringAssert.Contains(expectedSubstring, mailBody);
        }

        [Test]
        public void Test2()
        {
            var pac = new Package<BenderSendsLetter, Issue>
            {
                Reaction = new BenderSendsLetter {Subject = string.Empty},
                Items = Enumerable.Empty<Issue>().ToArray(),
                Properties =
                {
                    {
                        "Jql",
                        "project=BENDER AND sprint in openSprints() AND status not in (Closed, Verified, Resolved) and Assignee is EMPTY and NOT (issueType=\"Sub-task\" and summary ~ \"Testing\")"
                    }
                }

            };

            var mailBody = new IssuePackagesTemplate(Enumerable.Repeat(pac, 1), "https://jira.example.com/jira/").TransformText();
            StringAssert.Contains("https://jira.example.com/jira/issues/?jql=project%3DBENDER AND sprint in openSprints%28%29 AND status not in %28Closed%2C Verified%2C Resolved%29 and Assignee is EMPTY and NOT %28issueType%3D%22Sub-task%22 and summary ~ %22Testing%22%29",
                mailBody);
        }

        [Test]
        public void Test3()
        {
            var pac = new Package<BenderSendsLetter, Issue>
            {
                Reaction = new BenderSendsLetter { Subject = string.Empty },
                Items = Enumerable.Empty<Issue>().ToArray(),
                Properties =
                {
                    {
                        "Jql", "project=BENDER and status=\"In Progress\" and not updated > -2w"
                    }
                }
            };

            var mailBody = new IssuePackagesTemplate(Enumerable.Repeat(pac, 1), "https://jira.example.com/jira/").TransformText();
            StringAssert.Contains("https://jira.example.com/jira/issues/?jql=project%3DBENDER and status%3D%22In Progress%22 and not updated > -2w",
                mailBody);
        }
    }
}