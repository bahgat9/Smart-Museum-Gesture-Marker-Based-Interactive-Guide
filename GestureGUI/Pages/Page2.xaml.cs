using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GestureGUI.Pages
{
    public partial class Page2 : UserControl
    {
        
        private int _currentPage = 0;
        private const int UsersPerPage = 4;

        public Page2()
        {
            InitializeComponent();
            RefreshUsersDisplay();
            RestoreSelectedUser();
        }

        public void UpdateBluetoothUser(string userName, string userType, string deviceName)
        {
            AppState.AddBluetoothUser(userName, userType, deviceName);
            RefreshUsersDisplay();
        }

        public void ShowAction(string action)
        {
            ContentText.Text = "Action: " + action;
        }

        private void RefreshUsersDisplay()
        {
            UsersPanel.Children.Clear();

            var pageUsers = AppState.BluetoothUsers
                .Skip(_currentPage * UsersPerPage)
                .Take(UsersPerPage)
                .ToList();

            foreach (var user in pageUsers)
            {
                UsersPanel.Children.Add(CreateUserCard(user));
            }

            PrevButton.IsEnabled = _currentPage > 0;
            NextButton.IsEnabled = (_currentPage + 1) * UsersPerPage < AppState.BluetoothUsers.Count;
        }
        private void RestoreSelectedUser()
        {
            if (!string.IsNullOrWhiteSpace(AppState.SelectedUserName))
            {
                SelectedUserText.Text = $"Name: {AppState.SelectedUserName}";
                SelectedTypeText.Text = $"Type: {AppState.SelectedUserType}";
                ContentText.Text = GetContentForUserType(AppState.SelectedUserType);
            }
        }
        private Border CreateUserCard(BluetoothUser user)
        {
            Border border = new Border
            {
                Width = 220,
                Margin = new Thickness(10),
                Padding = new Thickness(15),
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(2)
            };

            StackPanel panel = new StackPanel();

            TextBlock nameText = new TextBlock
            {
                Text = user.UserName,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };

            TextBlock typeText = new TextBlock
            {
                Text = "Type: " + user.UserType,
                FontSize = 15,
                Foreground = Brushes.LightSkyBlue,
                Margin = new Thickness(0, 8, 0, 0)
            };

            TextBlock deviceText = new TextBlock
            {
                Text = "Device: " + user.DeviceName,
                FontSize = 14,
                Foreground = Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 0, 0)
            };

            Button selectButton = new Button
            {
                Content = "Select",
                Margin = new Thickness(0, 12, 0, 0),
                Padding = new Thickness(10, 6, 10, 6),
                Background = Brushes.DodgerBlue,
                Foreground = Brushes.White
            };

            selectButton.Click += (s, e) =>
            {
                SelectUser(user, border);
            };

            panel.Children.Add(nameText);
            panel.Children.Add(typeText);
            panel.Children.Add(deviceText);
            panel.Children.Add(selectButton);

            border.Child = panel;
            return border;
        }

        private void SelectUser(BluetoothUser user, Border selectedBorder)
        {
            foreach (var child in UsersPanel.Children)
            {
                if (child is Border border)
                    border.BorderBrush = Brushes.Transparent;
            }

            selectedBorder.BorderBrush = Brushes.Yellow;

            SelectedUserText.Text = $"Name: {user.UserName}";
            SelectedTypeText.Text = $"Type: {user.UserType}";
            ContentText.Text = GetContentForUserType(user.UserType);

            AppState.SelectedUserName = user.UserName;
            AppState.SelectedUserType = user.UserType;
            AppState.SelectedDeviceName = user.DeviceName;
        }

        private string GetContentForUserType(string userType)
        {
            switch (userType)
            {
                case "VIP":
                    return "Welcome VIP visitor. Premium museum content and exclusive artifact details are available for you.";
                case "Regular":
                    return "Welcome. You can explore the standard museum content and interact with the artifacts.";
                case "Premium":
                    return "Welcome Premium user. You have access to extra guided explanations and enhanced content.";
                case "Staff":
                    return "Welcome Staff member. Staff tools and internal content are available.";
                case "Guest":
                    return "Welcome Guest. General museum content is available for you.";
                default:
                    return "Welcome. Museum content is available for you.";
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                RefreshUsersDisplay();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if ((_currentPage + 1) * UsersPerPage < AppState.BluetoothUsers.Count)
            {
                _currentPage++;
                RefreshUsersDisplay();
            }
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppState.ClearBluetoothUsers();
                _currentPage = 0;
                RefreshUsersDisplay();

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = @"C:\Users\Bahgat\AppData\Local\Programs\Python\Python311\python.exe",
                    Arguments = "\"e:/MSA/MSA 4/SmartMuseum/Menna/bluetooth_user.py\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error running Bluetooth scan: " + ex.Message);
            }
        }
    }
}