using System;
using System.Windows;


namespace TestDataGenerator
{
    public partial class ProgressDialog : Window
    {
        private DataFetcherSvc _dataFetcher;

        public ProgressDialog(DataFetcherSvc dataFetcher)
        {
            _dataFetcher = dataFetcher;
            InitializeComponent();
            pbrProgress.Minimum = 0;
            pbrProgress.Maximum = 1;
            _dataFetcher.OnProgress += (value) => Dispatcher.Invoke(() => UpdateProgress(value));
            _dataFetcher.OnDone += () => Dispatcher.Invoke(Close);
        }

        private void CmdCancel(object sender, RoutedEventArgs ev) => Close();

        private void UpdateProgress(decimal value) => pbrProgress.Value = decimal.ToDouble(value);
    }
}
