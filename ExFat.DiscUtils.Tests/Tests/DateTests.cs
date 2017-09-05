namespace ExFat.DiscUtils.Tests
{
    using System;
    using Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DateTests
    {
        [TestMethod]
        public void UIn16ToDateTime1()
        {
            var d = DateTimeUtility.FromTimeStamp(0b1011010_0111_00100__10001_101101_00011, 151);
            Assert.AreEqual(new DateTime(2070, 7, 4, 17, 45, 7, 510, DateTimeKind.Local), d);
        }

        [TestMethod]
        public void DateTimeToUIn161()
        {
            var dateTime = new DateTime(2070, 7, 4, 17, 45, 7, 510, DateTimeKind.Local);
            var ts = dateTime.ToTimeStamp();
            Assert.AreEqual(ts.Item1, 0b1011010_0111_00100__10001_101101_00011);
            Assert.AreEqual(ts.Item2, 151);
        }

        [TestMethod]
        public void TimeZoneInfoUtc()
        {
            var t = DateTimeUtility.FromTimeZoneOffset(0x80);
            Assert.AreEqual(t.BaseUtcOffset, TimeSpan.FromHours(0));
        }

        [TestMethod]
        public void TimeZoneInfoDateLine()
        {
            var t = DateTimeUtility.FromTimeZoneOffset(0xD0);
            Assert.AreEqual(t.BaseUtcOffset, TimeSpan.FromHours(-12));
        }

        [TestMethod]
        public void TimeZoneInfoAzores()
        {
            var t = DateTimeUtility.FromTimeZoneOffset(0xFC);
            Assert.AreEqual(t.BaseUtcOffset, TimeSpan.FromHours(-1));
        }

        [TestMethod]
        public void TimeZoneInfoCustom()
        {
            var t = DateTimeUtility.FromTimeZoneOffset(0xF3);
            Assert.AreEqual(t.BaseUtcOffset, TimeSpan.FromHours(-3.25));
        }

        [TestMethod]
        public void TimeZoneInfoToByteUtc()
        {
            var b = DateTimeUtility.ToTimeZoneOffset(TimeZoneInfo.Utc);
            Assert.AreEqual(0x80, b);
        }
    }
}