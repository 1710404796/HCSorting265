using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.ViewModel
{
    public partial class SettingViewModel(MainViewModel mainViewModel) : ObservableRecipient
    {
        public WcsSaves Saves => MainStorage.Saves;

        public List<string> Stations => MainStorage.Saves.KeysSave.Values.Select(s => s.Name).ToList();
        [ObservableProperty]
        bool _canEditing = true;
        public bool IsBackGround
        {
            get => Saves.BackGroundEnable == System.Windows.Visibility.Visible;
            set
            {
                Saves.BackGroundEnable = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                mainViewModel.HasBackGround = Saves.BackGroundEnable;
            }
        }

    }
}
