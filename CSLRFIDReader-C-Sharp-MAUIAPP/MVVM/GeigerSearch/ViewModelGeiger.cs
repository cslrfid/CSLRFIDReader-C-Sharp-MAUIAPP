using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CSLHandheldReader_C_Sharp_MAUIAPP.Services;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.ViewModels
{
    public class ViewModelGeiger : INotifyPropertyChanged
    {
        public ICommand AppearingCommand { get; }
        public ICommand DisappearingCommand { get; }
        public ICommand OnStartGeigerButtonCommand { protected set; get; }

        private int _rssidBuV = 0;
        private string _rssiString = "RSSI";
        public string rssiStart { get { return _rssiString; } }

        double _progressbarRSSIValue = 0;
        public double progressbarRSSIValue { get { return _progressbarRSSIValue; } }

        private string _startGeigerButtonText = "Start";
        public string startGeigerButtonText { get { return _startGeigerButtonText; } }

        private int _buttonBank = 1;
        public int buttonBank { get { return _buttonBank; } set { _buttonBank = value; } }

        private string _entryEPC = "";
        public string entryEPC { get { return _entryEPC; } set { _entryEPC = value; } }

        private uint _power = 300;
        public uint power { get { return _power; } set { _power = value; } }

        private int _Threshold = 0;
        public string labelThresholdValueText { get { return _Threshold.ToString(); } set { try { _Threshold = int.Parse(value); } catch (Exception) { } } }

        bool _startInventory = false;
        public bool _KeyDown = false;
        int _beepSoundCount = 0;
        int _noTagCount = 0;

        public ViewModelGeiger()
        {
            AppearingCommand = new Command(OnAppearing);
            DisappearingCommand = new Command(OnDisappearing);
            OnStartGeigerButtonCommand = new Command(StartGeigerButtonClick);

            // Initialize with global variable equivalent
            _entryEPC = GlobalVariable._SELECT_EPC ?? "";

            OnPropertyChanged(nameof(entryEPC));
            _Threshold = GlobalVariable._config?.RFID_DBm == true ? -47 : 60;

            InventorySetting();
        }

        ~ViewModelGeiger()
        {
        }

        private void SetEvent(bool enable)
        {
            // Cancel RFID event handler
            GlobalVariable._reader.rfid.ClearEventHandler();

            // Key Button event handler
            GlobalVariable._reader.notification.ClearEventHandler();

            if (enable)
            {
                GlobalVariable._reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagSearchOneEvent);

                // Key Button event handler
                GlobalVariable._reader.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
            }
        }

        public void OnAppearing()
        {
            SetEvent(true);
        }

        public void OnDisappearing()
        {
            // don't turn off event handler is you need program work in sleep mode.
            StopGeiger();
            SetEvent(false);
        }

        void InventorySetting()
        {
            // Cancel old setting
            GlobalVariable._reader.rfid.CancelAllSelectCriteria();
            GlobalVariable._reader.rfid.SetPowerSequencing(0);

            // Set Geiger parameters
            GlobalVariable._reader.rfid.SetInventoryDuration(GlobalVariable._config?.RFID_Antenna_Dwell);
            GlobalVariable._reader.rfid.SetTagDelayTime((uint)(GlobalVariable._config?.RFID_CompactInventoryDelayTime)); // for CS108 only
            GlobalVariable._reader.rfid.SetIntraPacketDelayTime((uint)(GlobalVariable._config?.RFID_IntraPacketDelayTime)); // for CS710S only
            GlobalVariable._reader.rfid.SetDuplicateEliminationRollingWindow(0);
            
            if (GlobalVariable._config?.RFID_FixedQParms != null)
            {
                GlobalVariable._config.RFID_FixedQParms.qValue = 1;
                GlobalVariable._config.RFID_FixedQParms.toggleTarget = 1;
                GlobalVariable._reader.rfid.SetFixedQParms(GlobalVariable._config.RFID_FixedQParms);
            }
            
            GlobalVariable._reader.rfid.SetCurrentSingulationAlgorithm(CSLibrary.Constants.SingulationAlgorithm.FIXEDQ);
            GlobalVariable._reader.rfid.SetRSSIFilter(CSLibrary.Constants.RSSIFILTERTYPE.DISABLE);

            // Multi bank inventory
            GlobalVariable._reader.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.SELECT;
            GlobalVariable._reader.rfid.Options.TagRanging.multibanks = 0;
            GlobalVariable._reader.rfid.Options.TagRanging.compactmode = true;
            GlobalVariable._reader.rfid.Options.TagRanging.focus = GlobalVariable._config?.RFID_Focus ?? false;
        }

        void StartGeiger()
        {
            if (_startInventory)
                return;

            _startGeigerButtonText = "Stop";
            _startInventory = true;

            OnPropertyChanged(nameof(entryEPC));
            OnPropertyChanged(nameof(power));

            GlobalVariable._reader.rfid.SetPowerLevel(_power);

            GlobalVariable._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            if (GlobalVariable._geiger_Bank == 1) // if EPC
            {
                GlobalVariable._reader.rfid.Options.TagSelected.bank = CSLibrary.Constants.MemoryBank.EPC;
                GlobalVariable._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(_entryEPC);
                GlobalVariable._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
                GlobalVariable._reader.rfid.Options.TagSelected.epcMaskLength = (uint)_entryEPC.Length * 4;
            }
            else
            {
                GlobalVariable._reader.rfid.Options.TagSelected.bank = (CSLibrary.Constants.MemoryBank)GlobalVariable._geiger_Bank;
                GlobalVariable._reader.rfid.Options.TagSelected.Mask = CSLibrary.Tools.HexEncoding.ToBytes(_entryEPC);
                GlobalVariable._reader.rfid.Options.TagSelected.MaskOffset = 0;
                GlobalVariable._reader.rfid.Options.TagSelected.MaskLength = (uint)_entryEPC.Length * 4;
            }
            GlobalVariable._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            GlobalVariable._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_RANGING);

            OnPropertyChanged(nameof(startGeigerButtonText));

            // Create a beep sound timer.
            _beepSoundCount = 0;
            Application.Current?.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                if (_rssidBuV == 0)
                {
                    _noTagCount++;

                    if (_noTagCount > 2)
                        PlaySystemSound(-1);
                }
                else
                {
                    if (_beepSoundCount == 0 && _rssidBuV >= 20 && _rssidBuV < 60)
                        PlaySystemSound(3);

                    _beepSoundCount++;

                    if ((GlobalVariable._config?.RFID_DBm == true && CSLibrary.Tools.dBConverion.dBuV2dBm(_rssidBuV) >= _Threshold) ||
                        (GlobalVariable._config?.RFID_DBm != true && _rssidBuV >= _Threshold))
                    {
                        PlaySystemSound(4);
                        _beepSoundCount = 1;
                        _rssidBuV = 0;
                    }
                    else if (_rssidBuV >= 50)
                    {
                        if (_beepSoundCount >= 5)
                        {
                            _beepSoundCount = 0;
                            _rssidBuV = 0;
                        }
                    }
                    else if (_rssidBuV >= 40)
                    {
                        if (_beepSoundCount >= 10)
                        {
                            _beepSoundCount = 0;
                            _rssidBuV = 0;
                        }
                    }
                    else if (_rssidBuV >= 30)
                    {
                        if (_beepSoundCount >= 20)
                        {
                            _beepSoundCount = 0;
                            _rssidBuV = 0;
                        }
                    }
                    else if (_rssidBuV >= 20)
                    {
                        if (_beepSoundCount >= 40)
                        {
                            _beepSoundCount = 0;
                            _rssidBuV = 0;
                        }
                    }
                }

                if (_startInventory)
                    return true;

                // Stop all sound
                PlaySystemSound(-1);
                return false;
            });
        }

        void StopGeiger()
        {
            _startInventory = false;
            _startGeigerButtonText = "Start";
            GlobalVariable._reader.rfid.StopOperation();
            OnPropertyChanged(nameof(startGeigerButtonText));
        }

        void StartGeigerButtonClick()
        {
            if (!_startInventory)
            {
                StartGeiger();
            }
            else
            {
                StopGeiger();
            }
        }

        public void TagSearchOneEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            switch (e.type)
            {
                case CSLibrary.Constants.CallbackType.TAG_RANGING:

                    _rssidBuV = (int)Math.Round(e.info.rssi);
                    _noTagCount = 0;

                    if (GlobalVariable._config?.RFID_DBm == true)
                    {
                        // 0~1
                        _progressbarRSSIValue = e.info.rssidBm;
                    }
                    else
                    {
                        // 0~1
                        _progressbarRSSIValue = e.info.rssi;
                    }
                    _rssiString = ((int)Math.Round(_progressbarRSSIValue)).ToString();

                    OnPropertyChanged(nameof(rssiStart));
                    OnPropertyChanged(nameof(progressbarRSSIValue));
                    break;
            }
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
            switch (e.state)
            {
                case CSLibrary.Constants.RFState.IDLE:
                    break;
            }
        }

        bool CheckPageActive()
        {
            try
            {
                if (Shell.Current?.CurrentPage != null)
                {
                    return Shell.Current.CurrentPage.Title == "Geiger";
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
            if (!CheckPageActive())
                return;

            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON)
            {
                if (e.KeyDown)
                {
                    if (!_KeyDown)
                        StartGeiger();
                    _KeyDown = true;
                }
                else
                {
                    if (_KeyDown == true)
                        StopGeiger();
                    _KeyDown = false;
                }
            }
        }

        private async void PlaySystemSound(int soundType)
        {
            // Use MAUI sound service or platform-specific implementation
            try
            {
                // Simple implementation - could be enhanced with proper sound management
                /*
                var audioManager = ServiceHelper.GetService<Plugin.Maui.Audio.IAudioManager>();
                if (audioManager != null)
                {
                    switch (soundType)
                    {
                        case 3:
                            await SoundPlayer.PlaySound(audioManager, SoundSelect.BEEP3S);
                            break;
                        case 4:
                            await SoundPlayer.PlaySound(audioManager, SoundSelect.BEEPHIGH);
                            break;
                        case -1:
                            // Stop sound - implementation depends on sound system
                            break;
                    }
                }*/
            }
            catch (Exception)
            {
                // Handle sound playing error
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

/*
   // Helper class to get services
    public static class ServiceHelper
    {
        public static T? GetService<T>() => ServiceProvider?.GetService<T>();
        
        public static IServiceProvider? ServiceProvider
        {
            get
            {
#if WINDOWS
                return MauiWinUIApplication.Current.Services;
#elif ANDROID
                return MauiApplication.Current.Services;
#elif IOS || MACCATALYST
                return MauiUIApplicationDelegate.Current.Services;
#else
                return null;
#endif
            }
        }
    }*/
}