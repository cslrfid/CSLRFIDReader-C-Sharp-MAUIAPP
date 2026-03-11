using CSLHandheldReader_C_Sharp_MAUIAPP.Maui.ViewModels;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views;

public partial class PageInventory : ContentPage
{
	private ViewModelInventory _viewModel;

	public PageInventory()
	{
		InitializeComponent();
		_viewModel = new ViewModelInventory();
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.OnAppearing();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.OnDisappearing();
	}

    public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
	{
/*            var answer = await DisplayAlert("Select Tag", "Selected Tag for Read/Write and Geiger search", "OK", "Cancel");

        if (answer)
        {
			//BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo Items = (BLE.Client.ViewModels.ViewModelInventorynScan.TagInfo)e.SelectedItem;
			//BLE.Client.ViewModels.TagInfoViewModel Items = (BLE.Client.ViewModels.TagInfoViewModel)e.SelectedItem;

			//GlobalVariable._SELECT_EPC = Items.EPC_ORG;
            //GlobalVariable._SELECT_PC = Items.PC;
        }
*/
    }
}
