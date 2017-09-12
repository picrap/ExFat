// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils
{
    partial class ExFatFileSystem
    {
        private readonly char[] _separators;

        public static readonly char[] DefaultSeparators = new[] { '\\', '/' };

        private string GetFileName(string path)
        {
            var lastIndex = path.LastIndexOfAny(_separators);
            if (lastIndex < 0)
                return path;
            return path.Substring(lastIndex + 1);
        }

        private string GetDirectoryName(string path)
        {
            if (path == "")
                return null;
            var lastIndex = path.LastIndexOfAny(_separators);
            if (lastIndex < 0)
                return "";
            return path.Substring(0, lastIndex);
        }
    }
}
