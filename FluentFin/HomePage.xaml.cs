using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI.ApplicationSettings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentFin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            LoadLibrariesAsync();
        }

        private async void LoadLibrariesAsync()
        {
            try
            {
                JellyfinClient clientKeys = new JellyfinClient("", "");
                string jellyfinUrl = clientKeys.RetrieveString("serverUrl_Key");
                var apiKey = clientKeys.RetrieveToken();

                JellyfinClient client = new JellyfinClient(jellyfinUrl, apiKey);
                List<JellyfinLibrary> libraries = await client.GetLibrariesAsync();

                foreach (var library in libraries)
                {
                    NavigationViewItem item = new NavigationViewItem
                    {
                        Content = library.Name,  // Set display text
                        Tag = library.CollectionType,      // Store identifier
                        Icon = new SymbolIcon(Symbol.Play) // Optional icon
                    };

                    NavView.MenuItems.Add(item);
                    //Debug.WriteLine($"Library: {library.Name} (ID: {library.Id}, Type: {library.CollectionType})");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading libraries: {ex.Message}");
            }
        }

        private async void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer == LogoutButton)
            {
                ContentDialog logoutDialog = new ContentDialog
                {
                    Title = "Sign out",
                    Content = "Are you sure you want to sign out?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    XamlRoot = this.Content.XamlRoot // Required for WinUI 3
                };

                ContentDialogResult result = await logoutDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // Perform the logout action (example: close the app)
                    var vault = new PasswordVault();
                    var credentials = vault.RetrieveAll();

                    foreach (var cred in credentials)
                    {
                        vault.Remove(cred);
                    }
                    Frame.Navigate(typeof(LoginPage));
                }
            }

            
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                string pageName = selectedItem.Tag.ToString();

                switch (pageName)
                {
                    case "movies":
                        sender.Header = selectedItem.Content;
                        ContentFrame.Navigate(typeof(MoviesPage));
                        break;
                }
            }
        }
    }
}
