using CSLHandheldReader_C_Sharp_MAUIAPP.Maui.ViewModels;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using CSLHandheldReader_C_Sharp_MAUIAPP.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views;

public partial class PageDeviceList : ContentPage
{
    private DeviceListViewModel _viewModel;

    public PageDeviceList()
    {
        InitializeComponent();
        
        // Initialize Bluetooth LE
        var ble = CrossBluetoothLE.Current;
        var adapter = ble.Adapter;
        
        // Get dialog service from dependency injection
        var dialogService = Handler?.MauiContext?.Services.GetService<IMauiDialogService>() ?? new MauiDialogService();
        
        // Create and set the view model
        _viewModel = new DeviceListViewModel(ble, adapter, dialogService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Check and request permissions before starting scan
        await CheckAndRequestPermissions();
        await _viewModel.ViewAppearing();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _viewModel.ViewDisappearing();
    }
    
    private async Task CheckAndRequestPermissions()
    {
        try
        {
            // For Android, we need location permission for Bluetooth scanning
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                {
                    locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (locationStatus != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Permission Required", 
                            "Location permission is required for Bluetooth scanning on Android. Please grant permission in app settings.", 
                            "OK");
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Permission check error: {ex.Message}");
        }
    }

    private void OnBluetoothModeClicked(object sender, EventArgs e)
    {
        if (_viewModel != null)
            _viewModel.ConnectionMode = ConnectionModeEnum.Bluetooth;
    }

    private void OnTcpModeClicked(object sender, EventArgs e)
    {
        if (_viewModel != null)
            _viewModel.ConnectionMode = ConnectionModeEnum.TCP;
    }
}
