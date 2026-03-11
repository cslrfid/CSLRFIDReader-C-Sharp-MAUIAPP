using System;
using Microsoft.Maui.Controls;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views
{
    public partial class PageSettingAntenna : ContentPage
    {
        public PageSettingAntenna()
        {
            InitializeComponent();

            // TODO: Fix BleMvxApplication references for MAUI
            /*
            // the page only support 4 ports
            switch (BleMvxApplication._reader.rfid.GetAntennaPort())
            {
                case 2:
                    stacklayoutAntenna4.IsVisible = false;
                    break;

                case 4:
                    stacklayoutAntenna4.IsVisible = true;
                    break;

                default:
                    return;
            }

            switch (BleMvxApplication._reader.rfid.GetModel())
            {
                case CSLibrary.RFIDDEVICE.MODEL.CS203XL:
                    labelAntenna1.Text = "Antenna 1 (External)";
                    labelAntenna2.Text = "Antenna 2 (Internal)";
                    break;
            }

            switchAntenna1Enable.IsToggled = BleMvxApplication._config.RFID_AntennaEnable[0];
            switchAntenna2Enable.IsToggled = BleMvxApplication._config.RFID_AntennaEnable[1];
            switchAntenna3Enable.IsToggled = BleMvxApplication._config.RFID_AntennaEnable[2];
            switchAntenna4Enable.IsToggled = BleMvxApplication._config.RFID_AntennaEnable[3];

            entryPower1.Text = BleMvxApplication._config.RFID_Antenna_Power[0].ToString();
            entryPower2.Text = BleMvxApplication._config.RFID_Antenna_Power[1].ToString();
            entryPower3.Text = BleMvxApplication._config.RFID_Antenna_Power[2].ToString();
            entryPower4.Text = BleMvxApplication._config.RFID_Antenna_Power[3].ToString();

            entryDwell1.Text = BleMvxApplication._config.RFID_Antenna_Dwell[0].ToString();
            entryDwell2.Text = BleMvxApplication._config.RFID_Antenna_Dwell[1].ToString();
            entryDwell3.Text = BleMvxApplication._config.RFID_Antenna_Dwell[2].ToString();
            entryDwell4.Text = BleMvxApplication._config.RFID_Antenna_Dwell[3].ToString();
            */

            // Temporary initialization for demo purposes
            stacklayoutAntenna4.IsVisible = true; // Assume 4 antenna model
            
            switchAntenna1Enable.IsToggled = true;
            switchAntenna2Enable.IsToggled = true;
            switchAntenna3Enable.IsToggled = false;
            switchAntenna4Enable.IsToggled = false;

            entryPower1.Text = "300";
            entryPower2.Text = "300";
            entryPower3.Text = "300";
            entryPower4.Text = "300";

            entryDwell1.Text = "2000";
            entryDwell2.Text = "2000";
            entryDwell3.Text = "2000";
            entryDwell4.Text = "2000";
        }

        protected override void OnAppearing()
        {
            // TODO: Fix BleMvxApplication references for MAUI
            /*
            if (BleMvxApplication._settingPage1TagPopulationChanged)
            {
                BleMvxApplication._settingPage1TagPopulationChanged = false;
            }
            */

            base.OnAppearing();
        }

        public async void btnOKClicked(object sender, EventArgs e)
        {
            // TODO: Implement sound service for MAUI
            // DependencyService.Get<ISystemSound>().SystemSound(1);

            // TODO: Fix BleMvxApplication references for MAUI
            /*
            BleMvxApplication._config.RFID_AntennaEnable[0] = switchAntenna1Enable.IsToggled;
            BleMvxApplication._config.RFID_AntennaEnable[1] = switchAntenna2Enable.IsToggled;
            BleMvxApplication._config.RFID_AntennaEnable[2] = switchAntenna3Enable.IsToggled;
            BleMvxApplication._config.RFID_AntennaEnable[3] = switchAntenna4Enable.IsToggled;

            BleMvxApplication._config.RFID_Antenna_Power[0] = uint.Parse(entryPower1.Text);
            BleMvxApplication._config.RFID_Antenna_Power[1] = uint.Parse(entryPower2.Text);
            BleMvxApplication._config.RFID_Antenna_Power[2] = uint.Parse(entryPower3.Text);
            BleMvxApplication._config.RFID_Antenna_Power[3] = uint.Parse(entryPower4.Text);

            BleMvxApplication._config.RFID_Antenna_Dwell[0] = uint.Parse(entryDwell1.Text);
            BleMvxApplication._config.RFID_Antenna_Dwell[1] = uint.Parse(entryDwell2.Text);
            BleMvxApplication._config.RFID_Antenna_Dwell[2] = uint.Parse(entryDwell3.Text);
            BleMvxApplication._config.RFID_Antenna_Dwell[3] = uint.Parse(entryDwell4.Text);

            BleMvxApplication.SaveConfig();

            for (int cnt = 0; cnt < BleMvxApplication._reader.rfid.GetAntennaPort(); cnt++)
            {
                BleMvxApplication._reader.rfid.SetAntennaPortState((uint)cnt, BleMvxApplication._config.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[cnt], cnt);
                BleMvxApplication._reader.rfid.SetInventoryDuration(BleMvxApplication._config.RFID_Antenna_Dwell[cnt], (uint)cnt);
            }
            */

            await DisplayAlert("Antenna Settings", "Antenna settings saved successfully!", "OK");
        }
    }
}