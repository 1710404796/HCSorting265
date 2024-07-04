using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Data.Models;
using Humanizer;
using LinxUniverse.Auth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.ViewModel
{
    public partial class UserManagerViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private ObservableCollection<AdvUser> _users = new();
    }


    public class AdvUser
    {
        public LinxUser LinxUser { get; set; }
        public string Role { get; set; }
        public string PassStar => LinxUser.Password.Truncate(5, "****",Truncator.FixedLength);

    }
    //public 
}
