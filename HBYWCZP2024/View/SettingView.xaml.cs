using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.Service;
using HBYWCZP2024.Service.Models;
using HBYWCZP2024.View.CZPallet;
using HBYWCZP2024.View.User;
using HBYWCZP2024.ViewModel;
using MediatR;
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
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    [PowerView(PowerEnum.程序参数设定)]
    public partial class SettingView : Page
    {

        public SettingViewModel ViewModel
        {
            get;
        }

        public SettingView()
        {
            DataContext = this;
            ViewModel = App.GetService<SettingViewModel>();


            InitializeComponent();
            //StationNameBox.Text = MainStorage.Saves.SelectStageName;
            SelectBox.SelectedValue = MainStorage.Saves.SelectStageName;

            ApiEnableStack.Visibility = App.GetService<IMediator>().Send(new CheckPowerQuery(Attritubes.PowerEnum.接口使能配置)).Result ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 保存设定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MainStorage.SaveManager.Save();
            SaveButton.Content = "√ 保存成功";
            SaveButton.IsEnabled = false;
            await Task.Delay(1000);
            SaveButton.Content = "保存设定";

            SaveButton.IsEnabled = true;

        }

        /// <summary>
        /// 配置工位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetPlc_Click(object sender, RoutedEventArgs e)
        {
                new PLCListWindow().ShowDialog();
        }

        /// <summary>
        /// 权限配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SetPower_Click(object sender, RoutedEventArgs e)
        {
            var mediator=App.GetService<IMediator>();
            var checkPowerQuery=new CheckPowerQuery(PowerEnum.权限管理);
            bool result=await mediator.Send(checkPowerQuery);
            if (result)
            {
                App.GetService<PowerSettingWindows>().ShowDialog();
            }
            else
            {
                MessageBox.Show("当前没有此权限");   
            }
        }

        /// <summary>
        /// 调度号工位号表配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StationSetting_Click(object sender, RoutedEventArgs e)
        {
            App.GetService<StationSettingView>().ShowDialog();

        }

        /// <summary>
        /// 条码规则表配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RuleSetting_Click(object sender, RoutedEventArgs e)
        {
            App.GetService<PalletRuleEditView>().ShowDialog();

        }

        //private void StationNameBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var text = (sender as TextBox).Text;
        //    var oldName = MainStorage.Saves.SelectStageName;
        //    if (string.IsNullOrWhiteSpace(oldName)) return;
        //    if (text == oldName) return;
        //    if (string.IsNullOrWhiteSpace(text)) return;
        //    if (MainStorage.Saves.KeysSave.ContainsKey(text))
        //    {
        //        MessageBox.Show("已经有相同名称的工位，不可修改");
        //        (sender as TextBox).Text = oldName;
        //        return;
        //    }
        //    MainStorage.Saves.KeysSave[text] = MainStorage.Saves.KeysSave[oldName];
        //    MainStorage.Saves.KeysSave.Remove(oldName);
        //    MainStorage.Saves.SelectStageName = text;
        //}

        /// <summary>
        /// 工位选择下拉框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 页面释放时 BindingCleaner 会清空 ItemsSource/SelectedValue，
            // 此时 SelectionChanged 也会触发，必须忽略空值，避免 SelectStageName 被写成 null。
            if (SelectBox.SelectedValue is not string name || string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (name == MainStorage.Saves.SelectStageName) return;
            MainStorage.Saves.SelectStageName = name;
            MainStorage.SaveManager.Save();
            MainWindow.NageTo<SettingView>();
        }

        /// <summary>
        /// 重置OPCUA缓存表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ResetTable_Click(object sender, RoutedEventArgs e)
        {
            var mediator = App.GetService<IMediator>();
            var checkPowerQuery = new CheckPowerQuery(PowerEnum.重新匹配PLC节点);
            bool result = await mediator.Send(checkPowerQuery);
            if (result)
            {
                MainStorage.Saves.ResetTable = true;
                MessageBox.Show("重新匹配成功");
            }
            else
            {
                MessageBox.Show("当前没有此权限");
            }
            
        }

        /// <summary>
        /// 分选档位配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FXLevelConfig_Click(object sender, RoutedEventArgs e)
        {
            var fxLevelWindows = new FXLevelWindows(MainStorage.Saves.BatteryLevelList);
            fxLevelWindows.ShowDialog();
            if (fxLevelWindows.DialogResult == true)
            {
                MainStorage.Saves.BatteryLevelList = fxLevelWindows.Datas;
                MainStorage.SaveManager.Save();
            }

        }

        /// <summary>
        /// 工位等级配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StationLevelConfig_Click(object sender, RoutedEventArgs e)
        {
            var win = new View.CZPallet.StationLevelConfigWindow();
            win.ShowDialog();
        }

        /// <summary>
        /// 测试数据拆盘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TestChaiButton_Click(object sender, RoutedEventArgs e)
        {
            var mediator = App.GetService<IMediator>();
            var factory = App.GetService<IHBECSServiceFactory>();
            var hBECSService = factory.Create(MainStorage.Saves.SelectStageName);


            var mesRes = await hBECSService.MesService.UploadPalletResultAsync(new UploadPalletResultRequest(MainStorage.Saves.SelectStageName)
            {
                cellDataVOList = [..
                            new (int Channel, string BatteryCode)[]
                            {
                                (1, "0ZZCE11431B00SG2H0002737"),
                                (2, "0ZZCE11431B00SG2H0002442"),
                                (3, "0ZZCE11431B00SG2H0002503"),
                                (4, "0ZZCE11431B00SG2H0002255"),
                                (5, "0ZZCE11431B00SG2H0002066"),
                                (6, "0ZZCE11431B00SG2H0002210"),
                                (7, "0ZZCE11431B00SG2H0002501"),
                                (8, "0ZZCE11431B00SG2H0002156"),
                                (9, "0ZZCE11431B00SG2H0002337"),
                                (10, "0ZZCE11431B00SG2H0002500"),
                                (11, "0ZZCE11431B00SG2H0002716"),
                                (12, "0ZZCE11431B00SG2H0002502"),
                                (13, "0ZZCE11431B00SG2H0002567"),
                                (14, "0ZZCE11431B00SG2H0002568"),
                                (15, "0ZZCE11431B00SG2H0002125"),
                                (16, "0ZZCE11431B00SG2H0002367"),
                                (17, "0ZZCE11431B00SG2H0002569"),
                                (18, "0ZZCE11431B00SG2H0002444"),
                                (19, "0ZZCE11431B00SG2H0002415"),
                                (20, "0ZZCE11431B00SG2H0002244"),
                                (21, "0ZZCE11431B00SG2H0002593"),
                                (22, "0ZZCE11431B00SG2H0002082"),
                                (23, "0ZZCE11431B00SG2H0002151"),
                                (24, "0ZZCE11431B00SG2H0002314"),
                                (25, "0ZZCE11431B00SG2H0002265"),
                                (26, "0ZZCE11431B00SG2H0002681"),
                                (27, "0ZZCE11431B00SG2H0002437"),
                                (28, "0ZZCE11431B00SG2H0002579"),
                                (29, "0ZZCE11431B00SG2B0000584"),
                                (30, "0ZZCE11431B00SG2H0002438"),
                                (31, "0ZZCE11431B00SG2H0002519"),
                                (32, "0ZZCE11431B00SG2H0002008"),
                                (33, "0ZZCE11431B00SG2J0000823"),
                                (34, "0ZZCE11431B00SG2H0002085"),
                                (35, "0ZZCE11431B00SG2H0002138"),
                                (36, "0ZZCE11431B00SG2H0002220"),
                                (37, "0ZZCE11431B00SG2H0002029"),
                                (38, "0ZZCE11431B00SG2H0002316"),
                                (39, "0ZZCE11431B00SG2H0002057"),
                                (40, "0ZZCE11431B00SG2H0002145"),
                                (41, "0ZZCE11431B00SG2J0001066"),
                                (42, "0ZZCE11431B00SG2H0001580"),
                                (43, "0ZZCE11431B00SG2J0000789"),
                                (44, "0ZZCE11431B00SG2J0000788"),
                                (45, "0ZZCE11431B00SG2J0001057"),
                                (46, "0ZZCE11431B00SG2J0001060"),
                                (47, "0ZZCE11431B00SG2J0001149"),
                                (48, "0ZZCE11431B00SG2J0001064"),
                                (49, "0ZZCE11431B00SG2J0000938"),
                                (50, "0ZZCE11431B00SG2J0001150"),
                                (51, "0ZZCE11431B00SG2J0001003"),
                                (52, "0ZZCE11431B00SG2J0000860"),
                                (53, "0ZZCE11431B00SG2J0001138"),
                                (54, "0ZZCE11431B00SG2J0001139"),
                                (55, "0ZZCE11431B00SG2J0001137"),
                                (56, "0ZZCE11431B00SG2H0002726"),
                                (57, "0ZZCE11431B00SG2H0002423"),
                                (58, "0ZZCE11431B00SG2H0002768"),
                                (59, "0ZZCE11431B00SG2J0001141"),
                                (60, "0ZZCE11431B00SG2J0000790"),
                                (61, "0ZZCE11431B00SG2H0002487"),
                                (62, "0ZZCE11431B00SG2H0002787"),
                                (63, "0ZZCE11431B00SG2H0002300"),
                                (64, "0ZZCE11431B00SG2H0002508"),
                                (65, "0ZZCE11431B00SG2H0002636"),
                                (66, "0ZZCE11431B00SG2H0002507"),
                                (67, "0ZZCE11431B00SG2H0002227"),
                                (68, "0ZZCE11431B00SG2H0002289"),
                                (69, "0ZZCE11431B00SG2H0002228"),
                                (70, "0ZZCE11431B00SG2H0002206"),
                                (71, "0ZZCE11431B00SG2H0002539"),
                                (72, "0ZZCE11431B00SG2H0001918"),
                                (73, "0ZZCE11431B00SG2H0002538"),
                                (74, "0ZZCE11431B00SG2H0001883"),
                                (75, "0ZZCE11431B00SG2H0002630"),
                                (76, "0ZZCE11431B00SG2H0001922"),
                                (77, "0ZZCE11431B00SG2H0002639"),
                                (78, "0ZZCE11431B00SG2H0001946"),
                                (79, "0ZZCE11431B00SG2H0001921"),
                                (80, "0ZZCE11431B00SG2H0002673"),
                                (81, "0ZZCE11431B00SG2H0002866"),
                                (82, "0ZZCE11431B00SG2J0000826"),
                                (83, "0ZZCE11431B00SG2H0002871"),
                                (84, "0ZZCE11431B00SG2J0000670"),
                                (85, "0ZZCE11431B00SG2H0003248"),
                                (86, "0ZZCE11431B00SG2H0002933"),
                                (87, "0ZZCE11431B00SG2H0002646"),
                                (88, "0ZZCE11431B00SG2G0000408"),
                                (89, "0ZZCE11431B00SG2H0002976"),
                                (90, "0ZZCE11431B00SG2H0001884"),
                                (91, "0ZZCE11431B00SG2H0002857"),
                                (92, "0ZZCE11431B00SG2H0003033"),
                                (93, "0ZZCE11431B00SG2H0003354"),
                                (94, "0ZZCE11431B00SG2H0003249"),
                                (95, "0ZZCE11431B00SG2H0002937"),
                                (96, "0ZZCE11431B00SG2H0002861"),
                                (97, "0ZZCE11431B00SG2H0003034"),
                                (98, "0ZZCE11431B00SG2H0003309"),
                                (99, "0ZZCE11431B00SG2H0002039"),
                                (100, "0ZZCE11431B00SG2H0002018"),
                                (101, "0ZZCE11431B00SG2H0002212"),
                                (102, "0ZZCE11431B00SG2H0002050"),
                                (103, "0ZZCE11431B00SG2H0002027"),
                                (104, "0ZZCE11431B00SG2H0002040"),
                                (105, "0ZZCE11431B00SG2B0000585"),
                                (106, "0ZZCE11431B00SG2H0002086"),
                                (107, "0ZZCE11431B00SG2H0002065"),
                                (108, "0ZZCE11431B00SG2B0000445"),
                                (109, "0ZZCE11431B00SG2H0002299"),
                                (110, "0ZZCE11431B00SG2H0002121"),
                                (111, "0ZZCE11431B00SG2H0002200"),
                                (112, "0ZZCE11431B00SG2H0001896"),
                                (113, "0ZZCE11431B00SG2H0002504"),
                                (114, "0ZZCE11431B00SG2H0002436"),
                                (115, "0ZZCE11431B00SG2H0002079"),
                                (116, "0ZZCE11431B00SG2H0002614"),
                                (117, "0ZZCE11431B00SG2H0002518"),
                                (118, "0ZZCE11431B00SG2H0003126"),
                                (119, "0ZZCE11431B00SG2H0002245"),
                                (120, "0ZZCE11431B00SG2H0002058"),
                                (121, "0ZZCE11431B00SG2H0002033"),
                                (122, "0ZZCE11431B00SG2H0002038"),
                                (123, "0ZZCE11431B00SG2H0002035"),
                                (124, "0ZZCE11431B00SG2H0002214"),
                                (125, "0ZZCE11431B00SG2H0002309"),
                                (126, "0ZZCE11431B00SG2H0001966"),
                                (127, "0ZZCE11431B00SG2H0002031"),
                                (128, "0ZZCE11431B00SG2H0002007"),
                                (129, "0ZZCE11431B00SG2H0002135"),
                                (130, "0ZZCE11431B00SG2H0002295"),
                                (131, "0ZZCE11431B00SG2H0002213"),
                                (132, "0ZZCE11431B00SG2H0002150"),
                                (133, "0ZZCE11431B00SG2H0002139"),
                                (134, "0ZZCE11431B00SG2H0002218"),
                                (135, "0ZZCE11431B00SG2H0002141"),
                                (136, "0ZZCE11431B00SG2H0002161"),
                                (137, "0ZZCE11431B00SG2H0002081"),
                                (138, "0ZZCE11431B00SG2H0002155"),
                                (139, "0ZZCE11431B00SG2H0002047"),
                                (140, "0ZZCE11431B00SG2H0002153"),
                                (141, "0ZZCE11431B00SG2H0002308"),
                                (142, "0ZZCE11431B00SG2H0002067"),
                                (143, "0ZZCE11431B00SG2J0001056"),
                                (144, "0ZZCE11431B00SG2J0000861"),
                                (145, "0ZZCE11431B00SG2J0001144"),
                                (146, "0ZZCE11431B00SG2J0000794"),
                                (147, "0ZZCE11431B00SG2J0001136"),
                                (148, "0ZZCE11431B00SG2J0001063"),
                                (149, "0ZZCE11431B00SG2J0000973"),
                                (150, "0ZZCE11431B00SG2J0000793"),
                                (151, "0ZZCE11431B00SG2J0000977"),
                                (152, "0ZZCE11431B00SG2J0001059"),
                                (153, "0ZZCE11431B00SG2J0001148"),
                                (154, "0ZZCE11431B00SG2J0000939"),
                                (155, "0ZZCE11431B00SG2H0002728"),
                                (156, "0ZZCE11431B00SG2H0002264"),
                                (157, "0ZZCE11431B00SG2H0002751"),
                                (158, "0ZZCE11431B00SG2H0002626"),
                                (159, "0ZZCE11431B00SG2H0002534"),
                                (160, "0ZZCE11431B00SG2H0002217"),
                                (161, "0ZZCE11431B00SG2J0000937"),
                                (162, "0ZZCE11431B00SG2H0002019"),
                                (163, "0ZZCE11431B00SG2H0002717"),
                                (164, "0ZZCE11431B00SG2J0001145"),
                                (165, "0ZZCE11431B00SG2H0002433"),
                                (166, "0ZZCE11431B00SG2H0002780"),
                                (167, "0ZZCE11431B00SG2H0002164"),
                                (168, "0ZZCE11431B00SG2H0002201"),
                                (169, "0ZZCE11431B00SG2H0002335"),
                                (170, "0ZZCE11431B00SG2H0002278"),
                                (171, "0ZZCE11431B00SG2H0002483"),
                                (172, "0ZZCE11431B00SG2H0002120"),
                                (173, "0ZZCE11431B00SG2H0001938"),
                                (174, "0ZZCE11431B00SG2H0002788"),
                                (175, "0ZZCE11431B00SG2H0002558"),
                                (176, "0ZZCE11431B00SG2H0002513"),
                                (177, "0ZZCE11431B00SG2H0002881"),
                                (178, "0ZZCE11431B00SG2H0002873"),
                                (179, "0ZZCE11431B00SG2H0002642"),
                                (180, "0ZZCE11431B00SG2H0002641"),
                                (181, "0ZZCE11431B00SG2H0002697"),
                                (182, "0ZZCE11431B00SG2H0002635"),
                                (183, "0ZZCE11431B00SG2H0002686"),
                                (184, "0ZZCE11431B00SG2J0000821"),
                                (185, "0ZZCE11431B00SG2H0002790"),
                                (186, "0ZZCE11431A00SG270000138"),
                                (187, "0ZZCE11431B00SG290002951"),
                                (188, "0ZZCE11431B00SG2H0001923"),
                                (189, "0ZZCE11431B00SG2H0001929"),
                                (190, "0ZZCE11431B00SG2H0001913"),
                                (191, "0ZZCE11431B00SG2H0002714"),
                                (192, "0ZZCE11431B00SG2H0002644"),
                                (193, "0ZZCE11431B00SG2H0002665"),
                                (194, "0ZZCE11431B00SG2H0002698"),
                                (195, "0ZZCE11431B00SG2H0002928"),
                                (196, "0ZZCE11431B00SG2H0003200"),
                            }
                            .Map(s => new Celldatavolist { identification = s.BatteryCode, locationBat = s.Channel.ToString() })
                            .Filter(s => !string.IsNullOrWhiteSpace(s.identification))],
                containerCode = "14J119214",
                actionType = "IN"

            }
                    );
            //var mesdata = mesRes.Match(
            //    Succ: s => s,
            //    Fail: f =>
            //    {
            //        mediator.Send(new WcsDataWarningBoxCommand(f.Message));
            //        throw f;
            //    }
            //    );
        }

        /// <summary>
        /// 测试托杯绑定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TestBindCupButton_Click(object sender, RoutedEventArgs e)
        {
            var mediator = App.GetService<IMediator>();
            var factory = App.GetService<IHBECSServiceFactory>();
            var hBECSService = factory.Create(MainStorage.Saves.SelectStageName);
            
            await mediator.Send(new SaveToSqlCommand(
                MainStorage.Saves.ExDbTableName,
                MainStorage.Saves.EquipNum,
                SomeData.CupBatteries,
                "Test",
                Key: MainStorage.Saves.SelectStageName
            ));
        }

        /// <summary>
        /// 测试MES托盘信息获取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TestMesContainerManualButton_Click(object sender, RoutedEventArgs e)
        {
            var mediator = App.GetService<IMediator>();
            var key = MainStorage.Saves.SelectStageName;
            var containerCode = (ViewModel.Saves.TestMesContainerCode ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(containerCode))
            {
                MessageBox.Show("请先在界面上配置托盘号(containerCode)");
                return;
            }

            try
            {
                var channels = await mediator.Send(new ContainerManualCommand(key, containerCode));
                if (channels.Count == 0)
                {
                    MessageBox.Show($"MES返回空数据\n工位: {key}\n托盘: {containerCode}");
                    return;
                }

                var lines = channels.Map(ch =>
                {
                    var (code, lv) = ch.BattleWithLevel.Match(
                        Some: b => (b.Code, b.Level.Match(Some: l => l.ToString(), None: () => "")),
                        None: () => ("", "")
                    );
                    return $"{ch.ChannelId:00}: {code} {lv}";
                });

                MessageBox.Show(string.Join("\n", lines), $"MES托盘信息获取 - {containerCode}");
            }
            catch (Exception ex)
            {
                await mediator.Send(new WcsDataWarningBoxCommand($"MES托盘信息获取失败: {ex.Message}"));
            }
        }
    }
}
