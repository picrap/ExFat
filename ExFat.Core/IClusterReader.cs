namespace ExFat.Core
{
    public interface IClusterReader
    {
        long GetNextCluster(long cluster);
    }
}