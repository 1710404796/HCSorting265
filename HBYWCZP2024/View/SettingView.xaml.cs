using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.View.CZPallet;
using HBYWCZP2024.View.User;
using HBYWCZP2024.ViewModel;
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
            //StationNameBox.Text = MainStorage.Saves.SelectStageName;
            SelectBox.SelectedValue = MainStorage.Saves.SelectStageName;

            ApiEnableStack.Visibility = App.GetService<IMediator>().Send(new CheckPowerQuery(Attritubes.PowerEnum.接口使能配置)).Result ? Visibility.Visible : Visibility.Collapsed;
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

        private void SetPlc_Click(object sender, RoutedEventArgs e)
        {
            new PLCListWindow().ShowDialog();
        }

        private async void SetPower_Click(object sender, RoutedEventArgs e)
        {
            var mediator=App.GetService<IMediator>();
            var checkPowerQuery=new CheckPowerQuery(PowerEnum.权限配置);
            bool result=await mediator.Send(checkPowerQuery);
            if (result)
            {
                App.GetService<PowerSettingWindows>().ShowDialog();
            }
            else
            {
                MessageBox.Show("当前没有此权限");   
            }
        }

        private void StationSetting_Click(object sender, RoutedEventArgs e)
        {
            App.GetService<StationSettingView>().ShowDialog();

        }

        private void RuleSetting_Click(object sender, RoutedEventArgs e)
        {
            App.GetService<PalletRuleEditView>().ShowDialog();

        }

        private void StationNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (sender as TextBox).Text;
            if (text == MainStorage.Saves.SelectStageName) return;
            if (string.IsNullOrWhiteSpace(text)) return;
            if (MainStorage.Saves.KeysSave.ContainsKey(text))
            {
                MessageBox.Show("已经有相同名称的工位，不可修改");
                (sender as TextBox).Text = MainStorage.Saves.SelectStageName;
                return;
            }
            MainStorage.Saves.KeysSave[text] = MainStorage.Saves.KeysSave[MainStorage.Saves.SelectStageName];
            MainStorage.Saves.KeysSave.Remove(MainStorage.Saves.SelectStageName);


            MainStorage.Saves.SelectStageName = text;

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((string)SelectBox.SelectedValue == MainStorage.Saves.SelectStageName) return;
            MainStorage.Saves.SelectStageName = (string)SelectBox.SelectedValue;
            MainStorage.SaveManager.Save();
            MainWindow.NageTo<SettingView>();
        }

        private void ResetTable_Click(object sender, RoutedEventArgs e)
        {
            MainStorage.Saves.ResetTable = true;
        }
    }
}
