using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bender.Data;
using Bender.DI;

namespace Bender
{
    internal static class Application
    {
        public static void Run(IList<string> args)
        {
            var container = new DependencyContainer(args[0]);
            container.ValidateRules();
            Task.WaitAll(
                container.ResolveNotificationPipe<Issue>("Jql").RunAsync(),
                container.ResolveNotificationPipe<Issue>("Structure").RunAsync(),
                container.ResolveNotificationPipe<Build>().RunAsync()
            );
        }
    }
}