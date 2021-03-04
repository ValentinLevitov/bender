namespace Bender.Configuration
{
    public class IssueInclusionToStructRule : Rule
    {
        /// <summary>
        /// Identifiers of the used structures by the project
        /// </summary>
        public string[] Structures { get; set; } = new string[]{};
    }
}