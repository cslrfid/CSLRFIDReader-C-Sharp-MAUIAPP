# CSLRFIDReader-C-Sharp-MAUIAPP

A cross-platform mobile application for RFID reader control, built with .NET MAUI and C#. Connects to CSL RFID readers over Bluetooth (CS108, CS710S, CS203XL) or wired Ethernet (CS203XL) to perform tag inventory, read/write operations, barcode scanning, and advanced RFID functions.

## 📱 Supported Platforms

| Platform | Minimum Version | Notes |
|----------|----------------|-------|
| **Android** | API 21 (Android 5.0) | Primary target |
| **iOS** | iOS 10.0+ | iPhone/iPad |
| **MacCatalyst** | macOS 13.1+ | Mac App Store |
| **Windows** | Windows 10 (19041+) | Desktop UWP |
| **Tizen** | Tizen 6.5+ | Optional (requires separate SDK install) |

## 🔧 Requirements

- **.NET 10 SDK** or .NET 8 SDK
- **Visual Studio 2022** (Windows/Mac) or **VS Code** with MAUI extensions
- **CSLibrary2026** NuGet package (RFID reader SDK)
- A compatible CSL RFID reader:
  - **CS108** — Bluetooth handheld reader, up to 20m range
  - **CS710S** — Bluetooth rugged handheld with integrated barcode scanner
  - **CS203XL** — Fixed-mount reader with Bluetooth AND Ethernet (POE/POE+) connectivity

## 📦 Features

### Core Functions

| Feature | Description |
|---------|-------------|
| **Device Connection** | Bluetooth LE or Ethernet TCP/IP connection to CSL RFID readers (CS108, CS710S, CS203XL) |
| **Tag Inventory** | Real-time RFID tag scanning and counting |
| **Read/Write** | Read and write data to RFID tag memory banks (EPC, TID, USER, etc.) |
| **Register Tag** | Register/provision tags with custom data |
| **RFID Filter** | Pre-filter and post-filter tags by EPC, TID, RSSI, and other criteria |
| **Geiger Search** | Signal strength-based tag location (like a Geiger counter) |
| **Security/Kill** | Tag kill and security lock operations |

### Special Functions

| Feature | Description |
|---------|-------------|
| **Barcode + RFID Inventory** | Combined 1D/2D barcode scanning with RFID tag reading |
| **LED Tag Control** | Control LED-lit tags for visual item identification |
| **MQTT Inventory** | Publish RFID data to MQTT brokers for IoT integration |
| **Multi-Bank Inventory** | Read multiple memory banks (EPC+TID+USER) in one pass |
| **Impinj Special Features** | Impinj FastID, Focus, and other extensions |
| **Magnus S2/S3 Support** | Support for Magnus S2 and S3 tag ICs |
| **Temperature + GPS Demo** | Temperature sensing tags with location demo |
| **UCODE 8 Support** | Support for NXP UCODE 8 tags |
| **Xerxes Authentication** | Axzon Xerxes tag authentication and operations |
| **ASYGN Support** | ASYGN tag inventory operations |
| **Inventory to Wedge Forwarder** | Forward scanned data as keyboard input |

### Settings

- Antenna configuration (power level, dwell time)
- Operation profiles (singulation algorithm, session, target)
- Power sequencing
- Reader administration (firmware info, country/region settings)

## 🏗️ Architecture

