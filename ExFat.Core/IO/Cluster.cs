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
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
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

        /// <summary>
        /// The first data cluster
        /// </summary>
        public static Cluster First = new Cluster(2);
        /// <summary>
        /// Free cluster instance
        /// </summary>
        public static Cluster Free = new Cluster(0);
        /// <summary>
        /// Last cluster of chain
        /// </summary>
        public static Cluster Last = new Cluster(0xFFFFFFFF);
        /// <summary>
        /// Cluster marked bad
        /// </summary>
        public static Cluster Bad = new Cluster(0xFFFFFFF7);
        /// <summary>
        /// The marker
        /// </summary>
        public static Cluster Marker = new Cluster(0xFFFFFFF8);

        private static long MinLast = -8;
        private static UInt32 Reserved32 = 0xFFFFFFF0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> struct.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
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
        /// Converts value to <see cref="uint"/>.
        /// </summary>
        /// <returns></returns>
        public UInt32 ToUInt32()
        {
            return (UInt32)Value;
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

        /// <summary>
        /// Adds an offset to a cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Cluster operator +(Cluster cluster, int offset)
        {
            return cluster + (long)offset;
        }

        /// <summary>
        /// Adds an offset to a cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static Cluster operator +(Cluster cluster, long offset)
        {
            if (!cluster.IsData)
                throw new InvalidOperationException();
            return new Cluster(cluster.Value + offset);
        }

        /// <summary>
        /// Subtracts an offset to a cluster.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static Cluster operator -(Cluster cluster, long offset)
        {
            if (!cluster.IsData)
                throw new InvalidOperationException();
            return new Cluster(cluster.Value - offset);
        }

        /// <summary>
        /// Indicates whether two clusters have same value
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Cluster a, Cluster b)
        {
            return a.Value == b.Value;
        }

        /// <summary>
        /// Indicates whether two clusters have different value
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Cluster a, Cluster b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Cluster cluster)
                return Value == cluster.Value;
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return (int)Value;
        }
    }
}
