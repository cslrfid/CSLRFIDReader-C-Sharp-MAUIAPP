using CSLibrary.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Models
{
    public class TagInfo : INotifyPropertyChanged
    {
        private string _epc;
        private string _epc_Display;
        private UInt16 _pc;
        private string _bank1Data;
        private string _bank2Data;
        private int _rssi_Format;
        private float _rssi;
        private Int16 _phase;
        private int _channel;
        private DateTime _timeOfRead;
        private string _locationOfRead;
        private string _eCompass;

        public string EPC 
        { 
            get => _epc; 
            set { _epc = value; OnPropertyChanged(); } 
        }

        public string EPC_Display 
        { 
            get => _epc_Display; 
            set { _epc_Display = value; OnPropertyChanged(); } 
        }

        public UInt16 PC 
        { 
            get => _pc; 
            set { _pc = value; OnPropertyChanged(); } 
        }

        public string Bank1Data 
        { 
            get => _bank1Data; 
            set { _bank1Data = value; OnPropertyChanged(); } 
        }

        public string Bank2Data 
        { 
            get => _bank2Data; 
            set { _bank2Data = value; OnPropertyChanged(); } 
        }

        public int RSSI_Format 
        { 
            get => _rssi_Format; 
            set { _rssi_Format = value; OnPropertyChanged(); } 
        }

        public float RSSI 
        { 
            get => _rssi; 
            set { _rssi = value; OnPropertyChanged(); } 
        }

        public Int16 Phase 
        { 
            get => _phase; 
            set { _phase = value; OnPropertyChanged(); } 
        }

        public int Channel 
        { 
            get => _channel; 
            set { _channel = value; OnPropertyChanged(); } 
        }

        public DateTime timeOfRead 
        { 
            get => _timeOfRead; 
            set { _timeOfRead = value; OnPropertyChanged(); } 
        }

        public string locationOfRead 
        { 
            get => _locationOfRead; 
            set { _locationOfRead = value; OnPropertyChanged(); } 
        }

        public string eCompass 
        { 
            get => _eCompass; 
            set { _eCompass = value; OnPropertyChanged(); } 
        }

        public TagInfo()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
