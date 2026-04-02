using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using CSLibrary;
using static CSLibrary.RFIDDEVICE;
using CSLHandheldReader_C_Sharp_MAUIAPP.Services;
using Microsoft.Maui.Controls;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.ViewModels
{
    public enum ConnectionModeEnum { Bluetooth, TCP }

    public class DeviceListViewModel : INotifyPropertyChanged
    {
        private readonly IBluetoothLE _bluetoothLe;
        private readonly IMauiDialogService _dialogService;
        private readonly IAdapter _adapter;
        
        private bool _initialized;

        public ICommand DisconnectCommand => new Command<DeviceListItemViewModel>(DisconnectDevice);
        public ICommand ConnectDisposeCommand => new Command<DeviceListItemViewModel>(ConnectAndDisposeDevice);
        public ICommand StopScanCommand => new Command(StopScan, () => _adapter.IsScanning);
        public ICommand ConnectTcpCommand => new Command(async () => await ConnectTcpAsync());

        public ObservableCollection<DeviceListItemViewModel> Devices { get; set; } = new ObservableCollection<DeviceListItemViewModel>();
        
        public bool IsRefreshing => _adapter.IsScanning;
        public bool IsStateOn => _bluetoothLe.IsOn;
        public string StateText => GetStateText();
        
        // TCP mode
        private ConnectionModeEnum _connectionMode = ConnectionModeEnum.Bluetooth;
        public ConnectionModeEnum ConnectionMode
        {
            get => _connectionMode;
            set
            {
                if (_connectionMode != value)
                {
                    _connectionMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsBluetoothMode));
                    OnPropertyChanged(nameof(IsTcpMode));
                    OnPropertyChanged(nameof(IsDeviceListVisible));
                    OnPropertyChanged(nameof(IsTcpPanelVisible));
                }
            }
        }

        public bool IsBluetoothMode => ConnectionMode == ConnectionModeEnum.Bluetooth;
        public bool IsTcpMode => ConnectionMode == ConnectionModeEnum.TCP;
        public bool IsDeviceListVisible => IsBluetoothMode;
        public bool IsTcpPanelVisible => IsTcpMode;

        private string _ipAddress = "192.168.1.100";
        public string IpAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(); }
        }

        private int _tcpPort = 1515;
        public int TcpPort
        {
            get => _tcpPort;
            set { _tcpPort = value; OnPropertyChanged(); }
        }

        private DeviceListItemViewModel? _selectedDevice;
        public DeviceListItemViewModel? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                if (value != null)
                {
                    if (value.IsConnected)
                    {
                        _ = _dialogService.ShowAlertAsync($"This device is in OS Bluetooth list, please do the following:" + Environment.NewLine +
                            "1) in OS Bluetooth list, 'forget' it." + Environment.NewLine +
                            "2) after doing #1 above, make sure reader is not in HID mode. (characterized by fast Bluetooth LED flash). If reader is in HID mode, change to normal mode." + Environment.NewLine + 
                            Environment.NewLine +
                            "After #1 & #2 above, restart this App.", "Device Connected");
                    }
                    else
                        _ = Task.Run(() => HandleSelectedDevice(value));
                }
                OnPropertyChanged();
            }
        }

        public DeviceListViewModel(IBluetoothLE bluetoothLe, IAdapter adapter, IMauiDialogService dialogService)
        {
            _bluetoothLe = bluetoothLe;
            _adapter = adapter;
            _dialogService = dialogService;

            // Initialize DeviceFinder once — handles UUID filtering, platform byte-order, main-thread marshaling
            if (!_initialized)
            {
                CSLibrary.DeviceFinder.Initialize(adapter, bluetoothLe);
                _initialized = true;
            }

            // Subscribe to Bluetooth state changes
            _bluetoothLe.StateChanged += OnStateChanged;

            // Subscribe to DeviceFinder events (replaces ~85 lines of OnDeviceDiscovered UUID filtering)
            // Note: Initialize() is idempotent — safe to call each time ViewModel is constructed
            CSLibrary.DeviceFinder.OnDeviceFound += OnDeviceFound;
            CSLibrary.DeviceFinder.OnSearchCompleted += OnSearchCompleted;

            // Subscribe to connection lost handler
            GlobalVariable._reader.OnReaderStateChanged += OnReaderStateChanged;

            // Disconnect any existing reader connection on view init
            _ = GlobalVariable._reader.DisconnectAsync();
        }

        private void OnDeviceFound(object? sender, CSLibrary.DeviceFinder.DeviceFoundEventArgs e)
        {
            // DeviceFinder already marshals to main thread — no MainThread.Invoke needed
            var info = e.Device;
            AddOrUpdateDevice(info.NativeDevice, info.DeviceType);
        }

        private void OnSearchCompleted(object? sender, CSLibrary.DeviceFinder.SearchCompletedEventArgs e)
        {
            // Scan finished (timeout or user stop)
            OnPropertyChanged(nameof(IsRefreshing));
            OnPropertyChanged(nameof(StopScanCommand));
        }

        private void OnReaderStateChanged(object? sender, CSLibrary.Events.OnReaderStateChangedEventArgs e)
        {
            if (e.type == CSLibrary.Constants.ReaderCallbackType.CONNECTION_LOST)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _dialogService.HideLoadingAsync();
                    await _dialogService.ShowToastAsync("Connection Lost. Please reconnect reader.", ToastLevel.Error, 5000);
                });
            }
        }

        private void OnStateChanged(object? sender, BluetoothStateChangedArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged(nameof(IsStateOn));
                OnPropertyChanged(nameof(StateText));
            });
        }

        private string GetStateText()
        {
            try
            {
                switch (_bluetoothLe.State)
                {
                    case BluetoothState.Unavailable:
                        return "BLE is not available on this device.";
                    case BluetoothState.Unauthorized:
                        return "You are not allowed to use BLE.";
                    case BluetoothState.TurningOn:
                        return "BLE is warming up, please wait.";
                    case BluetoothState.On:
                        return "BLE is on.";
                    case BluetoothState.TurningOff:
                        return "BLE is turning off.";
                    case BluetoothState.Off:
                        if (DeviceInfo.Platform == DevicePlatform.iOS)
                            _ = _dialogService.ShowAlertAsync("Please put finger at bottom of screen and swipe up 'Control Center' and turn on Bluetooth. If Bluetooth is already on, turn it off and on again", "Bluetooth Required");
                        return "BLE is off. Turn it on!";
                }
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine($"Error getting state text: {ex.Message}");
            }

            return "Unknown BLE state.";
        }

        private void AddOrUpdateDevice(IDevice device, MODEL BTServiceType, bool isConnected = false)
        {
            try
            {
                var vm = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
                if (vm != null)
                    vm.Update(device);
                else
                    Devices.Add(new DeviceListItemViewModel(device, BTServiceType, isConnected));
            }
            catch (Exception)
            {
                CSLibrary.Debug.WriteLine("Can not add device");
            }
        }

        private bool _runningViewAppearing = false;
        public async Task ViewAppearing()
        {
            if (_runningViewAppearing)
                return;
            _runningViewAppearing = true;

            try
            {
                // Only scan in Bluetooth mode
                if (IsBluetoothMode)
                {
                    TryStartScanning();
                    await ListConnectedDevicesAsync();
                }
            }
            catch (Exception)
            {
                CSLibrary.Debug.WriteLine("Device Resume Error");
            }
        }

        public async Task ViewDisappearing()
        {
            try
            {
                // Unsubscribe from events to prevent duplicate handlers on page re-entry
                CSLibrary.DeviceFinder.OnDeviceFound -= OnDeviceFound;
                CSLibrary.DeviceFinder.OnSearchCompleted -= OnSearchCompleted;
                GlobalVariable._reader.OnReaderStateChanged -= OnReaderStateChanged;
                CSLibrary.DeviceFinder.StopDeviceSearch();  // safe even if not scanning
                OnPropertyChanged(nameof(IsRefreshing));
            }
            catch (Exception)
            {
                CSLibrary.Debug.WriteLine("Device Suspend error");
            }
        }

        public async Task ListConnectedDevicesAsync()
        {
            await Task.Delay(1000); // Give some time for the device disconnect
            if (DeviceInfo.Platform == DevicePlatform.iOS)
                await iosListConnectedDevicesAsync();
        }

        public async Task iosListConnectedDevicesAsync()
        {
            if (DeviceInfo.Platform != DevicePlatform.iOS)
                return;

            Console.WriteLine("Fetching connected devices...");

            Guid serviceUuid = new Guid("00009802-0000-1000-8000-00805f9b34fb");
            var connectedDevices = _adapter.GetSystemConnectedOrPairedDevices(new[] { serviceUuid });

            foreach (var device in connectedDevices)
                AddOrUpdateDevice(device, MODEL.CS710S, true);
        }

        private void TryStartScanning(bool refresh = false)
        {
            if (IsStateOn && (refresh || !Devices.Any()) && !IsRefreshing)
            {
                Devices.Clear();
                ScanForDevices();
            }
        }

        private void ScanForDevices()
        {
            try
            {
                OnPropertyChanged(nameof(StopScanCommand));
                OnPropertyChanged(nameof(IsRefreshing));
                
                // DeviceFinder handles UUID filtering, platform byte-order, and main-thread marshaling internally
                CSLibrary.DeviceFinder.StartDeviceSearch(ScanMode.LowLatency, 5000);
            }
            catch (Exception)
            {
                CSLibrary.Debug.WriteLine("Can not Scan devices");
            }
        }

        private void StopScan()
        {
            try
            {
                Devices.Clear();
                CSLibrary.DeviceFinder.StopDeviceSearch();
                OnPropertyChanged(nameof(IsRefreshing));
                OnPropertyChanged(nameof(StopScanCommand));
            }
            catch (Exception)
            {
                CSLibrary.Debug.WriteLine("Can not stop scan");
            }
        }

        private async void DisconnectDevice(DeviceListItemViewModel device)
        {
            try
            {
                if (!device.IsConnected)
                    return;

                await _dialogService.ShowLoadingAsync($"Disconnecting {device.Name}...");
                await _adapter.DisconnectDeviceAsync(device.Device);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(ex.Message, "Disconnect Error");
            }
            finally
            {
                device.Update();
                await _dialogService.HideLoadingAsync();
            }
        }

        private async void HandleSelectedDevice(DeviceListItemViewModel devices)
        {
            try
            {
                if (await ConnectDeviceAsync(devices))
                {
                    var device = _adapter.ConnectedDevices.FirstOrDefault(d => d.Id.Equals(devices.Device.Id));

                    if (device == null)
                        return;

                    await Connect(device, devices.BTServiceType);

                    // Navigate back to main menu after successful connection
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(ex.Message, "Connection Error");
            }
        }

        private async Task<bool> ConnectDeviceAsync(DeviceListItemViewModel device, bool showPrompt = true)
        {
            if (showPrompt && !await _dialogService.ConfirmAsync($"Connect to device '{device.Name}'?"))
                return false;

            try
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                await _adapter.ConnectToDeviceAsync(device.Device, new ConnectParameters(autoConnect: false, forceBleTransport: true), tokenSource.Token);
                await _dialogService.ShowToastAsync("Initializing Reader, Please Wait.", ToastLevel.Success, 10000);
                return true;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(ex.Message, "Connection Error");
                CSLibrary.Debug.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                device.Update();
            }
        }

        private async Task Connect(IDevice device, MODEL deviceType)
        {
            CSLibrary.Debug.WriteLine("device name :" + device.Name);
            GlobalVariable._deviceinfo = device;
            await GlobalVariable._reader.ConnectAsync(_adapter, device, deviceType);
        }

        private async Task ConnectTcpAsync()
        {
            if (string.IsNullOrWhiteSpace(IpAddress))
            {
                await _dialogService.ShowAlertAsync("Please enter a valid IP address.", "Invalid Input");
                return;
            }

            if (!await _dialogService.ConfirmAsync($"Connect to CS203XL at {IpAddress}:{TcpPort}?", "Confirm TCP Connection"))
                return;

            try
            {
                await _dialogService.ShowLoadingAsync($"Connecting to {IpAddress}...");

                // TCP connect via CSLibrary — sets CONNECTIONMODE.TCP internally
                await GlobalVariable._reader.ConnectAsync(IpAddress, TcpPort);

                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowToastAsync("CS203XL Connected via TCP!", ToastLevel.Success, 3000);

                // Navigate back to main menu after successful connection
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync($"TCP Connection failed: {ex.Message}", "Connection Error");
                CSLibrary.Debug.WriteLine($"TCP Connect error: {ex.Message}");
            }
        }

        private async void ConnectAndDisposeDevice(DeviceListItemViewModel item)
        {
            try
            {
                using (item.Device)
                {
                    await _adapter.ConnectToDeviceAsync(item.Device);
                    item.Update();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(ex.Message, "Failed to Connect and Dispose");
            }
            finally
            {
                await _dialogService.HideLoadingAsync();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
