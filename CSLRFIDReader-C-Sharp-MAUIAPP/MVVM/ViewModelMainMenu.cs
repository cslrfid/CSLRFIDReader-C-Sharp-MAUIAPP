using CSLHandheldReader_C_Sharp_MAUIAPP.Converters;
using CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views;
using CSLHandheldReader_C_Sharp_MAUIAPP.Services;
using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Windows.Input;
using static CSLibrary.RFIDDEVICE;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.ViewModels
{
    public class ViewModelMainMenu : INotifyPropertyChanged
    {
        private string _connectedButton = "Press to Scan/Connect Reader";
        private string _labelVoltage = "";
        private string _connectedButtonTextColor = "Red";
        private string _labelAppVersion = "Version MAUI";

        public ViewModelMainMenu()
        {
            // Initialize commands
            OnInventoryButtonCommand = new Command(OnInventoryButtonClicked);
            OnReadWriteButtonCommand = new Command(OnReadWriteButtonClicked);
            OnRegisterTagButtonCommand = new Command(OnRegisterTagButtonClicked);
            OnSpecialFuncButtonCommand = new Command(OnSpecialFuncButtonClicked);
            OnGeigerButtonCommand = new Command(OnGeigerButtonClicked);
            OnSettingButtonCommand = new Command(OnSettingButtonClicked);
            OnSecurityButtonCommand = new Command(OnSecurityButtonClicked);
            OnFilterButtonCommand = new Command(OnFilterButtonClicked);
            OnConnectButtonCommand = new Command(OnConnectButtonClicked);

            // Initialize app version based on orientation
            UpdateAppVersionBasedOnOrientation();
            
            // Request permissions on Android
            GetPermission();
        }

        // MUST grant location permission for Bluetooth scanning on Android
        private async void GetPermission()
        {
            try
            {
                while (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
                {
                    if (await Permissions.RequestAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
                    {
                        // Could show a message to user about why permission is needed
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Permission error: {ex.Message}");
            }
        }

        #region Properties

        public string connectedButton
        {
            get => _connectedButton;
            set
            {
                _connectedButton = value;
                OnPropertyChanged();
            }
        }

        public string labelVoltage
        {
            get => _labelVoltage;
            set
            {
                _labelVoltage = value;
                OnPropertyChanged();
            }
        }

        public string connectedButtonTextColor
        {
            get => _connectedButtonTextColor;
            set
            {
                _connectedButtonTextColor = value;
                OnPropertyChanged();
            }
        }

        public string labelVoltageTextColor
        {
            get => "Black"; // You can implement battery status logic here
        }

        public string labelAppVersion
        {
            get => _labelAppVersion;
            set
            {
                _labelAppVersion = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand OnReadWriteButtonCommand { get; private set; }
        public ICommand OnInventoryButtonCommand { get; private set; }
        public ICommand OnRegisterTagButtonCommand { get; private set; }
        public ICommand OnSpecialFuncButtonCommand { get; private set; }
        public ICommand OnGeigerButtonCommand { get; private set; }
        public ICommand OnSettingButtonCommand { get; private set; }
        public ICommand OnSecurityButtonCommand { get; private set; }
        public ICommand OnFilterButtonCommand { get; private set; }
        public ICommand OnConnectButtonCommand { get; private set; }

        #endregion






        public void OnAppearing()
        {
            GlobalVariable._reader.rfid.StopOperation();
            GlobalVariable._reader.barcode.Stop();

            //base.ViewAppearing();

            GlobalVariable._inventoryEntryPoint = 0;

            SetEvent(true);

            CheckConnection();

            if (GlobalVariable._reader.rfid.GetModel() != MODEL.UNKNOWN)
                GlobalVariable._reader.rfid.CancelAllSelectCriteria();

            GlobalVariable._reader.rfid.Options.TagRanging.focus = false;
            GlobalVariable._reader.rfid.Options.TagRanging.fastid = false;
        }

        public void OnDisappearing()
        {
            SetEvent(false);
        }

        void SetEvent(bool onoff)
        {
            GlobalVariable._reader.CancelEventOnReaderStateChanged();
            GlobalVariable._reader.notification.ClearEventHandler(); // Key Button event handler
            GlobalVariable._reader.rfid.ClearEventHandler(); // Cancel RFID event handler
            GlobalVariable._reader.barcode.ClearEventHandler(); // Cancel Barcode event handler

            if (onoff)
            {
                GlobalVariable._reader.OnReaderStateChanged += new EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs>(ReaderStateCChangedEvent);
                GlobalVariable._reader.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);
                GlobalVariable._reader.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
                GlobalVariable._reader.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);
            }
        }

        void ReaderStateCChangedEvent(object sender, CSLibrary.Events.OnReaderStateChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                switch (e.type)
                {
                    case CSLibrary.Constants.ReaderCallbackType.COMMUNICATION_ERROR:
                        {
                            Application.Current?.MainPage?.DisplayAlert("Communication Error", "Reader communication error, Please reset reader", "OK");
                        }
                        break;

                    case CSLibrary.Constants.ReaderCallbackType.CONNECTION_LOST:
                        break;

                    default:
                        break;
                }

                CheckConnection();
            });
        }

        void VoltageEvent(object sender, CSLibrary.Notification.VoltageEventArgs e)
        {
            if (GlobalVariable._config == null) // reader not connected
                return;

            if (e.Voltage == 0xffff)
            {
                labelVoltage = "Battery ERROR"; //			3.98v
            }
            else
            {
                double voltage = (double)e.Voltage / 1000;

                {
                    var batlow = CSLHandheldReader_C_Sharp_MAUIAPP.Converters.ClassBattery.BatteryLow(voltage);

                    if (GlobalVariable._batteryLow && batlow == CSLHandheldReader_C_Sharp_MAUIAPP.Converters.ClassBattery.BATTERYLEVELSTATUS.NORMAL)
                    {
                        GlobalVariable._batteryLow = false;
                        OnPropertyChanged(nameof(labelVoltageTextColor));
                    }
                    else
                        if (!GlobalVariable._batteryLow && batlow != CSLHandheldReader_C_Sharp_MAUIAPP.Converters.ClassBattery.BATTERYLEVELSTATUS.NORMAL)
                        {
                            GlobalVariable._batteryLow = true;

                            if (batlow == CSLHandheldReader_C_Sharp_MAUIAPP.Converters.ClassBattery.BATTERYLEVELSTATUS.LOW)
                            {
                                MainThread.BeginInvokeOnMainThread(async () =>
                                {
                                    await Application.Current?.MainPage?.DisplayAlert("Battery Warning", "20% Battery Life Left, Please Recharge RFID Reader or Replace Freshly Charged Battery", "OK");
                                });
                            }
                            //else if (batlow == ClassBattery.BATTERYLEVELSTATUS.LOW_17)
                            //{
                            //    MainThread.BeginInvokeOnMainThread(async () =>
                            //    {
                            //        await Application.Current?.MainPage?.DisplayAlert("Battery Warning", "8% Battery Life Left, Please Recharge RFID Reader or Replace with Freshly Charged Battery", "OK");
                            //    });
                            //}

                            OnPropertyChanged(nameof(labelVoltageTextColor));
                        }
                }

                switch (GlobalVariable._config.BatteryLevelIndicatorFormat)
                {
                    case 0:
                        labelVoltage = "Battery " + voltage.ToString("0.000") + "v"; //			v
                        break;

                    default:
                        labelVoltage = "Battery " + CSLHandheldReader_C_Sharp_MAUIAPP.Converters.ClassBattery.Voltage2Percent(voltage).ToString("0") + "%" + " " + voltage.ToString("0.000") + "v"; //			%
                        break;
                }
            }

            OnPropertyChanged(nameof(labelVoltage));
        }

        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
/*            if (GlobalVariable._config == null) // reader not connected
                return;

            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON)
            {
                if (e.KeyDown)
                {
                    _keyPressStartTime = DateTime.Now;
                }
                else
                {
                    double duration = (DateTime.Now - _keyPressStartTime).TotalMilliseconds;

                    for (int cnt = 0; cnt < GlobalVariable._config.RFID_Shortcut.Length; cnt++)
                    {
                        if (duration >= GlobalVariable._config.RFID_Shortcut[cnt].DurationMin && duration <= GlobalVariable._config.RFID_Shortcut[cnt].DurationMax)
                        {
                            switch (GlobalVariable._config.RFID_Shortcut[cnt].Function)
                            {
                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.INVENTORY:
                                    GlobalVariable._inventoryEntryPoint = 0;
                                    OnInventoryButtonClicked();
                                    break;

                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.BARCODE:
                                    GlobalVariable._inventoryEntryPoint = 1;
                                    OnInventoryButtonClicked();
                                    break;
                            }

                            break;
                        }
                    }
                }
            }
*/
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
            if (e.state == CSLibrary.Constants.RFState.INITIALIZATION_COMPLETE)
            {
                //bool LoadSuccess = GlobalVariable._config.Load(GlobalVariable._deviceinfo.Id.ToString(), GlobalVariable._reader.rfid.GetModel());
                //bool LoadSuccess = GlobalVariable._config.Load(GlobalVariable._deviceinfo.Id.ToString());

                GlobalVariable._config = new CONFIG(GlobalVariable._reader.rfid.GetModel());
                GlobalVariable._config.readerID = GlobalVariable._deviceinfo.Id.ToString();
                GlobalVariable._config.Load(GlobalVariable._deviceinfo.Id.ToString());

                {
                    //_ = BleMvxApplication.LoadConfig(BleMvxApplication._deviceinfo.Id.ToString(), BleMvxApplication._reader.rfid.GetModel(), (int)BleMvxApplication._reader.rfid.GetCountry());
                    //BleMvxApplication._config.readerID = BleMvxApplication._deviceinfo.Id.ToString();
                    if (GlobalVariable._reader.rfid.GetModel() == MODEL.CS710S)
                    {
                        if (new Version(GlobalVariable._reader.rfid.GetFirmwareVersionString()) < new Version("2.1.2"))
                        {
                            if (GlobalVariable._config.RFID_Profile == 343)
                                GlobalVariable._config.RFID_Profile = 244;
                        }

                        if (new Version(GlobalVariable._reader.rfid.GetFirmwareVersionString()) >= new Version("2.1.2"))
                        {
                            if (GlobalVariable._reader.rfid.GetModelCountry().Equals("CS710S-1"))
                            {
                                if (GlobalVariable._config.RFID_Profile == 343)
                                    GlobalVariable._config.RFID_Profile = 342;
                                else if (GlobalVariable._config.RFID_Profile == 244)
                                    GlobalVariable._config.RFID_Profile = 241;
                            }
                        }
                    }
                }

                // System Setting
                //                Xamarin.Essentials.DeviceDisplay.KeepScreenOn = BleMvxApplication._config._keepScreenOn;
                GlobalVariable._batteryLow = false;
                OnPropertyChanged(nameof(labelVoltageTextColor));

                // Set Country and Region information
                if (GlobalVariable._config.RFID_Region == "" || GlobalVariable._config.readerModel != GlobalVariable._reader.rfid.GetModel())
                {
                    GlobalVariable._config.readerModel = GlobalVariable._reader.rfid.GetModel();
                    GlobalVariable._config.RFID_Region = GlobalVariable._reader.rfid.GetCurrentCountry();

                    if (GlobalVariable._reader.rfid.IsFixedChannel())
                    {
                        GlobalVariable._config.RFID_FrequenceSwitch = 1;
                        GlobalVariable._config.RFID_FixedChannel = GlobalVariable._reader.rfid.GetCurrentFrequencyChannel();
                    }
                    else
                    {
                        GlobalVariable._config.RFID_FrequenceSwitch = 0; // Hopping
                    }
                }

                int portNum = GlobalVariable._reader.rfid.GetAntennaPort();
                for (uint cnt = 0; cnt < portNum; cnt++)
                {
                    GlobalVariable._reader.rfid.SetAntennaPortState(cnt, GlobalVariable._config.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                    GlobalVariable._reader.rfid.SetPowerLevel(GlobalVariable._config.RFID_Antenna_Power[cnt], (int)cnt);
                    GlobalVariable._reader.rfid.SetInventoryDuration(GlobalVariable._config.RFID_Antenna_Dwell[cnt], cnt);
                }

                if ((GlobalVariable._reader.bluetoothIC.GetFirmwareVersion() & 0x0F0000) != 0x030000) // ignore CS463
                    /*
                if (BleMvxApplication._reader.rfid.GetFirmwareVersion() < 0x0002061D || BleMvxApplication._reader.siliconlabIC.GetFirmwareVersion() < 0x00010010 || BleMvxApplication._reader.bluetoothIC.GetFirmwareVersion() < 0x00010011)
                {
                    _userDialogs.AlertAsync("Firmware too old" + Environment.NewLine + 
                                            "Please upgrade firmware to at least :" + Environment.NewLine +
                                            "RFID Processor firmware: V2.6.44" + Environment.NewLine +
                                            "SiliconLab Firmware: V1.0.16" + Environment.NewLine +
                                            "Bluetooth Firmware: V1.0.17");
                }
                    */
                    ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                GlobalVariable._reader.battery.SetPollingTime(GlobalVariable._config.RFID_BatteryPollingTime);
                GlobalVariable._reader.rfid.SetPowerBoost(GlobalVariable._config.RFID_PowerBoost);
            }
        }

        #region Command Implementations

        private void OnReadWriteButtonClicked()
        {
            // TODO: Check reader connection status
            // if (reader not connected) return;
            
            // Navigate to Read/Write page
            // Application.Current.MainPage.Navigation.PushAsync(new PageReadWrite());
        }

        private void OnInventoryButtonClicked()
        {
            // TODO: Check reader connection status and BLE busy state
            // if (reader not connected) return;
            
            // Navigate to Inventory page
            try
            {
                var inventoryPage = new CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views.PageInventory();
                Shell.Current?.Navigation.PushAsync(inventoryPage);
            }
            catch (Exception ex)
            {
                // Handle navigation error
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private void OnRegisterTagButtonClicked()
        {
            // TODO: Check reader connection status
            // if (reader not connected) return;
            
            // Navigate to Register Tag page
            // Application.Current.MainPage.Navigation.PushAsync(new PageRegisterTag());
        }

        private void OnSpecialFuncButtonClicked()
        {
            // TODO: Check reader connection status
            // if (reader not connected) return;
            
            // Navigate to Special Functions page
            // Application.Current.MainPage.Navigation.PushAsync(new PageSpecialFunctions());
        }

        private void OnGeigerButtonClicked()
        {
            // TODO: Check reader connection status
            // if (reader not connected) return;
            
            // Navigate to Geiger page
            // Application.Current.MainPage.Navigation.PushAsync(new PageGeiger());
        }

        private void OnSettingButtonClicked()
        {
            // TODO: Check reader connection status and BLE busy state
            // if (reader not connected) return;

            // Navigate to Settings page
            try
            {
                var PageSettings = new CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views.PageSetting();
                Shell.Current?.Navigation.PushAsync(PageSettings);
            }
            catch (Exception ex)
            {
                // Handle navigation error
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private void OnSecurityButtonClicked()
        {
            // TODO: Check reader connection status
            // if (reader not connected) return;
            
            // Navigate to Security page
            // Application.Current.MainPage.Navigation.PushAsync(new PageSecurity());
        }

        private void OnFilterButtonClicked()
        {
            // TODO: Check reader connection status
            // if (reader not connected) return;
            
            // Navigate to Filter page
            // Application.Current.MainPage.Navigation.PushAsync(new PageFilter());
        }

        private void OnConnectButtonClicked()
        {
            // TODO: Replace with actual reader busy check when GlobalVariable is implemented
            // if (GlobalVariable._reader.BLEBusy)
            // {
            //     ShowAlert("Configuring Reader, Please Wait");
            //     return;
            // }

            // Initialize selection variables
            // TODO: Replace with actual GlobalVariable when implemented
            // GlobalVariable._SELECT_EPC = "";
            // GlobalVariable._SELECT_PC = 3000;
            // GlobalVariable._PREFILTER_MASK_EPC = "";
            // etc...

            // for Geiger and Read/Write
            GlobalVariable._SELECT_EPC = "";
            //BleMvxApplication._SELECT_EPC = "E280115020001144766E1800"; // for testing
            GlobalVariable._SELECT_PC = 3000;

            // for PreFilter
            GlobalVariable._PREFILTER_MASK_EPC = "";
            GlobalVariable._PREFILTER_MASK_Offset = 0;
            GlobalVariable._PREFILTER_MASK_Truncate = 0;
            GlobalVariable._PREFILTER_Enable = false;

            // for Post Filter
            GlobalVariable._POSTFILTER_MASK_EPC = "";
            GlobalVariable._POSTFILTER_MASK_Offset = 0;
            GlobalVariable._POSTFILTER_MASK_MatchNot = false;
            GlobalVariable._POSTFILTER_MASK_Enable = false;

            labelVoltage = "";

            try
            {
                var deviceListPage = new CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views.PageDeviceList();
                Shell.Current?.Navigation.PushAsync(deviceListPage);
            }
            catch (Exception ex)
            {
                // Handle navigation error
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }

            CheckConnection();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates the app version string based on screen orientation
        /// </summary>
        /// <param name="isLandscape">True if in landscape mode, false for portrait</param>
        public void UpdateAppVersionForOrientation(bool isLandscape)
        {
            string version = Tools.GetAppVersion();
            
            if (isLandscape)
                labelAppVersion = "Version " + version;
            else
                labelAppVersion = "Version\n" + version;
        }

        /// <summary>
        /// Internal method to update app version based on current orientation
        /// This is a fallback method that assumes portrait mode initially
        /// </summary>
        private void UpdateAppVersionBasedOnOrientation()
        {
            // Default to portrait mode on initialization
            // The actual orientation will be set when the page appears
            UpdateAppVersionForOrientation(false);
        }

        private void CheckConnection()
        {
            if (GlobalVariable._reader.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            {
                connectedButton = "Connected to " + GlobalVariable._reader.ReaderName + "/Select Another";
                connectedButtonTextColor = "Blue";
            }
            else
            {
                connectedButton = "Press to Scan/Connect Reader";
                connectedButtonTextColor = "Red";
            }

            OnPropertyChanged(nameof(connectedButton));
            OnPropertyChanged(nameof(connectedButtonTextColor));
        }

        private void ShowConnectionWarningMessage()
        {
            // TODO: Implement user dialog
            // string connectWarningMsg = "Reader NOT connected\n\nPlease connect to reader first!!!";
            // Show dialog or toast message
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
