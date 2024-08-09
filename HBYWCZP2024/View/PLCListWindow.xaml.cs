using HBYWCZP2024.Models;
using LinxUniverse.PLC;
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
    /// PLCListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PLCListWindow : Window
    {
        //List<PLCSetting> settingList = MainStorage.Saves.PLCPSetting.ToList();

        List<TextBox[]> _textBoxes = new List<TextBox[]>();
        public PLCListWindow()
        {
            InitializeComponent();
        }
        List<StageMono> StationNames = [];
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            StationNames = MainStorage.Saves.KeysSave.Values.ToList();
            foreach (var item in StationNames)
            {
                AddPlc(item);
            }
        }

        void AddPlc(StageMono stage)
        {
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            TextBox[] textBoxes = [new(), new()];

            textBoxes[0].Width = 150;
            textBoxes[0].Text = stage.Name;
            //textBoxes[1].Width = 150;
            //textBoxes[1].Text = pLCSetting.IP;

            _textBoxes.Add(textBoxes);

            stackPanel.Children.Add(textBoxes[0]);
            //stackPanel.Children.Add(textBoxes[1]);

            Button delete = new Button { Content = "X" };

            stackPanel.Children.Add(delete);

            plcsettingPanel.Children.Add(stackPanel);

            delete.Click += (s, e) => {
                _textBoxes.Remove(textBoxes);
                plcsettingPanel.Children.Remove(stackPanel);
                //settingList.Remove(pLCSetting);

            };


        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {

            
            MainStorage.Saves.KeysSave.Clear();

            for (int i = 0; i < _textBoxes.Count; i++)
            {
                StationNames[i].Name = _textBoxes[i][0].Text;
                MainStorage.Saves.KeysSave[StationNames[i].Name] = StationNames[i];
                //settingList[i].IP = _textBoxes[i][1].Text;
            }
            if (!MainStorage.Saves.KeysSave.ContainsKey(MainStorage.Saves.SelectStageName))
            {
                MainStorage.Saves.SelectStageName = MainStorage.Saves.KeysSave.Keys.First();
            }

            MainStorage.SaveManager.Save();
            Close();
            MessageBox.Show("新增/删除工位后, 请重启软件", "重启提示");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StageMono pLCSetting = new() { Name = "NewStage" + StationNames.Count };
            StationNames.Add(pLCSetting);
            AddPlc(pLCSetting);
        }
    }
}
