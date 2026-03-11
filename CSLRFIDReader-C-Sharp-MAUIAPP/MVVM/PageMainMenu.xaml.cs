using CSLHandheldReader_C_Sharp_MAUIAPP.Maui.ViewModels;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.Views
{
    public partial class PageMainMenu : ContentPage
    {
        public PageMainMenu()
        {
            InitializeComponent();
            
            // Set the BindingContext to the ViewModel
            this.BindingContext = new ViewModelMainMenu();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Call ViewModel's OnAppearing to set up event handlers
            if (this.BindingContext is ViewModelMainMenu viewModel)
            {
                viewModel.OnAppearing();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Call ViewModel's OnDisappearing to clean up event handlers
            if (this.BindingContext is ViewModelMainMenu viewModel)
            {
                viewModel.OnDisappearing();
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // Determine if we're in landscape or portrait mode
            bool isLandscape = width > height;

            // Toggle visibility for all layout grids
            ToggleLayoutVisibility(isLandscape);

            // Update app version format in ViewModel
            if (this.BindingContext is ViewModelMainMenu viewModel)
            {
                viewModel.UpdateAppVersionForOrientation(isLandscape);
            }
        }

        private void ToggleLayoutVisibility(bool isLandscape)
        {
            // Toggle for each button's layout
            for (int i = 1; i <= 8; i++)
            {
                var portraitLayout = this.FindByName<Grid>($"PortraitLayout{i}");
                var landscapeLayout = this.FindByName<Grid>($"LandscapeLayout{i}");
                
                if (portraitLayout != null && landscapeLayout != null)
                {
                    portraitLayout.IsVisible = !isLandscape;
                    landscapeLayout.IsVisible = isLandscape;
                }
            }
        }
    }
}
