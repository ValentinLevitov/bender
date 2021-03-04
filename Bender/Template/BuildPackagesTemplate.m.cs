using System.Collections.Generic;
using Bender.Data;
using Bender.Data.Supplying;

namespace Bender.Template
{
    public partial class BuildPackagesTemplate
    {
        private IEnumerable<Package<BenderSendsLetter, Build>> Packages { get; }

        internal BuildPackagesTemplate(IEnumerable<Package<BenderSendsLetter, Build>> packages)
        {
            Packages = packages;
        }
    }
}