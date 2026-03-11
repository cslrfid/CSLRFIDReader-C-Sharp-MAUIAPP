using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Services
{
    public interface IMauiDialogService
    {
        Task ShowAlertAsync(string message, string title = "Alert", string buttonText = "OK");
        Task<bool> ShowConfirmAsync(string message, string title = "Confirm", string acceptText = "OK", string cancelText = "Cancel");
        Task ShowLoadingAsync(string message = "Loading...");
        Task HideLoadingAsync();
        Task ShowToastAsync(string message, ToastLevel level = ToastLevel.Info, int durationMilliseconds = 3000);
    }

    public enum ToastLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class MauiDialogService : IMauiDialogService
    {
        private bool _isLoadingVisible = false;
        private Page? _loadingPage;

        private Page? GetCurrentPage()
        {
            try
            {
                // 獲取當前活動頁面
                if (Application.Current?.MainPage is Shell shell)
                    return shell.CurrentPage;

                if (Application.Current?.MainPage is NavigationPage navPage)
                    return navPage.CurrentPage;

                if (Application.Current?.MainPage is TabbedPage tabbedPage && tabbedPage.CurrentPage != null)
                {
                    if (tabbedPage.CurrentPage is NavigationPage tabNavPage)
                        return tabNavPage.CurrentPage;
                    return tabbedPage.CurrentPage;
                }

                return Application.Current?.MainPage;
            }
            catch
            {
                return Application.Current?.MainPage;
            }
        }

        public async Task ShowAlertAsync(string message, string title = "Alert", string buttonText = "OK")
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var page = GetCurrentPage();
                    if (page != null)
                    {
                        await page.DisplayAlert(title, message, buttonText);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowAlertAsync error: {ex.Message}");
            }
        }

        public async Task<bool> ShowConfirmAsync(string message, string title = "Confirm", string acceptText = "OK", string cancelText = "Cancel")
        {
            try
            {
                return await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var page = GetCurrentPage();
                    if (page != null)
                    {
                        return await page.DisplayAlert(title, message, acceptText, cancelText);
                    }
                    return false;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowConfirmAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task ShowLoadingAsync(string message = "Loading...")
        {
            if (_isLoadingVisible) return;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var loadingPage = new ContentPage
                    {
                        BackgroundColor = Color.FromRgba(0, 0, 0, 0.5),
                        Content = new Grid
                        {
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            Children =
                            {
                                new Frame
                                {
                                    BackgroundColor = Colors.White,
                                    CornerRadius = 10,
                                    Padding = 30,
                                    HasShadow = true,
                                    Content = new StackLayout
                                    {
                                        Orientation = StackOrientation.Vertical,
                                        Spacing = 15,
                                        Children =
                                        {
                                            new ActivityIndicator
                                            {
                                                IsRunning = true,
                                                Color = Colors.Blue,
                                                VerticalOptions = LayoutOptions.Center,
                                                HorizontalOptions = LayoutOptions.Center
                                            },
                                            new Label
                                            {
                                                Text = message,
                                                FontSize = 16,
                                                TextColor = Colors.Black,
                                                HorizontalTextAlignment = TextAlignment.Center,
                                                VerticalOptions = LayoutOptions.Center
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };

                    _loadingPage = loadingPage;
                    _isLoadingVisible = true;

                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(loadingPage, false);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowLoadingAsync error: {ex.Message}");
                _isLoadingVisible = false;
                _loadingPage = null;
            }
        }

        public async Task HideLoadingAsync()
        {
            if (!_isLoadingVisible || _loadingPage == null) return;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        if (Application.Current?.MainPage?.Navigation.ModalStack.Contains(_loadingPage) == true)
                        {
                            await Application.Current.MainPage.Navigation.PopModalAsync(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"PopModalAsync error: {ex.Message}");
                    }
                    finally
                    {
                        _isLoadingVisible = false;
                        _loadingPage = null;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HideLoadingAsync error: {ex.Message}");
                _isLoadingVisible = false;
                _loadingPage = null;
            }
        }

        public async Task ShowToastAsync(string message, ToastLevel level = ToastLevel.Info, int durationMilliseconds = 3000)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var page = GetCurrentPage();
                    if (page is not ContentPage contentPage || contentPage.Content is not Grid mainGrid)
                    {
                        // 如果頁面沒有 Grid，使用簡單的 Alert 作為後備
                        if (page != null)
                        {
                            var title = level switch
                            {
                                ToastLevel.Success => "Success",
                                ToastLevel.Warning => "Warning",
                                ToastLevel.Error => "Error",
                                _ => "Message"
                            };
                            await page.DisplayAlert(title, message, "OK");
                        }
                        return;
                    }

                    var backgroundColor = level switch
                    {
                        ToastLevel.Success => Colors.Green,
                        ToastLevel.Warning => Colors.Orange,
                        ToastLevel.Error => Colors.Red,
                        _ => Colors.Gray
                    };

                    var toastFrame = new Frame
                    {
                        BackgroundColor = backgroundColor,
                        CornerRadius = 10,
                        Padding = 15,
                        Margin = new Thickness(20),
                        VerticalOptions = LayoutOptions.End,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        HasShadow = true,
                        Content = new Label
                        {
                            Text = message,
                            TextColor = Colors.White,
                            FontSize = 14,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        }
                    };

                    // 添加到 Grid 的最上層
                    Grid.SetRowSpan(toastFrame, mainGrid.RowDefinitions.Count > 0 ? mainGrid.RowDefinitions.Count : 1);
                    Grid.SetColumnSpan(toastFrame, mainGrid.ColumnDefinitions.Count > 0 ? mainGrid.ColumnDefinitions.Count : 1);
                    
                    mainGrid.Children.Add(toastFrame);

                    // 延遲移除
                    _ = Task.Delay(durationMilliseconds).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                if (mainGrid.Children.Contains(toastFrame))
                                {
                                    mainGrid.Children.Remove(toastFrame);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Remove toast error: {ex.Message}");
                            }
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowToastAsync error: {ex.Message}");
            }
        }
    }

    // 擴展方法來模擬 Acr.UserDialogs 的 API
    public static class MauiDialogServiceExtensions
    {
        public static async Task Alert(this IMauiDialogService dialogService, string message, string title = "Alert")
        {
            await dialogService.ShowAlertAsync(message, title);
        }

        public static async Task AlertAsync(this IMauiDialogService dialogService, string message, string title = "Alert")
        {
            await dialogService.ShowAlertAsync(message, title);
        }

        public static async Task<bool> ConfirmAsync(this IMauiDialogService dialogService, string message, string title = "Confirm")
        {
            return await dialogService.ShowConfirmAsync(message, title);
        }

        public static async Task ShowLoading(this IMauiDialogService dialogService, string message = "Loading...")
        {
            await dialogService.ShowLoadingAsync(message);
        }

        public static async Task HideLoading(this IMauiDialogService dialogService)
        {
            await dialogService.HideLoadingAsync();
        }

        public static async Task Toast(this IMauiDialogService dialogService, string message)
        {
            await dialogService.ShowToastAsync(message);
        }
    }
}