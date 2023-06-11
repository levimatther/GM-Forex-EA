using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace Util
{
    /// <summary>
    /// Controls colored console output by <see langword="Pastel"/>.
    /// </summary>
    public static class Pastel
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        private delegate string ColorFormat(   string input, Color color);
        private delegate string HexColorFormat(string input, string hexColor);

        private const string fmtStart = "\u001B[{0};2;";
        private const string fmtColor = "{1};{2};{3}m";
        private const string fmtContent = "{4}";
        private const string fmtEnd = "\u001B[0m";
        private static readonly string fmtAll = $"{fmtStart}{fmtColor}{fmtContent}{fmtEnd}";

        private static readonly (int BG, int FG) modPlane = (48, 38);

        private static readonly Regex closeNestedPastelStringRegex1 = new ($"({fmtEnd.Replace("[", @"\[")})+", RegexOptions.Compiled);
        private static readonly Regex closeNestedPastelStringRegex2 =
            new
            (
                $"(?<!^)(?<!{fmtEnd.Replace("[", @"\[")})" + 
                $"(?<!{string.Format($"{fmtStart.Replace("[", @"\[")}{fmtColor}", $"(?:{modPlane.FG}|{modPlane.BG})", @"\d{1,3}", @"\d{1,3}", @"\d{1,3}")})" +
                $"(?:{string.Format(fmtStart.Replace("[", @"\["), $"(?:{modPlane.FG}|{modPlane.BG})")})",
                RegexOptions.Compiled
            );

        private static readonly ReadOnlyDictionary<bool, Regex> closeNestedPastelStringRegex3 =
            new
            (
                new Dictionary<bool, Regex>
                {
                    [true] =
                        new ($"(?:{fmtEnd.Replace("[", @"\[")})(?!{string.Format(fmtStart.Replace("[", @"\["), modPlane.FG)})(?!$)", RegexOptions.Compiled),
                    [false] =
                        new ($"(?:{fmtEnd.Replace("[", @"\[")})(?!{string.Format(fmtStart.Replace("[", @"\["), modPlane.BG)})(?!$)", RegexOptions.Compiled)
                }
            );

        private static readonly Func<string, int> parseHexColor = (hc) => int.Parse(hc.Replace("#", ""), NumberStyles.HexNumber);

        private static readonly Func<string, Color, bool, string> colorFormat =
            (i, c, p) => string.Format(fmtAll, p ? modPlane.FG : modPlane.BG, c.R, c.G, c.B, CloseNestedPastelStrings(i, c, p));
        private static readonly Func<string, string, bool, string> colorHexFormat =
            (i, c, p) => colorFormat(i, Color.FromArgb(parseHexColor(c)), p);

        private static readonly ColorFormat foregroundColorFormat = (i, c) => colorFormat(i, c, true);
        private static readonly HexColorFormat foregroundHexColorFormat = (i, c) => colorHexFormat(i, c, true);

        private static readonly ColorFormat backgroundColorFormat = (i, c) => colorFormat(i, c, false);
        private static readonly HexColorFormat backgroundHexColorFormat = (i, c) => colorHexFormat(i, c, false);

        private static readonly ReadOnlyDictionary<bool, ColorFormat> colorFormatFuncs =
            new (new Dictionary<bool, ColorFormat>() { [true] = foregroundColorFormat, [false] = backgroundColorFormat });

        private static readonly ReadOnlyDictionary<bool, HexColorFormat> hexColorFormatFuncs =
            new(new Dictionary<bool, HexColorFormat>() { [true] = foregroundHexColorFormat, [false] = backgroundHexColorFormat });

        static Pastel()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
                var _ = GetConsoleMode(iStdOut, out var outConsoleMode) && SetConsoleMode(iStdOut, outConsoleMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
        }

        /// <summary>
        /// Returns a string wrapped in an ANSI foreground color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="color">The color to use on the specified string.</param>
        public static string FG(Color color, string input) => colorFormatFuncs[true](input, color);

        /// <summary>
        /// Returns a string wrapped in an ANSI foreground color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="hexColor">The color to use on the specified string.<para>Supported format: [#]RRGGBB.</para></param>
        public static string FG(string hexColor, string input) => hexColorFormatFuncs[true](input, hexColor);

        /// <summary>
        /// Returns a string wrapped in an ANSI background color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="color">The color to use on the specified string.</param>
        public static string BG(Color color, string input) => colorFormatFuncs[false](input, color);

        /// <summary>
        /// Returns a string wrapped in an ANSI background color code using the specified color.
        /// </summary>
        /// <param name="input">The string to color.</param>
        /// <param name="hexColor">The color to use on the specified string.<para>Supported format: [#]RRGGBB.</para></param>
        public static string BG(string hexColor, string input) => hexColorFormatFuncs[false](input, hexColor);

        private static string CloseNestedPastelStrings(string input, Color color, bool colorPlane)
        {
            var closedString = closeNestedPastelStringRegex1.Replace(input, fmtEnd);
            closedString = closeNestedPastelStringRegex2.Replace(closedString, $"{fmtEnd}$0");
            closedString = 
                closeNestedPastelStringRegex3[colorPlane]
                    .Replace
                        (closedString, $"$0{string.Format($"{fmtStart}{fmtColor}", colorPlane ? modPlane.FG : modPlane.BG, color.R, color.G, color.B)}");
            return closedString;
        }
    }
}
