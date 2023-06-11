using System;
using System.Drawing;


namespace Util
{
    public static class ConsoleLogger
    {
        public static int FormatWidth { get; set; } = 160;

        private static int StatusTxtLength = 10;

        static ConsoleLogger()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        public static void Announce(string msg)
        {
            Console.Out.WriteLine($"{Pastel.FG(Color.DodgerBlue, " \u25A0 INFO    \u25A0")} {Pastel.FG(Color.DodgerBlue, msg)}");
        }

        public static void Info(string msg, bool complete = true)
        {
            if (complete)
                Console.Out.WriteLine($"{Pastel.FG(Color.White, " \u25A0 INFO    \u25A0")} {Pastel.FG(Color.White, msg)}");
            else
            {
                var line = $"{Pastel.FG(Color.White, " \u25A0 INFO    \u25A0")} {Pastel.FG(Color.White, msg)}";
                int lineFieldSize = Math.Max(0, FormatWidth - StatusTxtLength);
                Console.Out.Write(line.PadRight(lineFieldSize, ' '));
            }
        }

        public static void Details(string msg)
        {
            Console.Out.WriteLine($"{Pastel.FG(Color.LightGray, " \u25A0 DETAILS \u25A0")} {Pastel.FG(Color.LightGray, msg)}");
        }

        public static void Success(string msg)
        {
            Console.Out.WriteLine($"{Pastel.FG(Color.Green, " \u25A0 SUCCESS \u25A0")} {Pastel.FG(Color.Green, msg)}");
        }

        public static void Error(string msg)
        {
            Console.Out.WriteLine($"{Pastel.FG(Color.Red, " \u25A0 ERROR   \u25A0")} {Pastel.FG(Color.Red, msg)}");
        }

        public static void Fail(string msg)
        {
            Console.Out.WriteLine($"{Pastel.FG(Color.Red, " \u25A0 FAIL    \u25A0")} {Pastel.FG(Color.Red, msg)}");
        }

        public static void SetStatus(bool status)
        {
            Console.Out.WriteLine($"{(status ? Pastel.FG(Color.Green, " [ DONE ] ") : Pastel.FG(Color.Red, " [ FAIL ] "))}");
        }
    }
}
