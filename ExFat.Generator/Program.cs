namespace ExFat.Generator
{
    using System.IO;
    using Core;

    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var f = File.Create(@"X:\1M"))
            {
                for (uint index = 0; index < 1 << 20; index += 4)
                {
                    var b = LittleEndian.GetBytes(index);
                    f.Write(b, 0, b.Length);
                }
            }
        }
    }
}