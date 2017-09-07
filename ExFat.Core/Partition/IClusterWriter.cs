// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition
{
    using IO;

    /// <summary>
    /// Cluster writer
    /// </summary>
    /// <seealso cref="IClusterReader" />
    public interface IClusterWriter : IClusterReader
    {
        /// <summary>
        /// Writes the cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="clusterBuffer">The cluster buffer.</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void WriteCluster(Cluster cluster, byte[] clusterBuffer, int offset, int length);

        /// <summary>
        /// Sets the next cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="nextCluster">The next cluster.</param>
        /// <returns></returns>
        void SetNextCluster(Cluster cluster, Cluster nextCluster);

        /// <summary>
        /// Allocates a cluster.
        /// </summary>
        /// <param name="previousClusterHint">A hint about the previous cluster, this allows to allocate the next one, if available</param>
        /// <returns></returns>
        Cluster AllocateCluster(Cluster previousClusterHint);

        /// <summary>
        /// Frees the cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        void FreeCluster(Cluster cluster);
    }
}