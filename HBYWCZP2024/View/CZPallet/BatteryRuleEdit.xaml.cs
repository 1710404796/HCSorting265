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
    /// BatteryRuleEdit.xaml 的交互逻辑
    /// </summary>
    public partial class BatteryRuleEdit : Window
    {
        private HashSet<BatteryRuleControl> BatteryRuleControls = new();

        //public BatteryRuleEditViewModel ViewModel { get; set; }
        public BatteryRuleEdit(PalletTypeRule palletTypeRule)
        {
            DataContext = this;
            //ViewModel= new BatteryRuleEditViewModel() { PalletTypeRule = palletTypeRule };
            InitializeComponent();
            PalletTypeRule = palletTypeRule;
        }

        public PalletTypeRule PalletTypeRule { get; }

        private void AddPallet_Click(object sender, RoutedEventArgs e)
        {
            CreateBatteryRule createPalletRule = new();
            var aa = createPalletRule.ShowDialog();
            if (aa ?? false)
            {
                BatteryTypeRule item = new(createPalletRule.Name, createPalletRule.RegexStr);
                PalletTypeRule.AddRule(item);
                BatteryList.Children.Add(CreatePanel(item));
                // 在界面中添加一波

            }
        }

        private BatteryRuleControl CreatePanel(BatteryTypeRule item)
        {
            var aa = new BatteryRuleControl(item);
            aa.OnDelete += DeleteOne;
            return aa;
        }
        private void DeleteOne(BatteryRuleControl arg2)
        {
            BatteryList.Children.Remove(arg2);
            BatteryRuleControls.Remove(arg2);
            PalletTypeRule.Rules.Remove(arg2.Item);

        }

        void Init()
        {

            foreach (var item in PalletTypeRule.Rules)
            {
                BatteryList.Children.Add(CreatePanel(item));

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in BatteryRuleControls)
            {
                item.OnDelete -= DeleteOne;
            }

        }
    }
}
