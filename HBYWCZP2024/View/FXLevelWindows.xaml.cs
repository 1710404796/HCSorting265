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

namespace HBYWCZP2024.View
{
    /// <summary>
    /// FXLevelWindows.xaml 的交互逻辑
    /// </summary>
    public partial class FXLevelWindows : Window
    {
        public FXLevelWindows(List<string[]> datas)
        {
            Datas = datas.ToList();
            while (Datas.Count < 7)
            {
                Datas.Add(new string[0]);
            }
            InitializeComponent();
        }

        public List<string[]> Datas { get; }
        TextBox[] textBoxes = new TextBox[7];
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 7; i++)
            {
                var stack = new StackPanel() { HorizontalAlignment= HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                stack.Orientation = Orientation.Horizontal;
                var textblock = new TextBlock() { Width = 100, VerticalAlignment = VerticalAlignment.Center };
                textblock.Text = $"电池{i + 11}等级";
                if (i == 6) textblock.Text = $"电池NG等级";

                var textBox = new TextBox() { Width = 200 };
                textBox.Text = string.Join(",", Datas[i]);

                stack.Children.Add(textblock);
                stack.Children.Add(textBox);
                textBoxes[i] = textBox;
                fxStack.Children.Add(stack);

            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < 7; i++)
            {
                var textBox = textBoxes[i];
                var text = textBox.Text;
                if (string.IsNullOrEmpty(text))
                {
                    Datas[i] = new string[0];
                }
                else
                {
                    Datas[i] = text.Split([',', '，']).Select(x => x.Trim()).ToArray();
                }
            }
            DialogResult = true;
            Close();

        }
    }
}
