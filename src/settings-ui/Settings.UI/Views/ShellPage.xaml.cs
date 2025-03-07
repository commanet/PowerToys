﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.PowerToys.Settings.UI.Services;
using Microsoft.PowerToys.Settings.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.System;

namespace Microsoft.PowerToys.Settings.UI.Views
{
    /// <summary>
    /// Root page.
    /// </summary>
    public sealed partial class ShellPage : UserControl
    {
        /// <summary>
        /// Declaration for the ipc callback function.
        /// </summary>
        /// <param name="msg">message.</param>
        public delegate void IPCMessageCallback(string msg);

        /// <summary>
        /// Declaration for the opening oobe window callback function.
        /// </summary>
        public delegate void OobeOpeningCallback();

        /// <summary>
        /// Gets or sets a shell handler to be used to update contents of the shell dynamically from page within the frame.
        /// </summary>
        public static ShellPage ShellHandler { get; set; }

        /// <summary>
        /// Gets or sets iPC default callback function.
        /// </summary>
        public static IPCMessageCallback DefaultSndMSGCallback { get; set; }

        /// <summary>
        /// Gets or sets iPC callback function for restart as admin.
        /// </summary>
        public static IPCMessageCallback SndRestartAsAdminMsgCallback { get; set; }

        /// <summary>
        /// Gets or sets iPC callback function for checking updates.
        /// </summary>
        public static IPCMessageCallback CheckForUpdatesMsgCallback { get; set; }

        /// <summary>
        /// Gets or sets callback function for opening oobe window
        /// </summary>
        public static OobeOpeningCallback OpenOobeWindowCallback { get; set; }

        /// <summary>
        /// Gets view model.
        /// </summary>
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        /// <summary>
        /// Gets a collection of functions that handle IPC responses.
        /// </summary>
        public List<System.Action<JsonObject>> IPCResponseHandleList { get; } = new List<System.Action<JsonObject>>();

        public static bool IsElevated { get; set; }

        public static bool IsUserAnAdmin { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellPage"/> class.
        /// Shell page constructor.
        /// </summary>
        public ShellPage()
        {
            InitializeComponent();

            DataContext = ViewModel;
            ShellHandler = this;
            ViewModel.Initialize(shellFrame, navigationView, KeyboardAccelerators);
            shellFrame.Navigate(typeof(GeneralPage));
        }

        public static int SendDefaultIPCMessage(string msg)
        {
            DefaultSndMSGCallback?.Invoke(msg);
            return 0;
        }

        public static int SendCheckForUpdatesIPCMessage(string msg)
        {
            CheckForUpdatesMsgCallback?.Invoke(msg);

            return 0;
        }

        public static int SendRestartAdminIPCMessage(string msg)
        {
            SndRestartAsAdminMsgCallback?.Invoke(msg);
            return 0;
        }

        /// <summary>
        /// Set Default IPC Message callback function.
        /// </summary>
        /// <param name="implementation">delegate function implementation.</param>
        public static void SetDefaultSndMessageCallback(IPCMessageCallback implementation)
        {
            DefaultSndMSGCallback = implementation;
        }

        /// <summary>
        /// Set restart as admin IPC callback function.
        /// </summary>
        /// <param name="implementation">delegate function implementation.</param>
        public static void SetRestartAdminSndMessageCallback(IPCMessageCallback implementation)
        {
            SndRestartAsAdminMsgCallback = implementation;
        }

        /// <summary>
        /// Set check for updates IPC callback function.
        /// </summary>
        /// <param name="implementation">delegate function implementation.</param>
        public static void SetCheckForUpdatesMessageCallback(IPCMessageCallback implementation)
        {
            CheckForUpdatesMsgCallback = implementation;
        }

        /// <summary>
        /// Set oobe opening callback function
        /// </summary>
        /// <param name="implementation">delegate function implementation.</param>
        public static void SetOpenOobeCallback(OobeOpeningCallback implementation)
        {
            OpenOobeWindowCallback = implementation;
        }

        public static void SetElevationStatus(bool isElevated)
        {
            IsElevated = isElevated;
        }

        public static void SetIsUserAnAdmin(bool isAdmin)
        {
            IsUserAnAdmin = isAdmin;
        }

        public static void Navigate(Type type)
        {
            NavigationService.Navigate(type);
        }

        public void Refresh()
        {
            shellFrame.Navigate(typeof(GeneralPage));
        }

        private void OobeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenOobeWindowCallback();
        }

        private bool navigationViewInitialStateProcessed; // avoid announcing initial state of the navigation pane.

        [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Params are required for event handler signature requirements.")]
#pragma warning disable CA1822 // Mark members as static
        private void NavigationView_PaneOpened(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
        {
            if (!navigationViewInitialStateProcessed)
            {
                navigationViewInitialStateProcessed = true;
                return;
            }

            var peer = FrameworkElementAutomationPeer.FromElement(sender);
            if (peer == null)
            {
                peer = FrameworkElementAutomationPeer.CreatePeerForElement(sender);
            }

            if (AutomationPeer.ListenerExists(AutomationEvents.MenuOpened))
            {
                var loader = ResourceLoader.GetForViewIndependentUse();
                peer.RaiseNotificationEvent(
                    AutomationNotificationKind.ActionCompleted,
                    AutomationNotificationProcessing.ImportantMostRecent,
                    loader.GetString("Shell_NavigationMenu_Announce_Open"),
                    "navigationMenuPaneOpened");
            }
        }

        [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Params are required for event handler signature requirements.")]
        private void NavigationView_PaneClosed(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
        {
            if (!navigationViewInitialStateProcessed)
            {
                navigationViewInitialStateProcessed = true;
                return;
            }

            var peer = FrameworkElementAutomationPeer.FromElement(sender);
            if (peer == null)
            {
                peer = FrameworkElementAutomationPeer.CreatePeerForElement(sender);
            }

            if (AutomationPeer.ListenerExists(AutomationEvents.MenuClosed))
            {
                var loader = ResourceLoader.GetForViewIndependentUse();
                peer.RaiseNotificationEvent(
                    AutomationNotificationKind.ActionCompleted,
                    AutomationNotificationProcessing.ImportantMostRecent,
                    loader.GetString("Shell_NavigationMenu_Announce_Collapse"),
                    "navigationMenuPaneClosed");
            }
        }

        private void OOBEItem_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            OpenOobeWindowCallback();
        }

        private async void FeedbackItem_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://aka.ms/powerToysGiveFeedback"));
        }
    }
}
