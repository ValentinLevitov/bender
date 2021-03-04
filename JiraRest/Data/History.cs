#nullable disable

namespace JiraRest.Data
{
    public class History
    {
        public string field { get; set; }
        public User assignee { get; set; }
        public string fieldtype { get; set; }
        public string from { get; set; }
        public string fromString { get; set; }
        public string to { get; set; }
        public string toString { get; set; }
    }
}