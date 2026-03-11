using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Maui.ViewModels
{
    public class ViewModelSetting : INotifyPropertyChanged
    {
        public ViewModelSetting()
        {
        }

        public void ViewAppearing()
        {
            // TODO: Fix BleMvxApplication reference for MAUI
            // BleMvxApplication._reader.siliconlabIC.OnAccessCompleted += new EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs>(OnAccessCompletedEvent);
        }

        public void ViewDisappearing()
        {
            // TODO: Fix BleMvxApplication reference for MAUI
            // BleMvxApplication._reader.siliconlabIC.OnAccessCompleted -= new EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs>(OnAccessCompletedEvent);
        }

        void OnAccessCompletedEvent(object sender, CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                switch (e.type)
                {
                    case CSLibrary.SiliconLabIC.Constants.AccessCompletedCallbackType.SERIALNUMBER:
                        Shell.Current?.DisplayAlert("Serial Number", "Serial Number : " + (string)e.info, "OK");
                        break;
                }
            });
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}