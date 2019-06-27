using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VirtualCOMPortApp
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private SerialPort _writerPort;
        private SerialPort _readerPort;

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<string> ReadedData { get; } = new ObservableCollection<string>();

        private string _writingData;
        public string WritingData
        {
            get => _writingData;
            set
            {
                if (_writingData == value) { return; }
                _writingData = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WritingData)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWritingDataButtonEnabled)));
            }
        }

        public bool IsWritingDataButtonEnabled => !string.IsNullOrEmpty(WritingData);

        private bool _isAppAvailable;
        public bool IsAppAvailable
        {
            get => _isAppAvailable;
            set
            {
                _isAppAvailable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAppAvailable)));
            }
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private void WritingDataButton_Click(object sender, RoutedEventArgs e)
        {
            _writerPort.WriteLine(WritingData);
            WritingData = "";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _writerPort = new SerialPort("COM4");
                _writerPort.Open();
                _readerPort = new SerialPort("COM3");
                _readerPort.Open();
                _readerPort.DataReceived += ReaderPort_DataReceived;
                IsAppAvailable = true;
            }
            catch
            {
                IsAppAvailable = false;
            }
        }

        private async void ReaderPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = _readerPort.ReadExisting();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ReadedData.Insert(0, data));
        }
    }
}
