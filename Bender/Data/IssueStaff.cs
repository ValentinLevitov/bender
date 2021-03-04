namespace Bender.Data
{
    public class IssueStaff
    {
        public User? Assignee { get; set; }
        public User? Reporter { get; set; }
        public User? Creator { get; set; }
    }
}