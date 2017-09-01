namespace ExFat.Core
{
    public interface IPartitionReader
    {
        int BytesPerCluster { get; }
        int BytesPerSector { get; }

        void ReadCluster(long cluster, byte[] clusterBuffer);
        void ReadSectors(long sector, byte[] sectorBuffer, int sectorCount);
    }
}
