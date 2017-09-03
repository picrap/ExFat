namespace ExFat.Core
{
    /// <summary>
    /// Cluster writer
    /// </summary>
    /// <seealso cref="ExFat.Core.IClusterReader" />
    public interface IClusterWriter : IClusterReader
    {
        /// <summary>
        /// Writes the cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="clusterBuffer">The cluster buffer.</param>
        void WriteCluster(long cluster, byte[] clusterBuffer);

        /// <summary>
        /// Sets the next cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="nextCluster">The next cluster.</param>
        /// <returns></returns>
        void SetNextCluster(long cluster, long nextCluster);

        /// <summary>
        /// Allocates a cluster.
        /// </summary>
        /// <param name="previousCluster">The previous cluster.</param>
        /// <returns></returns>
        long AllocateCluster(long previousCluster);
    }
}
