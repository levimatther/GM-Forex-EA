using System;
using System.IO;

using NLog;
using Topshelf;
using Topshelf.HostConfigurators;
using Topshelf.ServiceConfigurators;


namespace MarketDataProvider
{
    public class WindowsService
    {
        private const string LogFileName = "MarketDataProvider.log";
        private const string LogRelFolderPath = @"GMForexEA\Logs";
        private readonly string LogFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), LogRelFolderPath, LogFileName);

        private DataProviderSvc? _dataProvider;

        private void SetupLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var target = new NLog.Targets.FileTarget("file")
            {
                FileName = LogFilePath,
                CreateDirs = true
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, target);
            LogManager.Configuration = config;
        }

        public void Start()
        {
            SetupLogging();
            _dataProvider = new DataProviderSvc();
            _dataProvider.Run();
        }

        public void Stop()
        {
            _dataProvider?.Shutdown();
        }
    }

    static class Program
    {
        public static void Main()
        {
            void hostConfig(HostConfigurator hc)
            {
                void serviceConfig(ServiceConfigurator<WindowsService> sc)
                {
                    sc.ConstructUsing((name) => new WindowsService());
                    sc.WhenStarted((tc) => tc.Start());
                    sc.WhenStopped((tc) => tc.Stop());
                };
                hc.Service<WindowsService>(serviceConfig);
                hc.RunAsLocalSystem();
                hc.SetDescription("ChartingCenter Data Provider for GM Forex EA");
                hc.SetDisplayName("GM Data Provider");
                hc.SetServiceName("GMCCDP");
                hc.UseNLog();
            };
            var rc = HostFactory.Run(hostConfig);
            var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
