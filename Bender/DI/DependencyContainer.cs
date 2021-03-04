using System.Linq;
using Messaging;
using Bender.AppConfig;
using Bender.Configuration;
using Bender.Data;
using Bender.Data.Supplying;
using Bender.Data.Supplying.Convert;
using Bender.Notification;
using Serilog;
using System.Xml.Linq;
using Unity;
using Unity.Interception;
using Unity.Lifetime;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.TypeInterceptors.VirtualMethodInterception;
using Unity.Injection;

namespace Bender.DI
{
    internal class DependencyContainer
    {
        private readonly IUnityContainer _container = new UnityContainer()
                                                        .AddNewExtension<Interception>();

        public DependencyContainer(string @group)
        {
            var appSettings = new AppSettings();
            appSettings.Validate();
            _container.RegisterInstance(appSettings);

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(appSettings.LoggerSection)
                .CreateLogger();
            
            _container
                .RegisterInstance<ILogger>(logger);

            
            Configure(@group);
        }

        public ReactionPipe<TIssueType> ResolveNotificationPipe<TIssueType>(string? name = null) 
        {
            return _container.Resolve<ReactionPipe<TIssueType>>(name);
        }

        private void Configure(string @group)
        {
            ConfigureApplication();
            ConfigureJiraService();
            ConfigureRules(@group);
            ConfigureSuppliers();
            ConfigureNotificationPipes();
        }

        private void ConfigureApplication()
        {            
            var appSettings = _container.Resolve<AppSettings>();
            var subjectPrefix = new InjectionProperty("SubjectPrefix",
                new InjectionParameter<string>(appSettings.SubjectPrefix));

            _container
                .RegisterType<IMessenger, SmtpClient>(
                    new ContainerControlledLifetimeManager(),
                    new Interceptor<VirtualMethodInterceptor>(),
                    new InterceptionBehavior<SeriloggingBehavior>(),
                    new InjectionConstructor(appSettings.SmtpSection))

                .RegisterType<IPackageConverter<Issue>, IssuePackageConverter>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(_container.Resolve<AppSettings>().JiraRootUri),
                    subjectPrefix)

                .RegisterType<IPackageConverter<Build>, BuildPackageConverter>(
                    new ContainerControlledLifetimeManager(),
                    subjectPrefix);
        }

        private void ConfigureJiraService()
        {
            var appSettings = _container.Resolve<AppSettings>();

            _container
                .RegisterType<IJiraService, HttpJiraService>(
                    new ContainerControlledLifetimeManager(),
                    new Interceptor<VirtualMethodInterceptor>(),
                    new InterceptionBehavior<SeriloggingBehavior>(),
                    new InjectionConstructor(appSettings.JiraRootUri, appSettings.UserName, appSettings.Password),
                    new InjectionProperty("MaxIssueCount", appSettings.MaxResults));
            
            _container.RegisterInstance((IHttpHandler)_container.Resolve<IJiraService>());
        }

        private void ConfigureRules(string @group)
        {
            var appSettings = _container.Resolve<AppSettings>();
            var xmlConfig = XDocument.Load(appSettings.LocalRulesFileName);

            _container.RegisterType<IRulesConfig, XmlRulesConfig>(
                new InjectionConstructor(
                    new InjectionParameter<XDocument>(xmlConfig),
                    new InjectionParameter<ILogger>(_container.Resolve<ILogger>()))
                );

            var rulesConfig = _container.Resolve<IRulesConfig>();

            _container
                .RegisterInstance(rulesConfig.GetJqlRules(@group))
                .RegisterInstance(rulesConfig.GetInStructRules(@group))
                .RegisterInstance(rulesConfig.GetBuildRules(@group));
        }

        private void ConfigureSuppliers()
        {
            _container.RegisterType<IPackageSupplier, JqlSupplier>("Jql",
                new InjectionConstructor(
                    new InjectionParameter<IJiraService>(_container.Resolve<IJiraService>()),
                    new InjectionParameter<JqlRule[]>(_container.Resolve<JqlRule[]>()),
                    new InjectionParameter<ILogger>(_container.Resolve<ILogger>())
                ));

            _container.RegisterType<IPackageSupplier, IssuesInMultipleStructuresSupplier>("Structure",
                new InjectionConstructor(
                    new InjectionParameter<IJiraService>(_container.Resolve<IJiraService>()),
                    new InjectionParameter<IssueInclusionToStructRule[]>(_container.Resolve<IssueInclusionToStructRule[]>())),
                    new InjectionProperty("MaxIssueCount", _container.Resolve<AppSettings>().MaxResults)
                );

            _container.RegisterType<IPackageSupplier, BuildSupplier>(
                new InjectionConstructor(
                    new InjectionParameter<IJiraService>(_container.Resolve<IJiraService>()),
                    new InjectionParameter<BuildRule[]>(_container.Resolve<BuildRule[]>())
                ));
        }

        private void ConfigureNotificationPipes()
        {
            var appSettings = _container.Resolve<AppSettings>();

            _container.Resolve<IRulesConfig>().GetRedirectionMap();

            var redirector = new Redirector(_container.Resolve<IRulesConfig>().GetRedirectionMap(),
                               appSettings.Supervisors,
                               appSettings.Maintainers);

            _container.RegisterInstance(redirector);
            
            var commonProperties = new InjectionMember[]
                             {
                                 new InjectionProperty("Redirector", new InjectionParameter<Redirector>(redirector)),
                                 new InjectionProperty("LogoFileName", new InjectionParameter<string>(appSettings.LogoFileName)),
                                 new InjectionProperty("Messenger", new InjectionParameter<IMessenger>(_container.Resolve<IMessenger>())),
                                 new InjectionProperty("HttpHandler", new InjectionParameter<IHttpHandler>(_container.Resolve<IHttpHandler>()))
                             };

            _container.RegisterType<ReactionPipe<Issue>>("Jql",
                commonProperties.Union(
                    new[]
                    {
                        new InjectionProperty("PackageSupplier", _container.Resolve<IPackageSupplier>("Jql")),
                        new InjectionProperty("PackageConverter", _container.Resolve<IPackageConverter<Issue>>())
                    })
                    .ToArray()
                );

            _container.RegisterType<ReactionPipe<Issue>>("Structure",
                commonProperties.Union(
                    new[]
                    {
                        new InjectionProperty("PackageSupplier", _container.Resolve<IPackageSupplier>("Structure")),
                        new InjectionProperty("PackageConverter", _container.Resolve<IPackageConverter<Issue>>())
                    })
                    .ToArray()
                );

            _container.RegisterType<ReactionPipe<Build>>(
                commonProperties.Union(
                    new[]
                    {
                        new InjectionProperty("PackageSupplier", _container.Resolve<IPackageSupplier>()),
                        new InjectionProperty("PackageConverter", _container.Resolve<IPackageConverter<Build>>())
                    })
                    .ToArray()
                );
        }

        internal void ValidateRules()
        {
            _container.Resolve<IRulesConfig>().ValidateSchema();
        }
    }
}
