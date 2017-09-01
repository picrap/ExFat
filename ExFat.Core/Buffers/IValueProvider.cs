namespace ExFat.Core.Buffers
{
    public interface IValueProvider<TValue>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        TValue Value { get; set; }
    }
}