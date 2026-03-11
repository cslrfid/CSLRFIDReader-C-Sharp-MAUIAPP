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
    public class DeviceListViewModel : INotifyPropertyChanged
    {
        private readonly IBluetoothLE _bluetoothLe;
        private readonly IMauiDialogService _dialogService;
        private readonly IAdapter _adapter;
        
        private Guid _previousGuid;
        private CancellationTokenSource? _cancellationTokenSource;

        public IList<IService>? Services { get; private set; }
        public IDescriptor? Descriptor { get; private set; }

        private string _version = string.Empty;
        public string Version 
        { 
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        public ICommand DisconnectCommand => new Command<DeviceListItemViewModel>(DisconnectDevice);
        public ICommand ConnectDisposeCommand => new Command<DeviceListItemViewModel>(ConnectAndDisposeDevice);
        public ICommand StopScanCommand => new Command(StopScan, () => _cancellationTokenSource != null);

        public ObservableCollection<DeviceListItemViewModel> Devices { get; set; } = new ObservableCollection<DeviceListItemViewModel>();
        
        public bool IsRefreshing => _adapter.IsScanning;
        public bool IsStateOn => _bluetoothLe.IsOn;
        public string StateText => GetStateText();
        
        private DeviceListItemViewModel? _selectedDevice;
        public DeviceListItemViewModel? SelectedDevice
        {
            get { return _selectedDevice; }
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

        public List<DeviceListItemViewModel>? SystemDevices { get; private set; }

        public DeviceListViewModel(IBluetoothLE bluetoothLe, IAdapter adapter, IMauiDialogService dialogService)
        {
            _bluetoothLe = bluetoothLe;
            _adapter = adapter;
            _dialogService = dialogService;

            // TODO: Implement reader disconnection for MAUI
            _ = GlobalVariable._reader.DisconnectAsync();

            // Subscribe to Bluetooth events
            _bluetoothLe.StateChanged += OnStateChanged;
            _adapter.DeviceAdvertised += OnDeviceDiscovered;
            _adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
        }

        private void OnDeviceConnectionLost(object? sender, DeviceErrorEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();
                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowToastAsync($"Connection LOST {e.Device.Name} Please reconnect reader", ToastLevel.Error, 5000);
            });
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
                        return "BLE is turning off. That's sad!";
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

        private bool _scanAgain = true;

        private void Adapter_ScanTimeoutElapsed(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged(nameof(IsRefreshing));
                CleanupCancellationToken();

                if (_scanAgain)
                    ScanForDevices();
            });
        }

        private void OnDeviceDiscovered(object? sender, DeviceEventArgs args)
        {
            try
            {
                bool CSLRFIDReaderService = false;
                MODEL BTServiceType = MODEL.UNKNOWN;

                // CS108 filter
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    if (args.Device.AdvertisementRecords.Count < 1)
                        return;

                    foreach (AdvertisementRecord service in args.Device.AdvertisementRecords)
                    {
                        if (service.Data.Length == 2)
                        {
                            // CS108 Service ID = 0x0098
                            if (service.Data[0] == 0x00 && service.Data[1] == 0x98)
                            {
                                BTServiceType = MODEL.CS108;
                                CSLRFIDReaderService = true;
                                break;
                            }

                            // CS710S Service ID = 0x0298
                            if ((service.Data[0] == 0x02 && service.Data[1] == 0x98))
                            {
                                BTServiceType = MODEL.CS710S;
                                CSLRFIDReaderService = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (args.Device.AdvertisementRecords.Count < 1)
                        return;

                    foreach (AdvertisementRecord service in args.Device.AdvertisementRecords)
                    {
                        if (service.Data.Length == 2)
                        {
                            // CS108 Service ID = 0x9800
                            if (service.Data[0] == 0x98 && service.Data[1] == 0x00)
                            {
                                BTServiceType = MODEL.CS108;
                                CSLRFIDReaderService = true;
                                break;
                            }

                            // CS710S Service ID ios = 0x9802, android = 0x5350
                            if ((service.Data[0] == 0x98 && service.Data[1] == 0x02) || (service.Data[0] == 0x53 && service.Data[1] == 0x50))
                            {
                                BTServiceType = MODEL.CS710S;
                                CSLRFIDReaderService = true;
                                break;
                            }
                        }
                        else if (service.Data.Length == 4)
                        {
                            if (service.Data[0] == 0x18 && service.Data[1] == 0x0d && service.Data[2] == 0x98 && service.Data[3] == 0x02)
                            {
                                BTServiceType = MODEL.CS710S;
                                CSLRFIDReaderService = true;
                                break;
                            }
                        }
                    }
                }

                if (!CSLRFIDReaderService)
                    return;

                AddOrUpdateDevice(args.Device, BTServiceType);
            }
            catch (Exception)
            {
                CSLibrary.Debug.WriteLine("Can not handle discovered device");
            }
        }

        private void AddOrUpdateDevice(IDevice device, MODEL BTServiceType, bool isConnected = false)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var vm = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
                    if (vm != null)
                    {
                        vm.Update(device);
                    }
                    else
                    {
                        Devices.Add(new DeviceListItemViewModel(device, BTServiceType, isConnected));
                    }
                }
                catch (Exception)
                {
                    CSLibrary.Debug.WriteLine("Can not add device");
                }
            });
        }

        private bool _runningViewAppearing = false;
        public async Task ViewAppearing()
        {
            if (_runningViewAppearing)
                return;
            _runningViewAppearing = true;

            try
            {
                TryStartScanning();
                await ListConnectedDevicesAsync();
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
                await _adapter.StopScanningForDevicesAsync();
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

        private async void TryStartScanning(bool refresh = false)
        {
            if (IsStateOn && (refresh || !Devices.Any()) && !IsRefreshing)
            {
                Devices.Clear();
                ScanForDevices();
            }
        }

        private async void ScanForDevices()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                OnPropertyChanged(nameof(StopScanCommand));
                OnPropertyChanged(nameof(IsRefreshing));
                
                _adapter.ScanMode = ScanMode.LowLatency;
                await _adapter.StartScanningForDevicesAsync(_cancellationTokenSource.Token);
            }
            catch (Exception)
            {
                CSLibrary.Debug.WriteLine("Can not Scan devices");
            }
        }

        private void CleanupCancellationToken()
        {
            try
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                OnPropertyChanged(nameof(StopScanCommand));

                if (_scanAgain)
                    ScanForDevices();
            }
            catch (Exception)
            {
                CSLibrary.Debug.WriteLine("Can not stop _cancellationTokenSource");
            }
        }

        private void StopScan()
        {
            try
            {
                Devices.Clear();

                _cancellationTokenSource?.Cancel();
                CleanupCancellationToken();
                OnPropertyChanged(nameof(IsRefreshing));
                Task.Delay(100).Wait();
            }
            catch (Exception)
            {
                CSLibrary.Debug.WriteLine("can not stop _cancellationTokenSource");
            }
        }

        private async void DisconnectDevice(DeviceListItemViewModel device)
        {
            // TODO: Implement reader disconnection for MAUI
            // if (GlobalVariable._reader.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            // {
            //     GlobalVariable._reader.DisconnectAsync();
            // }

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
            {
                return false;
            }

            try
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                await _adapter.ConnectToDeviceAsync(device.Device, new ConnectParameters(autoConnect: false, forceBleTransport: true), tokenSource.Token);

                // Show success message
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

        private async Task Connect(IDevice _device, MODEL deviceType)
        {
            CSLibrary.Debug.WriteLine("device name :" + _device.Name);

            // TODO: Implement reader connection for MAUI
            GlobalVariable._deviceinfo = _device;
            await GlobalVariable._reader.ConnectAsync(_adapter, _device, deviceType);

            //CSLibrary.Debug.WriteLine("load config");

            // TODO: Implement config loading for MAUI
            // GlobalVariable._deviceinfo = _device;
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

        private async void OnDeviceDisconnected(object? sender, DeviceEventArgs e)
        {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();
            await _dialogService.HideLoadingAsync();
            await _dialogService.ShowToastAsync($"Disconnected {e.Device.Name}");
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
