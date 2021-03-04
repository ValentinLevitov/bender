using NUnit.Framework;

namespace Tests.HtmlUriEscape
{
    [TestFixture]
    public class HtmlFragmentsEscapingTests
    {
        [Test, Description("Symbols such as '<', '>', quotas and similar should be escaped for email body that is in html format")]
        public void JqlLinksEscaping()
        {
            const string unescaped1 = "project=BENDER AND sprint in openSprints() AND status not in (Closed, Verified, Resolved) and Assignee is EMPTY and NOT (issueType=\"Sub-task\" and summary ~ \"Testing\")";
            const string expected1 = "project=BENDER AND sprint in openSprints() AND status not in (Closed, Verified, Resolved) and Assignee is EMPTY and NOT (issueType=&quot;Sub-task&quot; and summary ~ &quot;Testing&quot;)";
            var escaped1 = Bender.Template.Utility.EscapeHtml(unescaped1);

            const string unescaped2 = "project=BENDER and status=\"In Progress\" and not updated > -2w";
            const string expected2 = "project=BENDER and status=&quot;In Progress&quot; and not updated &gt; -2w";
            var escaped2 = Bender.Template.Utility.EscapeHtml(unescaped2);

            Assert.AreEqual(expected1, escaped1);
            Assert.AreEqual(expected2, escaped2);
        }
    }
}