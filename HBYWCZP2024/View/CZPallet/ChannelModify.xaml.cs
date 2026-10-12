using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Models;
using MediatR;
using System.Windows;


namespace HBYWCZP2024.View.CZPallet
{
    /// <summary>
    /// ChannelModify.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelModify : Window
    {
        private readonly int id;

        public readonly IMediator mediator;
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
            mediator=App.GetService<IMediator>();
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
            string OldBarCode = StageChannel.Code;
            if (string.IsNullOrEmpty(OldBarCode))
            {
                OldBarCode ="（原电芯条码为空）";
            }

            // 强制修改条码限制取消
            if (await DiaoDuStage.UpdateBattery(id, CodeBox.Text, true))
            {
                RuntimeStateStore.Save();
                MessageBox.Show("修改成功!");
                await mediator.Send(new OperatorLogCommand($"原电芯条码{OldBarCode}被修改为{CodeBox.Text}")); 
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