```
CSLRFIDReader-C-Sharp-MAUIAPP/
├── MVVM/                          # New MAUI UI (recommended)
│   ├── ConnectReader/              # Device discovery & connection
│   ├── Inventory/                  # Tag inventory view
│   ├── GeigerSearch/               # Geiger counter search
│   ├── Settings/                  # App configuration pages
│   └── PageMainMenu.xaml          # Main menu shell
├── Xamarin_CrossMVVM/             # Legacy Xamarin UI (cross-platform)
│   ├── ConnectReader/              # Device discovery & connection
│   ├── Inventory/                 # Tag inventory view
│   ├── ReadWrite/                 # Tag read/write operations
│   ├── RegisterTag/               # Tag registration
│   ├── SecurityKill/              # Security & kill operations
│   ├── RFIDFilter/                # Pre/post filtering
│   ├── GeigerSearch/              # Geiger counter mode
│   ├── SpecialFuction/            # Advanced features
│   │   ├── BarcodeandRFIDInventory/
│   │   ├── LEDTag/
│   │   ├── MQTT/
│   │   ├── MultiBankInventory/
│   │   ├── ImpinjSpecialFeatures/
│   │   ├── MagnusS2orS3/
│   │   ├── Temperature tag with location demo for Tablet/
│   │   ├── UCODE8/
│   │   ├── Xerxes/
│   │   └── ASYGN/
│   └── Settings/                   # Configuration pages
├── Services/                       # App services
│   ├── GlobalVariable.cs           # Global state
│   ├── MauiDialogService.cs        # Dialog service
│   ├── SoundPlayer.cs              # Audio feedback
│   └── VersionTools.cs             # Version utilities
├── Platforms/                      # Platform-specific code
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   ├── Windows/
│   └── Tizen/
└── Resources/                       # Fonts, images, sounds
```

## 🚀 Build Instructions

### Prerequisites

1. Install [.NET SDK 8 or 10](https://dotnet.microsoft.com/download)
2. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) with MAUI workload
   - Or [VS Code](https://code.visualstudio.com/) with C# and MAUI extensions

### Restore and Build

```bash
# Clone the repository
git clone https://github.com/cslrfid/CSLRFIDReader-C-Sharp-MAUIAPP.git
cd CSLRFIDReader-C-Sharp-MAUIAPP

# Navigate to solution
cd CSLRFIDReader-C-Sharp-MAUIAPP

# Restore NuGet packages
dotnet restore

# Build for Android
dotnet build -f net10.0-android

# Build for iOS
dotnet build -f net10.0-ios

# Build for Windows
dotnet build -f net10.0-windows10.0.26100.0
```

### Run on Device

```bash
# Android (via USB debug or emulator)
dotnet build -t:Run -f net10.0-android

# iOS (requires Mac with Xcode)
dotnet build -t:Run -f net10.0-ios -p:CodesignKey="..."

# Windows
dotnet build -t:Run -f net10.0-windows10.0.26100.0
```

## 📦 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| **CSLibrary2026** | Latest | CSL RFID reader communication SDK |
| **Microsoft.Maui** | 10.x | Cross-platform UI framework |
| **CommunityToolkit.Mvvm** | Latest | MVVM pattern support |

## 📋 Compatible Hardware

| Reader | Connection | Description |
|--------|-----------|-------------|
| **CS108** | Bluetooth LE | Handheld reader, up to 20m range |
| **CS710S** | Bluetooth LE | Rugged handheld with integrated barcode scanner |
| **CS203XL** | Bluetooth LE / Ethernet (POE/POE+) | Fixed-mount reader, up to 35m range, IP68 |

This app supports all CSL readers compatible with CSLibrary2026.

## 📄 License

This project is proprietary software from **Convergence Systems Limited (CSL)**. All rights reserved.

For licensing inquiries, contact: [info@convergence.com.hk](mailto:info@convergence.com.hk)

## 🔗 Related Projects

- [CSLibrary2026](https://github.com/cslrfid/CSLibrary2026) — **Standalone** core RFID SDK (separate repository, separate NuGet package)
- [CS203XL-C-Sharp-APP-for-iOS-ANDROID-UWP](https://github.com/cslrfid/CS203XL-C-Sharp-APP-for-iOS-ANDROID-UWP) — Reference app for CS203XL (CSLibrary2024 over TCP/IP)
- [CSL RFID Reader SDKs](https://github.com/cslrfid) — Full SDK ecosystem

---

**Convergence Systems Limited** — RFID Hardware & Solutions  
https://www.convergence.com.hk
