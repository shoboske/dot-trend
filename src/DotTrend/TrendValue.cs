namespace DotTrend
{
    /// <summary>
    /// Represents a single data point in a time series trend result.
    /// Each TrendValue contains a date representing the period and the aggregate value for that period.
    /// </summary>
    public class TrendValue : IComparable<TrendValue>
    {
        /// <summary>
        /// Gets or sets the date representing the time period.
        /// The specific format depends on the interval used (minute, hour, day, week, month, year).
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the aggregate value for this time period.
        /// This could be a count, sum, average, minimum, or maximum depending on the aggregation function used.
        /// </summary>
        public decimal Aggregate { get; set; }

        /// <summary>
        /// Compares this TrendValue to another TrendValue by date.
        /// </summary>
        /// <param name="other">The TrendValue to compare with</param>
        /// <returns>A value indicating the relative ordering of the dates</returns>
        public int CompareTo(TrendValue? other)
        {
            if (other == null) return 1;
            return Date.CompareTo(other.Date);
        }
    }
}