namespace Test
{
    using System.IO;
    using System.Linq;
    using DiscUtils;
    using DiscUtils.Streams;
    using DiscUtils.Vhdx;
    using ExFat.DiscUtils;

    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var diskStream = File.OpenRead("FATs.vhdx"))
            {
                var disk = new Disk(diskStream, Ownership.Dispose);
                var volume = VolumeManager.GetPhysicalVolumes(disk)[1];
                using (var partitionStream = volume.Open())
                {
                    ExFatFileSystem.Detect(partitionStream);
                    var filesystem = new ExFatFileSystem(partitionStream);
                }
            }
        }
    }
}