using System;

#nullable disable

namespace JiraRest.Data
{
    public class Worklog
    {
        public string self { get; set; }
        public User author { get; set; }
        public User updateAuthor { get; set; }
        public string comment { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public DateTime started { get; set; }
        public string timeSpent { get; set; }
        public int timeSpentSeconds { get; set; }
        public int id { get; set; }
    }

    public class WorklogsResponse
    {
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public Worklog[] worklogs { get; set; }
    }
}