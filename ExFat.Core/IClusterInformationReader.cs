namespace ExFat.Core
{
public    interface IClusterInformationReader
{
    long GetNext(long cluster);
}
}