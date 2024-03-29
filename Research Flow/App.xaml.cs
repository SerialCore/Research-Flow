﻿using LogicService.Application;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Research_Flow
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name = "e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                ConfigureUI(rootFrame);
                rootFrame.NavigationFailed += OnNavigationFailed;
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter

                    ApplicationInfo.TrackAppUse(e);

                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.ToastNotification)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null)
                {
                    rootFrame = new Frame();
                    ConfigureUI(rootFrame);
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    Window.Current.Content = rootFrame;
                }
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage));
                }
                Window.Current.Activate();
            }
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            StorageFile file = args.Files[0] as StorageFile;
            if (file != null)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null)
                {
                    rootFrame = new Frame();
                    ConfigureUI(rootFrame);
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    Window.Current.Content = rootFrame;
                }
                if (rootFrame.Content == null)
                {
                    rootFrame.BackStack.Clear();
                    rootFrame.Navigate(typeof(MainPage), file);
                }
            }
            Window.Current.Activate();
        }

        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            ShareOperation shareOperation = args.ShareOperation;
            if (shareOperation.Data.Contains(StandardDataFormats.WebLink))
            {
                Uri link = await shareOperation.Data.GetWebLinkAsync();
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null)
                {
                    rootFrame = new Frame();
                    ConfigureUI(rootFrame);
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    Window.Current.Content = rootFrame;
                }
                if (rootFrame.Content == null)
                {
                    rootFrame.BackStack.Clear();
                    rootFrame.Navigate(typeof(MainPage), link);
                }
            }
            Window.Current.Activate();
        }

        private void ConfigureUI(Frame frame)
        {
            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            var appTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;

            coreTitleBar.ExtendViewIntoTitleBar = true;
            appTitleBar.ButtonBackgroundColor = Colors.Transparent;
            //appTitleBar.ButtonForegroundColor = Colors.Transparent;
            appTitleBar.ButtonHoverBackgroundColor = Colors.Transparent;
            //appTitleBar.ButtonHoverForegroundColor = Colors.Transparent;
            appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            //appTitleBar.ButtonInactiveForegroundColor = Colors.Transparent;

            if (ApplicationSetting.ContainKey("Theme"))
            {
                if (ApplicationSetting.EqualKey("Theme", "Light"))
                    frame.RequestedTheme = ElementTheme.Light;
                if (ApplicationSetting.EqualKey("Theme", "Dark"))
                    frame.RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                frame.RequestedTheme = ElementTheme.Default;
                if (frame.ActualTheme == ElementTheme.Light)
                    ApplicationSetting.Theme = "Light";
                if (frame.ActualTheme == ElementTheme.Dark)
                    ApplicationSetting.Theme = "Dark";
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name = "sender">The Frame which failed navigation</param>
        /// <param name = "e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name = "sender">The source of the suspend request.</param>
        /// <param name = "e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}