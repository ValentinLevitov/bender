using System.Collections.Generic;
using Bender.Data;
using Bender.Data.Supplying;

namespace Bender.Template
{
    using IssuePackage = Package<BenderSendsLetter, Issue>;
    public partial class IssuePackagesTemplate
    {
        private readonly IEnumerable<IssuePackage> _packages;
        private readonly string _rootUri;

        internal IssuePackagesTemplate(IEnumerable<IssuePackage> packages, string rootUri)
        {
            _packages = packages;
            _rootUri = rootUri;
        }
    }
}