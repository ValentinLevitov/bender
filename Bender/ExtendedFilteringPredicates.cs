using System;
using System.Text.RegularExpressions;
using Bender.Data;
using System.Linq;

namespace Bender
{
    internal static class ExtendedFilteringPredicates
    {
        internal static bool MoreThanOneFixVersion(IJiraService? jira, Issue issue)
        {
            return issue.BuildFixed!.Count() > 1;
        }

        internal static bool DueDateExpiredMoreThan2WorkingDays(IJiraService? jira, Issue issue)
        {
            return Algorithm.DueDateExpiredMoreThan2WorkingDays(DateTime.Now.Date, issue.DueDate);
        }

        internal static bool EstimatesAttachmentIsAbsent(IJiraService jira, Issue issue)
        {
            return
                !jira
                     .GetIssueAttachments(issue.Key)
                     .Any(
                         a =>
                         new Regex("Согласование оценки (CR|BR)-\\d+\\.msg", RegexOptions.IgnoreCase).IsMatch(a.Filename))
                ;
        }

        internal static class Algorithm
        {
            internal static bool DueDateExpiredMoreThan2WorkingDays(DateTime today, DateTime? dueDate)
            {
                var todayDate = today.Date;
                var dueDateDate = (dueDate ?? today).Date;

                var workingDaysBetweenTodayAndDueDate = Enumerable.Range(1, Math.Max(0, (int) (todayDate - dueDateDate).TotalDays))
                    .Select(i => dueDateDate + TimeSpan.FromDays(i))
                    .Count(d => !new[] { DayOfWeek.Saturday, DayOfWeek.Sunday }.Contains(d.DayOfWeek))
                    ;

                return workingDaysBetweenTodayAndDueDate >= 2;
            }
        }
    }
}