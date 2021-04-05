namespace Bender.Data.Supplying.Convert
{
    public class ScriptingContext
    {
        public Issue issue { get; set; } = new Issue();
        public string rootUri { get; set; } = string.Empty;
    }
}