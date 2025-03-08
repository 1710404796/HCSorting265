using HBYWCZP2024.Data.Models;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.View.User;
using HBYWCZP2024.ViewModel;
using LinxUniverse.Auth;
using MediatR;
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

        public readonly IMediator mediator;
        public UserManagerView(UserManager<LinxUser> userManager, UserManagerViewModel userManagerViewModel)
        {
            DataContext = this;
            _userManager = userManager;
            ViewModel = userManagerViewModel;
            mediator = App.GetService<IMediator>();
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
                await mediator.Send(new OperatorLogCommand($"添加用户{user.UserName}"));
            }
        }

        private async void DelUser_Click(object sender, RoutedEventArgs e)
        {
            var advuser = ddShow.SelectedItem as AdvUser;
            if (advuser != null )
            {

                await _userManager.DeleteAsync(advuser.LinxUser);
                ViewModel.Users.Remove(advuser);
            }
                await mediator.Send(new OperatorLogCommand($"删除用户{advuser.LinxUser.UserName}"));
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {

            IEnumerable<LinxUser> enumerable = (await _userManager.GetAllUserAsync());
            List<AdvUser> advUsers = [];
            foreach (LinxUser user in enumerable) {
                advUsers.Add(new AdvUser
                {
                    LinxUser = user,
                    Role = (await _userManager.GetUserRolesAsync(user)).FirstOrDefault() ?? string.Empty
                });
            };

            ViewModel.Users = new(advUsers);
        }

        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            //var advuser = ddShow.SelectedItem as AdvUser;
            //if (advuser != null)
            //{

            //    var newUser = new AddUser() { RFID = advuser.LinxUser.Password, RoleEnum }

            //if (newUser.ShowDialog() ?? false)
            //    {
            //        LinxUser user = new LinxUser
            //        {
            //            Password = newUser.RFID,
            //            UserName = newUser.UserName,
            //        };
                   
            //    } 
            //}
        }
    }
}
