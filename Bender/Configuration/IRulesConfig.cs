using System.Collections.Generic;

namespace Bender.Configuration
{
    public interface IRulesConfig
    {
        JqlRule[] GetJqlRules(string @group);
        BuildRule[] GetBuildRules(string @group);
        IssueInclusionToStructRule[] GetInStructRules(string @group);
        IReadOnlyDictionary<string, string> GetRedirectionMap();
        void ValidateSchema();
    }
}
