using System;
using Bender;
using Bender.Data;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class DueDateExpiredMoreThan2WorkingDaysTests
    {
        [Test]
        public void TestNullDateTimeDoesNotThrowException()
        {
            ExtendedFilteringPredicates.DueDateExpiredMoreThan2WorkingDays(null, new Issue());
        }

        [Test]
        public void TestDueDateIsGreaterThanToday()
        {
            var lessToday = new DateTime(2014, 06, 20);
            var greaterDueDate = new DateTime(2014, 06, 23);
            Assert.IsFalse(ExtendedFilteringPredicates.Algorithm.DueDateExpiredMoreThan2WorkingDays(lessToday, greaterDueDate));
        }

        [Test]
        public void TestDueDateIsMonday()
        {
            var dueDateIsMonday = new DateTime(2014, 06, 23);
            var todayIsMonday = new DateTime(2014, 06, 23);
            var todayIsTuesday = new DateTime(2014, 06, 24);
            var todayIsWednesday = new DateTime(2014, 06, 25);
            Assert.IsFalse(ExtendedFilteringPredicates.Algorithm.DueDateExpiredMoreThan2WorkingDays(todayIsMonday, dueDateIsMonday));
            Assert.IsFalse(ExtendedFilteringPredicates.Algorithm.DueDateExpiredMoreThan2WorkingDays(todayIsTuesday, dueDateIsMonday));
            Assert.IsTrue(ExtendedFilteringPredicates.Algorithm.DueDateExpiredMoreThan2WorkingDays(todayIsWednesday, dueDateIsMonday));
        }

        [Test]
        public void TestDueDateIsFriday()
        {
            var dueDateIsFriday = new DateTime(2014, 06, 20);
            var todayIsMonday = new DateTime(2014, 06, 23);
            var todayIsTuesday = new DateTime(2014, 06, 24);
            Assert.IsFalse(ExtendedFilteringPredicates.Algorithm.DueDateExpiredMoreThan2WorkingDays(todayIsMonday, dueDateIsFriday));
            Assert.IsTrue(ExtendedFilteringPredicates.Algorithm.DueDateExpiredMoreThan2WorkingDays(todayIsTuesday, dueDateIsFriday));
        }

        [Test]
        public void TestDueDateIsSaturday()
        {
            var dueDateIsSaturday = new DateTime(2014, 06, 21);
            var todayIsMonday = new DateTime(2014, 06, 23);
            var todayIsTuesday = new DateTime(2014, 06, 24);
            Assert.IsFalse(ExtendedFilteringPredicates.Algorithm.DueDateExpiredMoreThan2WorkingDays(todayIsMonday, dueDateIsSaturday));
            Assert.IsTrue(ExtendedFilteringPredicates.Algorithm.DueDateExpiredMoreThan2WorkingDays(todayIsTuesday, dueDateIsSaturday));
        }
    }
}
