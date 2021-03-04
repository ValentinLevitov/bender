#nullable disable
namespace JiraRest.Data
{
    public class ChangelogResponse
    {
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public Histories[] histories { get; set; } = new Histories[]{};
    }
}