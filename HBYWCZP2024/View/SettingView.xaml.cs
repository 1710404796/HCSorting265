using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.ViewModel;
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
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingView : Page
    {

        public SettingViewModel ViewModel
        {
            get;
        }

        public SettingView()
        {
            DataContext = this;
            ViewModel = App.GetService<SettingViewModel>();
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MainStorage.SaveManager.Save();
            SaveButton.Content = "√ 保存成功";
            SaveButton.IsEnabled = false;
            await Task.Delay(1000);
            SaveButton.Content = "保存设定";

            SaveButton.IsEnabled = true;
        }
    }
}
