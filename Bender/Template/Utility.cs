using System.Web;

namespace Bender.Template
{
    public static class Utility
    {
        public static string EscapeHtml(string unescaped) => HttpUtility.HtmlEncode(unescaped);
    }
}