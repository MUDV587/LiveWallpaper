using LiveWallpaper.WinUI.Pages;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LiveWallpaper.WinUI
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private object _previousSelectedNaviItem;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            settingTranslation.Y = ActualHeight;
            if (SystemInformation.IsFirstRun || Debugger.IsAttached)
                teachTipAddLocalwapaper.IsOpen = true;
        }

        internal void GoBack()
        {
            settingTransStoryboard.Children[0].SetValue(DoubleAnimation.FromProperty, settingTranslation.Y);
            settingTransStoryboard.Children[0].SetValue(DoubleAnimation.ToProperty, ActualHeight);
            settingTransStoryboard.Begin();
            nvShell.SelectedItem = _previousSelectedNaviItem;
        }

        private void nvShell_SelectionChanged(
            Microsoft.UI.Xaml.Controls.NavigationView sender,
            Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                teachTipAddLocalwapaper.IsOpen = false;
                cbFlyout.Visibility = Visibility.Collapsed;
                settingTransStoryboard.Children[0].SetValue(DoubleAnimation.FromProperty, settingTranslation.Y);
                settingTransStoryboard.Children[0].SetValue(DoubleAnimation.ToProperty, 0);
                settingTransStoryboard.Begin();
            }
            else
            {
                var navOptions = new FrameNavigationOptions();
                navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
                if (sender.PaneDisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top)
                {
                    navOptions.IsNavigationStackEnabled = false;
                }

                Type pageType = null;
                if (args.SelectedItem == nviLocalWallpaper)
                {
                    cbFlyout.Visibility = Visibility.Visible;
                    abBtnAddwallpaper.Visibility = Visibility.Visible;
                    abBtnDownloadWallpaper.Visibility = Visibility.Collapsed;
                    pageType = typeof(LocalWallpaperPage);
                }
                else if (args.SelectedItem == nviWallpaperStore)
                {
                    cbFlyout.Visibility = Visibility.Visible;
                    abBtnAddwallpaper.Visibility = Visibility.Collapsed;
                    abBtnDownloadWallpaper.Visibility = Visibility.Visible;
                    pageType = typeof(WallpaperStorePage);
                }

                if (pageType != null)
                {
                    _previousSelectedNaviItem = args.SelectedItem;
                    contentFrame.NavigateToType(pageType, null, navOptions);
                }
            }
        }
    }
}
