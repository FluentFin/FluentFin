using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentFin
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Trigger your login function
                OnLoginClick(sender, e);  // or call your login logic directly
            }
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            StatusContent.Content = new ProgressRing { Width = 40, Height = 40, IsActive = true };
            string serverUrl = HostTextBox.Text;
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Password;

            Debug.WriteLine("We are inside OnLoginClick");
            Debug.WriteLine($"Host:{serverUrl}");
            Debug.WriteLine($"{username}");
            Debug.WriteLine($"{password}");

            JellyfinClient client = new JellyfinClient("","");

            if (!string.IsNullOrEmpty(serverUrl) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                if (await client.IsServerAvailable(serverUrl))
                {
                    var token = await client.AuthenticateUserAsync(serverUrl, username, password);

                    if (token != null)
                    {
                        StatusContent.Content = null;
                        Frame.Navigate(typeof(HomePage));
                    }
                    else
                    {
                        StatusContent.Content = new InfoBar
                        {
                            Title = "Login Failed",
                            Message = "Your username or password is incorrect",
                            Severity = InfoBarSeverity.Error, // Makes it red
                            IsOpen = true
                        };
                    }
                }
                else
                {
                    StatusContent.Content = new InfoBar
                    {
                        Title = "Server Not Found",
                        Message = "Could not connect to the server. Please check the URL.",
                        Severity = InfoBarSeverity.Error,
                        IsOpen = true
                    };
                }
            }
            else
            {   
                StatusContent.Content = new InfoBar
                {
                    Title = "Enter all fields",
                    Message = "One or more fields are empty",
                    Severity = InfoBarSeverity.Error, // Makes it red
                    IsOpen = true
                };
            }
        }
    }
}
