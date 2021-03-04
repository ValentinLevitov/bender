using System;
using System.Collections.Generic;
using System.Linq;
using Bender.Extensions;
using Messaging;

namespace Bender.Notification
{
    public class Redirector
    {
        private readonly IReadOnlyDictionary<string, string> redirectionMap;
        private readonly IEnumerable<string> supervisors;
        private readonly IEnumerable<string> maintainers;

        public IEnumerable<string> Supervisors => supervisors?.Any() ?? false ? supervisors : maintainers;

        public static Redirector Empty => new Redirector(new Dictionary<string, string>(), Enumerable.Empty<string>(), Enumerable.Empty<string>());

        public Redirector(IReadOnlyDictionary<string, string> redirectionMap,
                          IEnumerable<string> supervisors,
                          IEnumerable<string> maintainers)
        {
            this.redirectionMap = redirectionMap
                .ToCaseInsensitiveDictionary()
                .ToReadOnlyDictionary();

            this.supervisors = supervisors;
            this.maintainers = maintainers;
        }

        private string GetActualAddressee(string recipient)
        {
            string? redirectTo;
            return string.IsNullOrWhiteSpace(recipient) ? string.Join(',', Supervisors)
                : redirectionMap.TryGetValue(recipient, out redirectTo) ? redirectTo
                : recipient;
            
            // TODO: Implement redirection by regex
        }

        public Message ActualizeAddressees(Message initial, char separator = ',')
        {
            var destination = initial;
            var to = (!string.IsNullOrWhiteSpace(destination.To) 
                        ? destination.To.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                        : Supervisors)
                    .Select(GetActualAddressee)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

            var cc =
                destination.Cc.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(GetActualAddressee)
                    .Union(supervisors ?? Enumerable.Empty<string>()) // Add Supervisors (if present) to message Cc, don't substitute them by Maintenance team.
                    .Except(to, StringComparer.OrdinalIgnoreCase)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

            destination.To = string.Join(separator, to);
            destination.Cc = string.Join(separator, cc);

            return destination;

        }
    }
}