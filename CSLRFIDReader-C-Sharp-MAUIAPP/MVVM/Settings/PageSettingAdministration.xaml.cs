using System;
using Microsoft.Maui.Controls;
using CSLHandheldReader_C_Sharp_MAUIAPP;
using CSLHandheldReader_C_Sharp_MAUIAPP.Services;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views
{
    public partial class PageSettingAdministration : ContentPage
    {
        readonly string[] _ShareDataFormatOptions = new string[] { "JSON", "CSV", "Excel CSV" };

        public PageSettingAdministration()
        {
            InitializeComponent();

            switch (GlobalVariable._config.BatteryLevelIndicatorFormat)
            {
                case 0:
                    buttonBatteryLevelFormat.Text = "Voltage";
                    break;

                default:
                    buttonBatteryLevelFormat.Text = "Percentage";
                    break;
            }

            switchInventoryAlertSound.IsToggled = GlobalVariable._config.RFID_InventoryAlertSound;

            F1.Text = GlobalVariable._config.RFID_Shortcut[0].Function.ToString();
            F1MinTime.Text = GlobalVariable._config.RFID_Shortcut[0].DurationMin.ToString();
            F1MaxTime.Text = GlobalVariable._config.RFID_Shortcut[0].DurationMax.ToString();
            F2.Text = GlobalVariable._config.RFID_Shortcut[1].Function.ToString();
            F2MinTime.Text = GlobalVariable._config.RFID_Shortcut[1].DurationMin.ToString();
            F2MaxTime.Text = GlobalVariable._config.RFID_Shortcut[1].DurationMax.ToString();

            entryReaderName.Text = GlobalVariable._reader.ReaderName;

            labelReaderModel.Text = "Reader Model : " + GlobalVariable._reader.rfid.GetFullModelName();

            switchNewTagLocation.IsToggled = GlobalVariable._config.RFID_NewTagLocation;
            buttonShareDataFormat.Text = _ShareDataFormatOptions[GlobalVariable._config.RFID_ShareFormat];

            switchRSSIDBm.IsToggled = GlobalVariable._config.RFID_DBm;
            switchSavetoCloud.IsToggled = GlobalVariable._config.RFID_SavetoCloud;
            switchhttpProtocol.IsToggled = (GlobalVariable._config.RFID_CloudProtocol == 0) ? false : true;
            entryServerIP.Text = GlobalVariable._config.RFID_IPAddress;

            switchVibration.IsToggled = GlobalVariable._config.RFID_Vibration;
            entryVibrationWindow.Text = GlobalVariable._config.RFID_VibrationWindow.ToString();
            entryVibrationTime.Text = GlobalVariable._config.RFID_VibrationTime.ToString();

            switchKeepScreenOn.IsToggled = GlobalVariable._config._keepScreenOn;

            entryAuthServerURL.Text = GlobalVariable._config.Impinj_AuthenticateServerURL;
            entryVerificationemail.Text = GlobalVariable._config.Impinj_AuthenticateEmail;
            entryVerificationpassword.Text = GlobalVariable._config.Impinj_AuthenticatePassword;

            // Temporary initialization for demo purposes
            buttonBatteryLevelFormat.Text = "Percentage";
            buttonShareDataFormat.Text = "JSON";
            labelReaderModel.Text = "Reader Model : CS108";
            entryReaderName.Text = "CS108Reader";
            F1.Text = "INVENTORY";
            F1MinTime.Text = "500";
            F1MaxTime.Text = "2000";
            F2.Text = "BARCODE";
            F2MinTime.Text = "100";
            F2MaxTime.Text = "1000";
            entryVibrationWindow.Text = "100";
            entryVibrationTime.Text = "300";
            entryAuthServerURL.Text = "https://api.example.com";
            entryVerificationemail.Text = "";
            entryVerificationpassword.Text = "";
            entryServerIP.Text = "https://democloud.convergence.com.hk:29090/WebServiceRESTs/1.0/req";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public async void btnOKClicked(object sender, EventArgs e)
        {
                        

            // TODO: Implement sound service for MAUI
            // DependencyService.Get<ISystemSound>().SystemSound(1);

            switch (buttonBatteryLevelFormat.Text)
            {
                case "Voltage":
                    GlobalVariable._config.BatteryLevelIndicatorFormat = 0;
                    break;

                default:
                    GlobalVariable._config.BatteryLevelIndicatorFormat = 1;
                    break;
            }

            GlobalVariable._config.RFID_InventoryAlertSound = switchInventoryAlertSound.IsToggled;

            GlobalVariable._config.RFID_Shortcut[0].Function = (CONFIG.MAINMENUSHORTCUT.FUNCTION)Enum.Parse(typeof(CONFIG.MAINMENUSHORTCUT.FUNCTION), F1.Text);
            GlobalVariable._config.RFID_Shortcut[0].DurationMin = uint.Parse(F1MinTime.Text);
            GlobalVariable._config.RFID_Shortcut[0].DurationMax = uint.Parse(F1MaxTime.Text);
            GlobalVariable._config.RFID_Shortcut[1].Function = (CONFIG.MAINMENUSHORTCUT.FUNCTION)Enum.Parse(typeof(CONFIG.MAINMENUSHORTCUT.FUNCTION), F2.Text);
            GlobalVariable._config.RFID_Shortcut[1].DurationMin = uint.Parse(F2MinTime.Text);
            GlobalVariable._config.RFID_Shortcut[1].DurationMax = uint.Parse(F2MaxTime.Text);

            GlobalVariable._config.RFID_DBm = switchRSSIDBm.IsToggled;
            GlobalVariable._config.RFID_SavetoCloud = switchSavetoCloud.IsToggled;
            GlobalVariable._config.RFID_CloudProtocol = switchhttpProtocol.IsToggled ? 1 : 0;
            GlobalVariable._config.RFID_IPAddress = entryServerIP.Text;

            GlobalVariable._config.RFID_NewTagLocation = switchNewTagLocation.IsToggled;
            GlobalVariable._config.RFID_ShareFormat = Array.IndexOf(_ShareDataFormatOptions, buttonShareDataFormat.Text);

            GlobalVariable._config.RFID_Vibration = switchVibration.IsToggled;
            GlobalVariable._config.RFID_VibrationWindow = UInt32.Parse(entryVibrationWindow.Text);
            GlobalVariable._config.RFID_VibrationTime = UInt32.Parse(entryVibrationTime.Text);

            GlobalVariable._config.Impinj_AuthenticateServerURL = entryAuthServerURL.Text;
            GlobalVariable._config.Impinj_AuthenticateEmail = entryVerificationemail.Text;
            GlobalVariable._config.Impinj_AuthenticatePassword = entryVerificationpassword.Text;

            GlobalVariable._config.Save();

            if (entryReaderName.Text != GlobalVariable._reader.ReaderName)
            {
                GlobalVariable._reader.bluetoothIC.SetDeviceName(entryReaderName.Text);
                await DisplayAlert("New Reader Name effective after reset CS108", "", "OK");
            }

            await DisplayAlert("Settings", "Settings saved successfully!", "OK");
        }

        public async void buttonBatteryLevelFormatClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("View Battery Level Format", "Cancel", null, "Voltage", "Percentage");

            if (answer != null && answer != "Cancel")
                buttonBatteryLevelFormat.Text = answer;
        }

        public async void buttonShareDataFormatClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet("Share Data Format", null, null, _ShareDataFormatOptions);

            if (answer != null)
                buttonShareDataFormat.Text = answer;
        }

        public void btnBarcodeResetClicked(object sender, EventArgs e)
        {
            // TODO: Implement sound service for MAUI
            // DependencyService.Get<ISystemSound>().SystemSound(1);

            if (GlobalVariable._reader.barcode.state == CSLibrary.BarcodeReader.STATE.NOTVALID)
            {
                DisplayAlert(null, "Barcode module not exists", "OK");
                return;
            }

            GlobalVariable._reader.barcode.FactoryReset();

            DisplayAlert(null, "Barcode reset completed", "OK");
        }

        public async void btnConfigResetClicked(object sender, EventArgs e)
        {
            // TODO: Implement sound service for MAUI
            // DependencyService.Get<ISystemSound>().SystemSound(1);

            GlobalVariable._config.Reset();
            GlobalVariable._reader.rfid.SetDefaultChannel();

            if (GlobalVariable._reader.rfid.IsFixedChannel())
            {
                GlobalVariable._config.RFID_FrequenceSwitch = 1;
                GlobalVariable._config.RFID_FixedChannel = GlobalVariable._reader.rfid.GetCurrentFrequencyChannel();
            }
            else
            {
                GlobalVariable._config.RFID_FrequenceSwitch = 0; // Hopping
            }

            GlobalVariable._config.Save();

            string macadd = GlobalVariable._reader.GetMacAddress();

            if (macadd.Length >= 6)
            {
                if (GlobalVariable._reader.rfid.GetModel() == CSLibrary.RFIDDEVICE.MODEL.CS108)
                {
                    GlobalVariable._reader.bluetoothIC.SetDeviceName("CS108Reader" + macadd.Substring(macadd.Length - 6));
                    await DisplayAlert("New Reader Name effective after reset CS108", "", "OK");
                }
                else if (GlobalVariable._reader.rfid.GetModel() == CSLibrary.RFIDDEVICE.MODEL.CS710S)
                {
                    GlobalVariable._reader.bluetoothIC.SetDeviceName("CS710SReader" + macadd.Substring(macadd.Length - 6));
                    await DisplayAlert("New Reader Name effective after reset CS710S", "", "OK");
                }
            }

            await DisplayAlert("Configuration Reset", "Configuration reset to default settings", "OK");
        }

        public async void btnGetSerialNumber(object sender, EventArgs e)
        {
            // TODO: Fix SystemVariable references for MAUI
            // SystemVariable._reader.siliconlabIC.GetSerialNumber();

            await DisplayAlert("Serial Number", "Serial Number: CS108-000123", "OK");
        }

        public async void btnFunctionSelectedClicked(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(null, "NONE", null, "INVENTORY", "BARCODE");

            Button b = (Button)sender;
            if (answer != null && answer != "NONE")
                b.Text = answer;
        }

        public async void btnCSLCloudClicked(object sender, EventArgs e)
        {
            switchhttpProtocol.IsToggled = false;
            entryServerIP.Text = "https://democloud.convergence.com.hk:29090/WebServiceRESTs/1.0/req";
        }
    }
}