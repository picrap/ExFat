// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Up-case table
    /// </summary>
    public class ExFatUpCaseTable
    {
        private readonly IDictionary<char, char> _table = new Dictionary<char, char>();

        /// <summary>
        /// Sets the default table.
        /// </summary>
        public void SetDefault()
        {
            _table.Clear();
            for (var c = 'a'; c <= 'z'; c++)
                _table[c] = char.ToUpper(c);
        }

        /// <summary>
        /// Reads the specified upcase table stream.
        /// </summary>
        /// <param name="upcaseTableStream">The upcase table stream.</param>
        public void Read(Stream upcaseTableStream)
        {
            _table.Clear();
            byte[] pairBytes = new byte[2];
            char currentChar = '\0';
            bool settingCurrentChar = false;
            for (;;)
            {
                if (upcaseTableStream.Read(pairBytes, 0, pairBytes.Length) == 0)
                    break;
                var c = (char) LittleEndian.ToUInt16(pairBytes);
                // short form: FFFF <char> sets the next char to be set
                // otherwise this is indexed
                if (c == 0xFFFF)
                    settingCurrentChar = true;
                else if (settingCurrentChar)
                {
                    currentChar += c;
                    settingCurrentChar = false;
                }
                else
                {
                    if (currentChar != c)
                        _table[currentChar] = c;
                    ++currentChar;
                }
            }
        }

        /// <summary>
        /// To the upper.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        public char ToUpper(char c)
        {
            if (_table.TryGetValue(c, out char uc))
                return uc;
            return c;
        }
    }
}