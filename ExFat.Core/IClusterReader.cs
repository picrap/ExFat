namespace ExFat.Core
{
    public interface IClusterReader
    {
        long GetNext(long cluster);
    }
}