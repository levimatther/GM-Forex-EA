using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;


namespace Commons
{
    public static class Rand
    {
        private static readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        private static readonly decimal Int32Range = new decimal(0, 1, 0, false, 0);
        private static readonly decimal Int64Range = new decimal(0, 0, 1, false, 0);

        private const string _lowerLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string _upperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string _numbers = "1234567890";

        public static byte[] GetBytes(int count)
        {
            var bytes = new byte[count];
            _rng.GetBytes(bytes);
            return bytes;
        }

        public static int NextInt32()
        {
            var bytes = new byte[4];
            _rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static int NextInt32(int minInclusive, int maxExclusive)
        {
            if (maxExclusive < minInclusive) throw new ArgumentException($"Maximum must be greater than or equal to minimum.", "maxExclusive");
            var range = maxExclusive - minInclusive;
            var n = NextUInt32() / Int32Range;
            var value = Math.Floor(minInclusive + n * range);
            return Convert.ToInt32(value);
        }

        public static uint NextUInt32()
        {
            var bytes = new byte[4];
            _rng.GetBytes(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static uint NextUInt32(uint minInclusive, uint maxExclusive)
        {
            if (maxExclusive < minInclusive) throw new ArgumentException($"Maximum must be greater than or equal to minimum.", "maxExclusive");
            var range = maxExclusive - minInclusive;
            var n = NextUInt32() / Int32Range;
            var value = Math.Floor(minInclusive + n * range);
            return Convert.ToUInt32(value);
        }

        public static long NextInt64()
        {
            var bytes = new byte[8];
            _rng.GetBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static long NextInt64(long minInclusive, long maxExclusive)
        {
            if (maxExclusive < minInclusive) throw new ArgumentException($"Maximum must be greater than or equal to minimum.", "maxExclusive");
            var range = maxExclusive - minInclusive;
            var n = NextUInt64() / Int64Range;
            var value = Math.Floor(minInclusive + n * range);
            return Convert.ToInt64(value);
        }

        public static ulong NextUInt64()
        {
            var bytes = new byte[8];
            _rng.GetBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static ulong NextUInt64(ulong minInclusive, ulong maxExclusive)
        {
            if (maxExclusive < minInclusive) throw new ArgumentException($"Maximum must be greater than or equal to minimum.", "maxExclusive");
            var range = maxExclusive - minInclusive;
            var n = NextUInt64() / Int64Range;
            var value = Math.Floor(minInclusive + n * range);
            return Convert.ToUInt64(value);
        }

        public static decimal NextDecimal()
        {
            var n1 = new decimal(NextInt32(), NextInt32(), NextInt32(), false, 0);
            var n2 = new decimal(NextInt32(), NextInt32(), NextInt32(), false, 0);
            var value = n1 < n2 ? n1 / n2 : n2 < n1 ? n2 / n1 : 0M;
            return value < 1 ? value : 0M;
        }

        public static decimal NextDecimal(decimal minInclusive, decimal maxExclusive)
        {
            if (maxExclusive < minInclusive) throw new ArgumentException($"Maximum must be greater than or equal to minimum.", "maxExclusive");
            var range = maxExclusive - minInclusive;
            var rangeScale = (decimal.GetBits(range)[3] >> 16) & 0x1F;
            var value = Math.Round(minInclusive + NextDecimal() * range, rangeScale, MidpointRounding.AwayFromZero);
            return value < maxExclusive ? value : minInclusive;
        }

        public static string NextString(int length, bool upper = false, bool lower = false, bool numeric = false)
        {
            var alphabet = (upper ? _upperLetters : "") +  (lower ? _lowerLetters : "") + (numeric ? _numbers : "");
            var alphabetLength = alphabet.Length;
            if (alphabetLength == 0) return "";
            var result = new StringBuilder(length);
            for (int i = 1 ; i <= length ; i ++)
                result.Append(alphabet[NextInt32(0, alphabetLength)]);
            return result.ToString();
        }

        public static string NextString(int length, string alphabet)
        {
            var alphabetLength = alphabet.Length;
            if (alphabetLength == 0) return "";
            var result = new StringBuilder(length);
            for (int i = 1 ; i <= length ; i ++)
                result.Append(alphabet[NextInt32(0, alphabetLength)]);
            return result.ToString();
        }

        public static string Hex(int length, bool lowerCase = true, char? seperator = null)
        {
            var bytes = GetBytes(length);
            return seperator.HasValue ? Util.BytesToHex(bytes, seperator.Value, lowerCase) : Util.BytesToHex(bytes, lowerCase);
        }

        public static bool NextBoolean(decimal bias = 0M)
        {
            if (bias < 0M || 1M < bias) throw new ArgumentException($"Bias must be in range [0; 1]", "bias");
            return (1M - bias) <= NextDecimal();
        }

        public static void Shuffle<T>(IList<T> items)
        {
            if (items.Count <= 1) return;
            for (int i = items.Count - 1 ; 1 <= i ; i --)
            {
                var ri = NextInt32(0, i + 1);
                if (ri != i) (items[i], items[ri]) = (items[ri], items[i]);
            }
        }
    }
}
