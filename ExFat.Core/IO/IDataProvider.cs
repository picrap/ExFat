namespace ExFat.Core.IO
{
    using Entries;

    /// <summary>
    /// When implemented by <see cref="ExFatDirectoryEntry"/>-derived classes, provides information about how to handle data streams
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Gets the data descriptor.
        /// </summary>
        /// <value>
        /// The data descriptor or null if none found.
        /// </value>
        DataDescriptor DataDescriptor { get; }
    }
}