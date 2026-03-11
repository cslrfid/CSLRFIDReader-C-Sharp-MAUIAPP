using System;
using Microsoft.Maui.Controls;
using CSLHandheldReader_C_Sharp_MAUIAPP.Services;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views
{
    public partial class PageAbout : ContentPage
    {
        public PageAbout()
        {
            InitializeComponent();

            labelModel.Text = "Model " + GlobalVariable._reader.rfid.GetFullModelName();
            labelAppVer.Text = "Application Version " + Tools.GetAppVersion();
            labelLibVer.Text = "Library Version " + GlobalVariable._reader.GetVersion().ToString();
            labelBtFwVer.Text = "Bluetooth Firmware Version " + Version2String(GlobalVariable._reader.bluetoothIC.GetFirmwareVersion());
            labelRFIDFwVer.Text = "RFID Firmware Version " + (GlobalVariable._reader.rfid.GetFirmwareVersionString());
            if (GlobalVariable._reader.rfid.GetModelName() == "CS710S")
                labelSiliconlabFwVer.Text = "ATMEL IC Firmware Version " + Version2String(GlobalVariable._reader.siliconlabIC.GetFirmwareVersion());
            else
                labelSiliconlabFwVer.Text = "SiliconLab IC Firmware Version " + Version2String(GlobalVariable._reader.siliconlabIC.GetFirmwareVersion());
            labelSerialNumber.Text = "Reader Serial Number " + GlobalVariable._reader.siliconlabIC.GetSerialNumberSync();
            labelPCBSerialNumber.Text = "PCB Serial Number " + GlobalVariable._reader.rfid.GetPCBAssemblyCode();
        }

        string Version2String(uint ver)
        {
            return string.Format("{0}.{1}.{2}", (ver >> 16) & 0xff, (ver >> 8) & 0xff, ver & 0xff);
        }

        string GetPCBVersion()
        {
            try
            {
                var ver = GlobalVariable._reader.siliconlabIC.GetPCBVersion();

                if (ver.Substring(2, 1) != "0")
                    return ver.Substring(0, 1) + "." + ver.Substring(1, 2);
                else
                    return ver.Substring(0, 1) + "." + ver.Substring(1, 1);
            }
            catch (Exception ex)
            {
                return "No PCB Version";
            }
        }

        public async void buttonOpenPrivacypolicyClicked(object sender, EventArgs args)
        {
            try
            {
                await Launcher.OpenAsync(new Uri("https://www.convergence.com.hk/apps-privacy-policy/"));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Could not open privacy policy in browser", "OK");
            }
        }
    }
}