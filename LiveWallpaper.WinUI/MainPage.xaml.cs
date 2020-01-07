using LiveWallpaper.WinUI.Pages;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LiveWallpaper.WinUI
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (SystemInformation.IsFirstRun)
                ToggleThemeTeachingTip1.IsOpen = true;
        }

        private void nvShell_SelectionChanged(
            Microsoft.UI.Xaml.Controls.NavigationView sender, 
            Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navOptions = new FrameNavigationOptions();
            navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            if (sender.PaneDisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top)
            {
                navOptions.IsNavigationStackEnabled = false;
            }

            if (args.IsSettingsSelected)
            {
                contentFrame.NavigateToType(typeof(SettingsPage), null, navOptions);
            }
            else
            {
                Type pageType = null;
                if (args.SelectedItem == nviLocalWallpaper)
                {
                    pageType = typeof(LocalWallpaperPage);
                }
                else if (args.SelectedItem == nviWallpaperStore)
                {
                    pageType = typeof(WallpaperStorePage);
                }
                else if (args.SelectedItem == nviAddWallpaper)
                {

                }
                else if (args.SelectedItem == nviDownloadWallpaper)
                {

                }

                if (pageType != null)
                    contentFrame.NavigateToType(pageType, null, navOptions);
            }
        }
    }
}
