using System;
using System.Collections.Generic;
using System.Linq;
using Bender.Template;
using JiraRest;

namespace Bender.Data.Supplying.Convert
{
    internal class BuildPackageConverter : PackageConverterBase<Build>
    {
        public override HttpRequest[] ToHttpRequests(IEnumerable<Package<BenderMakesUpdateHimself, Build>> packages)
        {
            return new HttpRequest[]{};
        }

        protected internal override string StickThemesToSingleHtml(IEnumerable<Package<BenderSendsLetter, Build>> packages)
        {
            return new BuildPackagesTemplate(packages).TransformText();
        }
    }
}