using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace Messaging
{
    public class SmtpClient : IMessenger
    {
        private readonly IConfigurationSection _config;

        public SmtpClient(IConfigurationSection config)
        {
            _config = config;
        }

        public int SendRetriesCount { get; set; } = 20;

        public TimeSpan DelayAfterErrorInterval { get; set; } = TimeSpan.FromMinutes(1);

        public virtual void Send(Message data)
        {
            var mailMsg = new MailMessage
            {
                Subject = data.Subject,
                IsBodyHtml = data.IsBodyHtml
            };

            if (!string.IsNullOrWhiteSpace(data.LogoFileName) && data.IsBodyHtml)
            {
                var inlineLogo = new LinkedResource(data.LogoFileName)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };

                var logoBody = $@"<img src=""cid:{inlineLogo.ContentId}"" />";

                var view = AlternateView.CreateAlternateViewFromString(data.Body + logoBody, null, "text/html");
                view.LinkedResources.Add(inlineLogo);
                mailMsg.AlternateViews.Add(view);                
            }
            else
            {
                mailMsg.Body = data.Body;
            }

            mailMsg.To.Add(data.To.Trim(", ".ToCharArray()));

            if (Enum.TryParse(data.Importance, true, out MailPriority priority))
            {
                mailMsg.Priority = priority;
            }

            if (!string.IsNullOrWhiteSpace(data.Cc))
            {
                mailMsg.CC.Add(data.Cc.Trim(", ".ToCharArray()));
            }
            if (!string.IsNullOrWhiteSpace(data.Bcc))
            {
                mailMsg.Bcc.Add(data.Bcc.Trim(", ".ToCharArray()));
            }

            mailMsg.From = new MailAddress(_config["From"]);

            using (var client = new System.Net.Mail.SmtpClient
            {
                Host = _config["Host"],
                Port = int.Parse(_config["Port"]),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_config["User"], _config["Password"]),
                EnableSsl = bool.Parse(_config["EnableSsl"])
            })
            {
                client.Send(mailMsg);
            }
        }

        public virtual void SendAll(IEnumerable<Message> messages)
        {
            Send(this, messages, DelayAfterErrorInterval, SendRetriesCount);
        }

        internal static void Send(IMessenger messenger, IEnumerable<Message> messages, TimeSpan delayInterval, int retryCount)
        {
            foreach (var message in messages)
            {
                SendMessageWithRetries(messenger, message, delayInterval, retryCount);
             }
            //messages
            //    .ToObservable()
            //    .SelectMany(message => GetRetriableSendingObservable(messenger, message, delayInterval, retryCount))
            //    .DefaultIfEmpty()
            //    .Wait();
        }

        private static void SendMessageWithRetries(IMessenger messenger, Message message, TimeSpan delayInterval, int retryCount)
        {
            for (var i = 0;; i++)
            {
                try
                {
                    messenger.Send(message);
                    return;
                }
                catch (SmtpException)
                {
                    if (i >= retryCount)
                    {
                        throw;
                    }
                    Thread.Sleep(delayInterval);
                }
            }
        }


        //private static IObservable<Unit> GetRetriableSendingObservable(IMessenger messenger, Message message, TimeSpan delayInterval, int retryCount) =>
            
        //    Observable
        //        .Defer(() => Observable.Start(() => messenger.Send(message)))
        //        .Catch((SmtpException ex) => Observable
        //            .Throw<Unit>(ex)
        //            .DelaySubscription(delayInterval))
        //        .Retry(retryCount);
    }
}