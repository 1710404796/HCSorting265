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

namespace HCSortingCZP.View
{
    /// <summary>
    /// RFIDInput.xaml 的交互逻辑
    /// </summary>
    public partial class RFIDInput : Window
    {
        public RFIDInput()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 输入的id
        /// </summary>
        public string RFID { get; internal set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dani.Focus();

        }
        private void Dani_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                DialogResult = true;
                RFID = Dani.Password.Trim();
                Close();
            }
        }

        private void giveup_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("真的要放弃吗?", "放弃确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                Dani.Focus();

                return;
            }
            DialogResult = false;
            Close();
        }
    }
}
