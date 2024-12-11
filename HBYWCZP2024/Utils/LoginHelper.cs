using HBYWCZP2024;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.View;
using MediatR;

namespace HBYWCZP2024.Utils
{
    public class LoginHelper
    {
        static string[] powerType = ["操作员", "技师", "me技术员", "ME工程师", "系统管理员", "admin"];
        public static int GetPowerId(string id)
        {
            return System.Array.IndexOf(powerType, id) + 1;
        }
        public static string GetPowerName(int id)
        {
            return powerType[id - 1];

        }
        // 只有返回值的时候采用 无需-1
        public static string GetPowerName(string id)
        {
            return GetPowerName(int.Parse(id));
        }
        public static async Task Login()
        {
            var mediator = App.GetService<IMediator>();
            var rfid = new RFIDInput();
            rfid.ShowDialog();
            if (rfid.DialogResult != true)
            {
                return;
            }

            var aa = await mediator.Send(new LoginCommand(rfid.RFID));
            if (aa)
            {
                MainWindow.NageTo<LogDashBoardView>();
            }
            Console.WriteLine(aa);
        }
    }
}