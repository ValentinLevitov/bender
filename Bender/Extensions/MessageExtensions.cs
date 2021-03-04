using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Messaging;

namespace Bender.Extensions
{
    internal static class MessageExtensions
    {
        public static IEnumerable<Message> SetLogo(this IEnumerable<Message> messages, string logoFileName)
        {
            return messages.Select(
                m =>
                {
                    m.LogoFileName = logoFileName;
                    return m;
                });
        }

        public static IEnumerable<Message> Redirect(this IEnumerable<Message> messages, Notification.Redirector redirector, char separator = ',')
        {
            return messages.Select(m => redirector.ActualizeAddressees(m, separator));
        }
    }
}
