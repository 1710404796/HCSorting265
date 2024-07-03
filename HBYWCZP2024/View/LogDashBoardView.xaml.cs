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
    /// LogDashBoardView.xaml 的交互逻辑
    /// </summary>
    public partial class LogDashBoardView : Page
    {
        public LogDashBoardView()
        {
            InitializeComponent();
        }
        RichTextBox element;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            element = App.GetService<RichTextBox>();
            DashBoard.Children.Add(element);
            element.ScrollToEnd();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DashBoard.Children.Remove(element);

        }

    }
}
