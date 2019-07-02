using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VirtualCOMPortApp
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private string[] _portNames;
        private SerialPort _writerPort;
        private SerialPort _readerPort;

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<string> ReadedData { get; } = new ObservableCollection<string>();

        private string _writeTargetPortName;
        public string WriteTargetPortName
        {
            get { return _writeTargetPortName; }
            set
            {
                if (_writeTargetPortName == value) { return; }
                _writeTargetPortName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WriteTargetPortName)));
            }
        }

        private string _readTargetPortName;
        public string ReadTargetPortName
        {
            get { return _readTargetPortName; }
            set
            {
                if (_readTargetPortName == value) { return; }
                _readTargetPortName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReadTargetPortName)));
            }
        }

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

        private async void WritingDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (_writerPort == null)
            {
                await new MessageDialog("Please open a COM port to write.").ShowAsync();
                return;
            }

            _writerPort.WriteLine(WritingData);
            WritingData = "";
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _portNames = await SerialPortService.GetPortNamesAsync();
                IsAppAvailable = true;
            }
            catch
            {
                IsAppAvailable = false;
            }

            Bindings.Update();
        }

        private async void ReaderPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = _readerPort.ReadExisting();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ReadedData.Insert(0, data));
        }

        private async void ConnectWriterPortButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WriteTargetPortName))
            {
                await new MessageDialog("Please select a target COM port.").ShowAsync();
                return;
            }

            _writerPort?.Close();
            _writerPort = new SerialPort(WriteTargetPortName);
            _writerPort.Open();
        }

        private async void ConnectReaderPortButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReadTargetPortName))
            {
                await new MessageDialog("Please select a target COM port.").ShowAsync();
                return;
            }

            if (_readerPort != null)
            {
                _readerPort.DataReceived -= ReaderPort_DataReceived;
                _readerPort.Close();
            }

            _readerPort = new SerialPort(ReadTargetPortName);
            _readerPort.Open();
            _readerPort.DataReceived += ReaderPort_DataReceived;
        }
    }
}
