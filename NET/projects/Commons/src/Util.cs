using System;


namespace Commons
{
    public static class Util
    {
        public static string BytesToHex(byte[] bytes, bool lowerCase = false)
        {
            var length = bytes.Length;
            if (length == 0) return "";
            char letterA = lowerCase ? 'a' : 'A';
            var chars = new char[length * 2];
            byte b;
            for (int bi = 0, ci = 0 ; bi < length ; bi ++, ci ++)
            {
                b = (byte) (bytes[bi] >> 4);
                chars[ci] = (char) (b <= 9 ? '0' + b : letterA + (b - 10));
                b = (byte) (bytes[bi] & 0x0F);
                chars[++ ci] = (char) (b <= 9 ? '0' + b : letterA + (b - 10));
            }
            return new string(chars);
        }

        public static string BytesToHex(byte[] bytes, char seperator, bool lowerCase = false)
        {
            var length = bytes.Length;
            if (length == 0) return "";
            char letterA = lowerCase ? 'a' : 'A';
            var chars = new char[length * 2 + (length - 1)];
            byte b;
            for (int bi = 0, ci = 0 ; bi < length ; bi++, ci++)
            {
                b = (byte) (bytes[bi] >> 4);
                chars[ci] = (char) (b <= 9 ? '0' + b : letterA + (b - 10));
                b = (byte) (bytes[bi] & 0x0F);
                chars[++ ci] = (char) (b <= 9 ? '0' + b : letterA + (b - 10));
                if (bi + 1 < length) chars[++ ci] = seperator;
            }
            return new string(chars);
        }

        public static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0) throw new ArgumentException("Hex string must have an even number of digits.", "hexString");
            var length = hex.Length / 2;
            byte[] bytes = new byte[length];
            static int digitValue(char digit) =>
                digit switch
                {
                    var d when ('0' <= d && d <= '9') => d - '0',
                    var d when ('A' <= d && d <= 'F') => 10 + (d - 'A'),
                    var d when ('a' <= d && d <= 'f') => 10 + (d - 'a'),
                    _ => throw new ArgumentException($"Invalid hex digit: '{digit}'")
                };
            for (int bi = 0, ci = 0 ; bi < length ; bi ++, ci += 2)
                bytes[bi] = (byte) ((digitValue(hex[ci]) << 4) + digitValue(hex[ci + 1]));
            return bytes;
        }

        public static long UnixNowMillis => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static long ToUnixTimestamp(this DateTime time) => new DateTimeOffset(time).ToUnixTimeMilliseconds();
    }
}
