using System;

namespace JiraRest
{
    public struct HttpRequest
    {
        public string Verb { get; set; }
        public string Body { get; set; }
        public Uri Uri { get; set; }
    }
}