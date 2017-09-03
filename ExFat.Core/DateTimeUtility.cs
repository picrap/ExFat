namespace ExFat.Core
{
    using System;

    public static class DateTimeUtility
    {
        /// <summary>
        /// Converts a time stamp to <see cref="DateTime"/>.
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="tenMs">The ten ms.</param>
        /// <returns></returns>
        public static DateTime FromTimeStamp(UInt16 timeStamp, Byte tenMs)
        {
            var twoSeconds = timeStamp & 0x1F; // 0-4 - 5 bits
            var minute = (timeStamp >> 5) & 0x3F; // 5-10 - 6 bits
            var hour = (timeStamp >> 11) & 0x1F; // 11-15 - 5 bits
            var day = (timeStamp >> 16) & 0x1F; // 16-20 - 5 bits
            var month = (timeStamp >> 21) & 0x0F; // 21-24 - 4 bits
            var year = (timeStamp >> 25) & 0x7F; // 25-31 - 7 bits
            return new DateTime(year + 1980, month, day, hour, minute, twoSeconds * 2 + tenMs / 100, tenMs % 100 * 10);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to time stamp.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static Tuple<UInt16, Byte> ToTimeStamp(this DateTime dateTime)
        {
            var timeStamp = (dateTime.Year - 1980) << 25
                | dateTime.Month << 21
                | dateTime.Day << 16
                | dateTime.Hour << 11
                | dateTime.Minute << 5
                | dateTime.Second >> 1;
            var tenMs = dateTime.Millisecond / 10 + dateTime.Second % 2 * 100;
            return Tuple.Create((UInt16)timeStamp, (Byte)tenMs);
        }
    }
}