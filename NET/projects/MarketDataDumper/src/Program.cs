using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;


namespace MarketDataProvider
{
    static class Program
    {
        static bool ParseCommandLine(string[] args, [NotNullWhen(true)] out VO.DataDumperParams? cmdParams)
        {
            VO.Subscription cvtSubscription(string txt)
            {
                var prvSymTfPair = txt.Split('/');
                (var prvSymPair, var timeframe) = prvSymTfPair.Length == 2 ? (prvSymTfPair[0].Split(':'), prvSymTfPair[1]) : throw new FormatException();
                (var provider, var symbol) = prvSymPair.Length == 2 ? (prvSymPair[0], prvSymPair[1]) : throw new FormatException();
                return new VO.Subscription(provider, symbol, timeframe);
            }
            try
            {
                var subscriptions = args.Select(cvtSubscription).ToArray();
                cmdParams = new(subscriptions);
                return true;
            }
            catch (FormatException)
            {
                cmdParams = null;
                return false;
            }
        }
        
        static void Main(string[] args)
        {
            if (ParseCommandLine(args, out var cmdParams))
            {
                var dataDumper = new DataDumperSvc(cmdParams);
                Console.CancelKeyPress += (_, ev) => { ev.Cancel = true; dataDumper.Shutdown(); };
                dataDumper.Run();
            }
        }
    }
}
