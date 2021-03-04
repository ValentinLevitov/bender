using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using static System.String;
using System.Text;

namespace Bender.AppConfig
{

    internal class AppSettings
    {
        //private readonly string _assemblyDir;
        //private readonly IConfigurationSection _configSection;

        private readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(
@"
{
    ""Logger"": {
        ""Serilog"": {
            ""Using"": [""Serilog.Sinks.Console""],
            ""WriteTo"": [{""Name"": ""Console""}]
        }
    }
}
"
            )))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("secrets/appsettings.secrets.json", optional: true)
            .Build();

        internal void Validate()
        {
            if(JiraRootUri == null)
            {
                throw new ArgumentException("Required parameter Jira.rootUri is not configured in appsettings.json file", "Jira.rootUri");
            }
            
            if(!SmtpSection.Exists())
            {
                throw new ArgumentException("Required configuration section 'Smtp' is absent in appsettings.json file", "Smtp");
            }
        }

        public string LogoFileName => _configuration["Application:logoFileName"] ?? "logo.jpg";

        public string LocalRulesFileName => _configuration["Application:rulesFileName"] ?? "rules.xml";

        public string JiraRootUri  => _configuration["Jira:rootUri"];

        public string UserName => _configuration["Jira:userName"];

        public string Password => _configuration["Jira:password"];

        public string[] Supervisors => (_configuration["Application:supervisors"] ?? Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);

        public string[] Maintainers => (_configuration["Application:maintenanceTeam"] ?? Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);

        public int MaxResults => _configuration.GetValue<int?>("Jira:maxResults") ?? 50;

        public string SubjectPrefix => _configuration["Application:subjectPrefix"] ?? Empty;

        public IConfigurationSection SmtpSection => _configuration.GetSection("Smtp");

        public IConfigurationSection LoggerSection => _configuration.GetSection("Logger");
    }
}
