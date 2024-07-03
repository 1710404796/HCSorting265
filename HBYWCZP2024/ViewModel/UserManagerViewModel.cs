using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Data.Models;
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
        private ObservableCollection<User> _users = new();
    }
}
