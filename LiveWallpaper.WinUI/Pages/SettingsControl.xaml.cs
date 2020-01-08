 using LiveWallpaper.WinUI.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LiveWallpaper.WinUI.Pages
{
    public sealed partial class SettingsControl : UserControl, INotifyPropertyChanged
    {
        public SettingsControl()
        {
            this.InitializeComponent();
        }

        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private async void ThemeChanged_CheckedAsync(object sender, RoutedEventArgs e)
        {
            var param = (sender as RadioButton)?.CommandParameter;

            if (param != null)
            {
                await ThemeSelectorService.SetThemeAsync((ElementTheme)param);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            var mainPage = rootFrame.Content as MainPage;
            mainPage.GoBack();
        }
    }
}
