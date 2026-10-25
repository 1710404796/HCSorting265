
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HCSortingCZP.Utils;

namespace HCSortingCZP.View
{
    /// <summary>
    /// LockScreenView.xaml 的交互逻辑
    /// </summary>
    public partial class LockScreenView : UserControl
    {
        private bool _isLoggingIn;

        public LockScreenView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FocusInput();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                Dani.Clear();
                ErrorText.Text = string.Empty;
                FocusInput();
            }
        }

        private async void Dani_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await LoginAsync();
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await LoginAsync();
        }

        private void Dani_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Dani.Password))
            {
                ErrorText.Text = string.Empty;
            }
        }

        private async Task LoginAsync()
        {
            if (_isLoggingIn)
            {
                return;
            }

            var rfid = Dani.Password.Trim();
            if (string.IsNullOrWhiteSpace(rfid))
            {
                ErrorText.Text = "RFID不能为空";
                Dani.Focus();
                return;
            }

            _isLoggingIn = true;
            LoginButton.IsEnabled = false;
            ErrorText.Text = string.Empty;

            try
            {
                if (await LoginHelper.Login(rfid))
                {
                    Dani.Clear();
                }
                else
                {
                    ErrorText.Text = "没有找到指定ID的用户";
                    Dani.Clear();
                    Dani.Focus();
                }
            }
            catch (Exception exception)
            {
                ErrorText.Text = $"处理请求时发生错误：{exception.Message}";
                Dani.Clear();
                Dani.Focus();
            }
            finally
            {
                _isLoggingIn = false;
                LoginButton.IsEnabled = true;
            }
        }

        private void FocusInput()
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                new Action(() =>
                {
                    Dani.Focus();
                    Keyboard.Focus(Dani);
                }));
        }
    }
}
