
using HBYWCZP2024.Utils;
using LinxUniverse.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HBYWCZP2024.View
{
    /// <summary>
    /// LockScreenView.xaml 的交互逻辑
    /// </summary>
    public partial class LockScreenView : UserControl
    {
        private LinxAuthenticationStateProvider _authenticationStateProvider;

        public LockScreenView()
        {
            _authenticationStateProvider =
                App.GetService<LinxAuthenticationStateProvider>();
            InitializeComponent();
        }

        private async void aa_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await LoginHelper.Login();
                // await userManager.CheckLogin();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            aa.Focus();
            await LoginHelper.Login();

        }

        private async void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // await userManager.CheckLogin();
                await LoginHelper.Login();

            }
        }

        private async void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // await userManager.CheckLogin();

            await LoginHelper.Login();

        }

        private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //try
            //{
            //    //if ((bool)e.NewValue)
            //    //    await LoginHelper.Login();
            //}
            //catch (Exception)
            //{

            //    //throw;
            //}


        }
    }
}
