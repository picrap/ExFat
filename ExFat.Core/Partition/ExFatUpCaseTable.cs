// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.Partition
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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
            //for (var c = 'a'; c <= 'z'; c++)
            for (char c = (char)0; c < (char)0xFFFF; c++)
            {
                var uc = char.ToUpper(c);
                if (uc != c)
                    _table[c] = uc;
            }
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
            for (; ; )
            {
                if (upcaseTableStream.Read(pairBytes, 0, pairBytes.Length) == 0)
                    break;
                var c = (char)LittleEndian.ToUInt16(pairBytes);
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
        /// Writes the table to specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public UInt32 Write(Stream stream)
        {
            UInt32 checksum = 0;
            var current = 0;
            var skip = LittleEndian.GetBytes((UInt16)0xFFFF);
            foreach (var lc in _table.Keys.OrderBy(c => c))
            {
                // something to skip
                if (lc != current)
                {
                    Write(stream, skip, ref checksum);
                    Write(stream, LittleEndian.GetBytes((UInt16)(lc - current)), ref checksum);
                }
                Write(stream, LittleEndian.GetBytes(_table[lc]), ref checksum);
                current = lc + 1;
            }
            return checksum;
        }

        private void Write(Stream stream, byte[] bs, ref UInt32 c)
        {
            foreach (var b in bs)
                Write(stream, b, ref c);
        }

        private void Write(Stream stream, byte b, ref UInt32 c)
        {
            stream.WriteByte(b);
            c = c.RotateRight() + b;
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