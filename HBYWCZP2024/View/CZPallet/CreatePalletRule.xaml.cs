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
    /// CreatePalletRule.xaml 的交互逻辑
    /// </summary>
    public partial class CreatePalletRule : Window
    {
        public string Name { get; set; } = string.Empty;
        public string RegexStr { get; set; } = string.Empty;
        public string FakeStr { get; set; } = string.Empty;
        public int ChannelNum { get; set; } = 24;
        public CreatePalletRule()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Name = RuleName.Text;
            RegexStr = RegexString.Text;
            //FakeStr = FakeString.Text;
            int v;
            int.TryParse(Channels.Text, out v);
            ChannelNum = v;
            DialogResult = true;
            Close();
        }
    }
}
