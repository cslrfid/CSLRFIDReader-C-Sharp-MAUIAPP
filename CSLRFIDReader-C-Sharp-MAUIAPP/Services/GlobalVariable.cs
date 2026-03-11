using System;
using CSLibrary;
using CSLHandheldReader_C_Sharp_MAUIAPP.Services;
using Plugin.BLE.Abstractions.Contracts;

namespace CSLHandheldReader_C_Sharp_MAUIAPP
{
    public static class GlobalVariable
    {
        // Core components
        public static HighLevelInterface _reader = new HighLevelInterface();
        public static CONFIG _config;

        // System
        public static IDevice _deviceinfo;

        // for Geiger and Read/Write
        public static string _SELECT_EPC = "";
        public static string _SELECT_TID = "";
        public static UInt16 _SELECT_PC = 0x0000;

        // for PreFilter
        public static string _PREFILTER_MASK_EPC = "";
        public static uint _PREFILTER_MASK_Offset = 0;
        public static int _PREFILTER_MASK_Truncate = 0;
        public static int _PREFILTER_Bank = 1;
        public static bool _PREFILTER_Enable = false;

        // for Post Filter
        public static string _POSTFILTER_MASK_EPC = "";
        public static uint _POSTFILTER_MASK_Offset = 0;
        public static bool _POSTFILTER_MASK_MatchNot = false;
        public static bool _POSTFILTER_MASK_Enable = false;

        // for RSSI Filter
        public static CSLibrary.Constants.RSSIFILTERTYPE _RSSIFILTER_Type = CSLibrary.Constants.RSSIFILTERTYPE.DISABLE;
        public static CSLibrary.Constants.RSSIFILTEROPTION _RSSIFILTER_Option = CSLibrary.Constants.RSSIFILTEROPTION.GREATEROREQUAL;
        public static double _RSSIFILTER_Threshold_dBm = 0;

        public static int _inventoryEntryPoint = 0;
        public static bool _settingPage1TagPopulationChanged = false;
        public static bool _settingPage3QvalueChanged = false;
        public static bool _settingPage4QvalueChanged = false;

        // for Cloud server
        public static UInt16 _sequenceNumber = 0;

        // for battery level display
        public static bool _batteryLow = false;

        // for RFMicro
        public static int _rfMicro_TagType; // 0 = S2, 1 = S3, 2 = Axzon
        public static int _rfMicro_Power; // 0 ~ 4
        public static int _rfMicro_Target; // 0 = A, 1 = B, 2 = Toggle
        public static int _rfMicro_SensorType; // 0=Sensor code, 1=Temperature
        public static int _rfMicro_SensorUnit; // 0 = Average value, 1 = RAW, 2 = Temperature F, 3 = Temperature C, 4 = Dry/Wet
        public static int _rfMicro_minOCRSSI;
        public static int _rfMicro_maxOCRSSI;
        public static int _rfMicro_thresholdComparison; // 0 ~ 1
        public static int _rfMicro_thresholdValue;
        public static string _rfMicro_thresholdColor;
        public static int _rfMicro_WetDryThresholdValue;

        //for ColdChain
        public static int _coldChain_TempOffset;
        public static int _coldChain_Temp1THUnder;
        public static int _coldChain_Temp1THOver;
        public static int _coldChain_Temp1THCount;
        public static int _coldChain_Temp2THUnder;
        public static int _coldChain_Temp2THOver;
        public static int _coldChain_Temp2THCount;
        public static int _coldChain_LogInterval;

        // for Xerxes Tag
        //public static int _xerxes_Power;
        //public static int _xerxes_Target;
        public static int _xerxes_delay;

        // for Focus and Fast ID
        public static Boolean _focus = false;
        public static Boolean _fastID = false;

        // for Geiger Demo 
        public static int _geiger_Bank = 1;

        // for Large Content
        public static string _LargeContent = "";

        // LED Tag
        public static bool _LEDTag_Selected = false;

        // Wedge Forwarder
        public static string _WedgeIP = "127.0.0.1";
        public static int _WedgePort = 9394;
        public static int _WedgeDuplicateFilter = 0;
        public static int _WedgeRollingWindows = 1;
    }
}