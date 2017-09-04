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
    }
}