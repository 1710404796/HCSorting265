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
    /// AutoClassEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AutoClassEditWindow : Window
    {
        public AutoFormGeneratorControl AutoFormGeneratorControl;
        public AutoClassEditWindow(object obj)
        {
            InitializeComponent();
            AutoFormGeneratorControl = FormGenerator.CreateForm(obj, "dani");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyForm.Content = AutoFormGeneratorControl;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = true;
            Close();
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    AutoFormGeneratorControl.Save();
        //}
    }
}
