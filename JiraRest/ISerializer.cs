namespace JiraRest
{
    public interface ISerializer
    {
        T Deserialize<T>(string input);
    }
}