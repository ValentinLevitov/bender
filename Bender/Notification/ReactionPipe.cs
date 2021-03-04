using System.Linq;
using System.Threading.Tasks;
using Messaging;
using Bender.Data.Supplying;
using Bender.Data.Supplying.Convert;
using Bender.Extensions;

namespace Bender.Notification
{
    internal class ReactionPipe<TIssueType>
    {
        public IPackageSupplier? PackageSupplier { get; set; }
        public IPackageConverter<TIssueType>? PackageConverter { get; set; }
        public IMessenger? Messenger { get; set; }
        public IHttpHandler? HttpHandler { get; set; }
        public Redirector Redirector { get; set; } = Redirector.Empty;
        public string LogoFileName { get; set; } = string.Empty;

        public void Run()
        {
            if (PackageConverter == null || PackageSupplier == null)
            {
                // Nothing to do
                return;
            }

            var allPackages = PackageSupplier.GetPackages();

            var messages =
                allPackages
                    .OfType<Package<BenderSendsLetter, TIssueType>>()
                    .ToMessages(PackageConverter)
                    .Redirect(Redirector)
                    .SetLogo(LogoFileName);

            Messenger?.SendAll(messages);

            var updatesBenderShouldMadeHimself = allPackages
                    .OfType<Package<BenderMakesUpdateHimself, TIssueType>>()
                    .ToHttpRequests(PackageConverter)
                ;

            HttpHandler?.HandleAll(updatesBenderShouldMadeHimself);
        }

        public async Task RunAsync()
        {
            await Task.Factory.StartNew(Run);
        }
    }
}