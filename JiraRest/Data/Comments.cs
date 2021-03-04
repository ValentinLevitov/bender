using System;

#nullable disable

namespace JiraRest.Data
{
    public class Comment
    {
        public string self { get; set; }
        public int id { get; set; }
        public User author { get; set; }
        public User updateAuthor { get; set; }
        public string body { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
    }

    public class CommentsResponse
    {
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public Comment[] comments { get; set; }
    }
}
