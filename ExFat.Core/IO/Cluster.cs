// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.IO
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents a cluster value
    /// </summary>
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public struct Cluster
    {
        public long Value { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is free.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is free; otherwise, <c>false</c>.
        /// </value>
        public bool IsFree => Value == 0;

        /// <summary>
        /// Gets a value indicating whether this instance is a data cluster.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is data; otherwise, <c>false</c>.
        /// </value>
        public bool IsData => Value >= 2;

        /// <summary>
        /// Gets a value indicating whether this instance is last.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is last; otherwise, <c>false</c>.
        /// </value>
        public bool IsLast => Value < 0 && Value >= MinLast;

        public static Cluster Free = new Cluster(0);
        public static Cluster Last = new Cluster(0xFFFFFFFF);
        public static Cluster Bad = new Cluster(0xFFFFFFF7);

        private static long MinLast = -8;
        private static UInt32 Reserved32 = 0xFFFFFFF0;

        public Cluster(UInt32 cluster)
        {
            if (cluster >= Reserved32)
                Value = (int)cluster;
            else
                Value = cluster;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> struct.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        public Cluster(long cluster)
        {
            Value = cluster;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UInt32"/> to <see cref="Cluster"/>.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Cluster(UInt32 cluster)
        {
            return new Cluster(cluster);
        }

        public static Cluster operator +(Cluster cluster, int offset)
        {
            return cluster + (long)offset;
        }

        public static Cluster operator +(Cluster cluster, long offset)
        {
            if (!cluster.IsData)
                throw new InvalidOperationException();
            return new Cluster(cluster.Value + offset);
        }

        public static bool operator ==(Cluster a, Cluster b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(Cluster a, Cluster b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is Cluster cluster)
                return Value == cluster.Value;
            return false;
        }

        public override int GetHashCode()
        {
            return (int) Value;
        }
    }
}
