namespace DotTrend
{
    /// <summary>
    /// Represents a period in time for trend grouping
    /// </summary>
    public class TrendPeriod
    {
        /// <inheritdoc/>
        public int Year { get; set; }
        /// <inheritdoc/>
        public int Month { get; set; }
        /// <inheritdoc/>
        public int Day { get; set; }
        /// <inheritdoc/>
        public int Hour { get; set; }
        /// <inheritdoc/>
        public int Minute { get; set; }
        /// <inheritdoc/>
        public string Interval { get; set; }

        /// <inheritdoc/>
        public TrendPeriod(int year, int month, int day, int hour, int minute, string interval)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Interval = interval;
        }

        /// <summary>
        /// Converts this period to a DateTime
        /// </summary>
        public DateTime ToDateTime()
        {
            return Interval switch
            {
                "minute" => new DateTime(Year, Month, Day, Hour, Minute, 0),
                "hour" => new DateTime(Year, Month, Day, Hour, 0, 0),
                "day" => new DateTime(Year, Month, Day),
                "week" => new DateTime(Year, Month, Day),
                "month" => new DateTime(Year, Month, 1),
                "year" => new DateTime(Year, 1, 1),
                _ => throw new NotSupportedException($"Interval '{Interval}' is not supported.")
            };
        }

        /// <summary>
        /// Gets the formatted date string for this period
        /// </summary>
        public string GetFormattedDate()
        {
            return Interval switch
            {
                "minute" => $"{Year:D4}-{Month:D2}-{Day:D2} {Hour:D2}:{Minute:D2}:00",
                "hour" => $"{Year:D4}-{Month:D2}-{Day:D2} {Hour:D2}:00",
                "day" => $"{Year:D4}-{Month:D2}-{Day:D2}",
                "week" => $"{Year:D4}-{GetISOWeekNumber():D2}",
                "month" => $"{Year:D4}-{Month:D2}",
                "year" => $"{Year:D4}",
                _ => throw new NotSupportedException($"Interval '{Interval}' is not supported.")
            };
        }

        /// <summary>
        /// Gets the ISO week number for this date
        /// </summary>
        private int GetISOWeekNumber()
        {
            return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                new DateTime(Year, Month, Day),
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not TrendPeriod other)
            {
                return false;
            }

            return Interval switch
            {
                "minute" => Year == other.Year && Month == other.Month && Day == other.Day && Hour == other.Hour && Minute == other.Minute,
                "hour" => Year == other.Year && Month == other.Month && Day == other.Day && Hour == other.Hour,
                "day" => Year == other.Year && Month == other.Month && Day == other.Day,
                "week" => Year == other.Year && ToDateTime().AddDays(-((int)ToDateTime().DayOfWeek)).Date == 
                          other.ToDateTime().AddDays(-((int)other.ToDateTime().DayOfWeek)).Date,
                "month" => Year == other.Year && Month == other.Month,
                "year" => Year == other.Year,
                _ => false
            };
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Interval switch
            {
                "minute" => HashCode.Combine(Year, Month, Day, Hour, Minute),
                "hour" => HashCode.Combine(Year, Month, Day, Hour),
                "day" => HashCode.Combine(Year, Month, Day),
                "month" => HashCode.Combine(Year, Month),
                "year" => Year.GetHashCode(),
                _ => 0
            };
        }
    }
}