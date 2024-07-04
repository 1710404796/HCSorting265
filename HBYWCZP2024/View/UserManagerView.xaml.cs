using HBYWCZP2024.Data.Models;
using HBYWCZP2024.View.User;
using HBYWCZP2024.ViewModel;
using LinxUniverse.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HBYWCZP2024.View
{
    /// <summary>
    /// UserManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class UserManagerView : Page
    {
        private readonly UserManager<LinxUser> _userManager;
        public UserManagerViewModel ViewModel { get; }

        public UserManagerView(UserManager<LinxUser> userManager, UserManagerViewModel userManagerViewModel)
        {
            DataContext = this;
            _userManager = userManager;
            ViewModel = userManagerViewModel;

            InitializeComponent();
        }

        private void ddShow_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        private void ddShow_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var newUser = new AddUser();

            if ( newUser.ShowDialog() ?? false)
            {
                LinxUser user = new LinxUser
                {
                    Password = newUser.RFID,
                    UserName = newUser.UserName,
                };
                await _userManager.CreateAsync(user);
                await _userManager.AddToRoleAsync(user, newUser.RoleEnum.ToString());
                ViewModel.Users.Add(new AdvUser { LinxUser = user, Role = newUser.RoleEnum.ToString() });
            }
        }

        private async void DelUser_Click(object sender, RoutedEventArgs e)
        {
            //await _userManager.DeleteAsync(user);

        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {

            ViewModel.Users = new( (await _userManager.GetAllUserAsync()).Select(s => new AdvUser
            {
                LinxUser = s, Role = _userManager.get
            }));
        }
    }
}
