using HBYWCZP2024.Models;
using System.Windows;


namespace HBYWCZP2024.View.CZPallet
{
    /// <summary>
    /// ChannelModify.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelModify : Window
    {
        private readonly int id;

        public string Title { get; set; } = "修改通道信息";

        /// <summary>
        /// 用于修改code
        /// </summary>
        public ChannelModify(XYLStation diaoDuStage, int id)
        {
            DataContext = this;
            DiaoDuStage = diaoDuStage;
            this.id = id;
            StageChannel = diaoDuStage.Channels[id];

            InitializeComponent();
        }
        public int Tid => id + 1;
        public StationChannel StageChannel { get; }
        public XYLStation DiaoDuStage { get; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Ti.Text = $"调度号：{DiaoDuStage.InsideNum} {DiaoDuStage.Name} 通道：{Tid}";

            //DiaoDuStage.OnNG += ShowMessage;
            //DiaoDuStage.OnWarnning += ShowMessage;
        }
        /// <summary>
        /// 保存结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // 强制修改条码限制取消
            if (await DiaoDuStage.UpdateBattery(id, CodeBox.Text, true))
            {
                MessageBox.Show("修改成功!");
                Close();
            }
            else
            {
                MessageBox.Show("修改失败!");

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

            //DiaoDuStage.OnNG -= ShowMessage;
            //DiaoDuStage.OnWarnning -= ShowMessage;

        }

        void ShowMessage(object sender, string message)
        {
            MessageBox.Show(message);
        }
    }
}
