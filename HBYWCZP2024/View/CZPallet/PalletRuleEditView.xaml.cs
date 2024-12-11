using HBYWCZP2024.Models.CZPallet;
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
using System.Windows.Shapes;

namespace HBYWCZP2024.View.CZPallet
{
    /// <summary>
    /// PalletRuleEditView.xaml 的交互逻辑
    /// </summary>
    public partial class PalletRuleEditView : Window
    {

        //public PalletRuleEditViewModel ViewModel { get; }
        public System.Collections.Generic.HashSet<PalletRuleControl> PalletRuleControls { get; set; } = new();
        public PalletRuleEditView()
        {
            InitializeComponent();
        }

        private void AddPallet_Click(object sender, RoutedEventArgs e)
        {
            CreatePalletRule createPalletRule = new();
            var aa = createPalletRule.ShowDialog();
            if (aa ?? false)
            {
                PalletTypeRule item = new PalletTypeRule(createPalletRule.Name, createPalletRule.RegexStr, createPalletRule.FakeStr, createPalletRule.ChannelNum);
                MainStorage.Saves.PalletTypeRules.Add(item)
                ;
                PalletList.Children.Add(CreatePanel(item));
                // 在界面中添加一波

            }
        }
        /// <summary>
        /// 生成设置表
        /// </summary>
        /// <param name="palletTypeRule"></param>
        /// <returns></returns>
        public PalletRuleControl CreatePanel(PalletTypeRule palletTypeRule)
        {
            // Todo: 考虑简化vm

            PalletRuleControl palletRuleControl = new(palletTypeRule);
            palletRuleControl.OnDelete += DeleteOne;
            PalletRuleControls.Add(palletRuleControl);
            return palletRuleControl;
        }

        private void DeleteOne(PalletRuleControl arg2)
        {
            PalletList.Children.Remove(arg2);
            PalletRuleControls.Remove(arg2);
            MainStorage.Saves.PalletTypeRules.Remove(arg2.PalletTypeRule);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        void Init()
        {

            foreach (var item in MainStorage.Saves.PalletTypeRules)
            {
                PalletList.Children.Add(CreatePanel(item));

            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in PalletRuleControls)
            {
                item.OnDelete -= DeleteOne;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainStorage.SaveManager.Save();
            Close();
        }
    }
}
