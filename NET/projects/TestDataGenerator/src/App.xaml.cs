using System;
using System.IO;
using System.Windows;

using NLog;


namespace TestDataGenerator
{
    public delegate DataFetcherSvc DataFetcherFactory();

    public partial class App : Application
    {
        private const string LogFileName = "TestDataGenerator.log";
        private const string LogRelFolderPath = @"GMForexEA\Logs";
        private readonly string LogFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), LogRelFolderPath, LogFileName);
        private const string OutputRelFolderPath = @"MetaQuotes\Terminal\Common\Files\GMForexEA";
        private readonly string OutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), OutputRelFolderPath);

        private void OnStartup(object sender, StartupEventArgs e)
        {
            SetupLogging();
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow = new MainWindow(() => new DataFetcherSvc(OutputPath));
            MainWindow.Show();
        }

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
    }
}
