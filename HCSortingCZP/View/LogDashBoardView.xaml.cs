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

namespace HCSortingCZP.View
{
    /// <summary>
    /// LogDashBoardView.xaml 的交互逻辑
    /// </summary>
    public partial class LogDashBoardView : Page, IDisposable
    {
        public LogDashBoardView()
        {
            InitializeComponent();
        }

        private RichTextBox? element;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            element = App.GetService<RichTextBox>();
            if (element.Parent is Panel oldPanel)
            {
                oldPanel.Children.Remove(element);
            }
            else if (element.Parent is Decorator oldDecorator)
            {
                oldDecorator.Child = null;
            }
            else if (element.Parent is ContentControl oldContentControl &&
                     ReferenceEquals(oldContentControl.Content, element))
            {
                oldContentControl.Content = null;
            }

            DashBoard.Children.Clear();
            DashBoard.Children.Add(element);
            element.ScrollToEnd();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DetachLog();
        }

        public void Dispose()
        {
            DetachLog();
        }

        private void DetachLog()
        {
            if (element == null)
            {
                return;
            }

            DashBoard.Children.Remove(element);
            element = null;
        }

    }
}
