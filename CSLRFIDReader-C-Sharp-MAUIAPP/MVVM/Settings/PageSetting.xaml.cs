using CSLHandheldReader_C_Sharp_MAUIAPP.Maui.ViewModels;
using CSLHandheldReader_C_Sharp_MAUIAPP.Services; 

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views
{
    public partial class PageSetting : TabbedPage
    {
        public PageSetting()
        {
            InitializeComponent();
            BindingContext = new ViewModelSetting();

            switch (GlobalVariable._reader.rfid.GetAntennaPort())
            {
                case 1: // Only one antenna, enable power config page
                    this.Children.RemoveAt(2);
                    break;

                default: // multi antenna, enable antennas setting page
                    this.Children.RemoveAt(3);
                    break;
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ViewModelSetting viewModel)
            {
                viewModel.ViewAppearing();
            }
        }

        protected override void OnDisappearing()
        {
            if (BindingContext is ViewModelSetting viewModel)
            {
                viewModel.ViewDisappearing();
            }
            base.OnDisappearing();
        }
    }
}