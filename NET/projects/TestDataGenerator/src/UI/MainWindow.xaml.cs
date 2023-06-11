using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Commons;

namespace TestDataGenerator
{
    public partial class MainWindow : Window
    {
        private DataFetcherFactory _dataFetcherFactory;

        private (ToggleButton Button, string Symbol)[] _symbols;
        private (ToggleButton Button, VO.Timeframe Timeframe)[] _timeframes;

        public MainWindow(DataFetcherFactory dataFetcherFactory)
        {
            _dataFetcherFactory = dataFetcherFactory;
            InitializeComponent();
            _symbols = new[]
            {
                (tbnSymbolAUDCAD, "AUDCAD"),
                (tbnSymbolAUDCHF, "AUDCHF"),
                (tbnSymbolAUDJPY, "AUDJPY"),
                (tbnSymbolAUDNZD, "AUDNZD"),
                (tbnSymbolAUDUSD, "AUDUSD"),
                (tbnSymbolCADCHF, "CADCHF"),
                (tbnSymbolCADJPY, "CADJPY"),
                (tbnSymbolCHFJPY, "CHFJPY"),
                (tbnSymbolEURAUD, "EURAUD"),
                (tbnSymbolEURCAD, "EURCAD"),
                (tbnSymbolEURCHF, "EURCHF"),
                (tbnSymbolEURGBP, "EURGBP"),
                (tbnSymbolEURJPY, "EURJPY"),
                (tbnSymbolEURNZD, "EURNZD"),
                (tbnSymbolEURUSD, "EURUSD"),
                (tbnSymbolGBPAUD, "GBPAUD"),
                (tbnSymbolGBPCAD, "GBPCAD"),
                (tbnSymbolGBPCHF, "GBPCHF"),
                (tbnSymbolGBPJPY, "GBPJPY"),
                (tbnSymbolGBPNZD, "GBPNZD"),
                (tbnSymbolGBPUSD, "GBPUSD"),
                (tbnSymbolNZDCAD, "NZDCAD"),
                (tbnSymbolNZDCHF, "NZDCHF"),
                (tbnSymbolNZDJPY, "NZDJPY"),
                (tbnSymbolNZDUSD, "NZDUSD"),
                (tbnSymbolUSDCAD, "USDCAD"),
                (tbnSymbolUSDCHF, "USDCHF"),
                (tbnSymbolUSDJPY, "USDJPY"),
                (tbnSymbolGER30, "GER30"),
                (tbnSymbolSPX500, "SPX500"),
                (tbnSymbolUKOIL, "UKOIL"),
                (tbnSymbolUS30, "US30"),
                (tbnSymbolUSOIL, "USOIL"),
                (tbnSymbolXAGUSD, "XAGUSD"),
                (tbnSymbolXAUUSD, "XAUUSD")
            };
            _timeframes = new []
            {
                (tbnTimeFrameM1, VO.Timeframe.M1),
                (tbnTimeFrameM2, VO.Timeframe.M2),
                (tbnTimeFrameM5, VO.Timeframe.M5),
                (tbnTimeFrameM15, VO.Timeframe.M15),
                (tbnTimeFrameM30, VO.Timeframe.M30),
                (tbnTimeFrameH1, VO.Timeframe.H1)
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MaxHeight = MinHeight = Height;
        }

        private Result<DataFetcherSvc.Params> BuildParameters()
        {
            (DateTime From, DateTime Till)? getDateRange()
            {
                var startDate = calStart.DisplayDate;
                var endDate = calEnd.DisplayDate;
                if (startDate != DateTime.MinValue && endDate != DateTime.MaxValue)
                {
                    var dateFrom = new DateTime(startDate.Year, startDate.Month, 1);
                    var dateTill = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1);
                    if (dateFrom < dateTill) return (From: dateFrom, Till: dateTill);
                }
                return null;
            }
            var symbols = _symbols.Where((sr) => sr.Button.IsChecked ?? false).Select((sr) => sr.Symbol).ToArray();
            if (symbols.Length == 0) return Result<DataFetcherSvc.Params>.Fail(new ApplicationException("Please specify CC symbols."));
            var timefarmes = _timeframes.Where((tr) => tr.Button.IsChecked ?? false).Select((tr) => tr.Timeframe).ToArray();
            if (timefarmes.Length == 0) return Result<DataFetcherSvc.Params>.Fail(new ApplicationException("Please specify at least one timefarme."));
            var dateRange = getDateRange();
            if (!dateRange.HasValue) return Result<DataFetcherSvc.Params>.Fail(new ApplicationException("Please specify a valid date range."));
            var ret = new DataFetcherSvc.Params(symbols, timefarmes, dateRange.Value);
            return Result<DataFetcherSvc.Params>.Done(ret);
        }

        private void CmdGenerate(object sender, RoutedEventArgs ev)
        {
            void start(DataFetcherSvc.Params parameters)
            {
                var dataFetcher = _dataFetcherFactory();
                dataFetcher.Run(parameters);
                var dlg = new ProgressDialog(dataFetcher) { Owner = this };
                dlg.ShowDialog();
                dataFetcher.Shutdown();
            }
            void displayError(Exception? ex) => MessageBox.Show(ex?.Message ?? "Unknown error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            BuildParameters().Next(start).Else(displayError);
        }

        private void CmdClose(object sender, RoutedEventArgs ev) => Close();

        private void OnCalendarGotMouseCapture(object sender, MouseEventArgs ev)
        {
            if (ev.OriginalSource is UIElement src)
                if (src is CalendarDayButton || src is CalendarItem)
                    src.ReleaseMouseCapture();
        }

        private void OnCalendarDisplayModeChanged(object sender, CalendarModeChangedEventArgs ev)
        {
            if (sender is Calendar src)
                if (ev.NewMode != CalendarMode.Year) src.DisplayMode = CalendarMode.Year;
        }
    }
}
