using System;
using System.Collections.Generic;
using System.Linq;
using Messaging;
using static System.String;

namespace Bender.Data.Supplying.Convert
{
    internal static class Common<TIssueType>
    {
        public static Message[] ToMessage(IEnumerable<Package<BenderSendsLetter, TIssueType>> packages, 
            Func<IEnumerable<Package<BenderSendsLetter, TIssueType>>, string> toHtml, string subjectPrefix)
        {
            string ToOrderedString(IEnumerable<string> a) => Join(",", a.OrderBy(c => c).ToArray());
            return (from package in packages
                    group package by new
                                     {
                                         To = ToOrderedString(package.Reaction.Addressees.To),
                                         Cc = ToOrderedString(package.Reaction.Addressees.Cc),
                                         package.Reaction.Subject
                                     }
                    into ag
                    select new Message
                           {
                               To = ag.Key.To,
                               Cc = ag.Key.Cc,

                               Subject = $"{subjectPrefix}{ag.Key.Subject}",

                               IsBodyHtml = true,
                               Body = toHtml(ag)
                           }).ToArray();
        }
    }
}
