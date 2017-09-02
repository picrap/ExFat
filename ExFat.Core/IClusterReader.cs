namespace ExFat.Core
{
    /// <summary>
    /// Allows to access cluter information and data
    /// </summary>
    public interface IClusterReader
    {
        /// <summary>
        /// Gets the cluster size, in bytes
        /// </summary>
        /// <value>
        /// The bytes per cluster.
        /// </value>
        int BytesPerCluster { get; }

        /// <summary>
        /// Reads one cluster.
        /// </summary>
        /// <param name="cluster">The cluster number.</param>
        /// <param name="clusterBuffer">The cluster buffer. It must be large enough to contain full cluster</param>
        void ReadCluster(long cluster, byte[] clusterBuffer);
        /// <summary>
        /// Gets the next item for a given cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns></returns>
        long GetNextCluster(long cluster);
    }
}
