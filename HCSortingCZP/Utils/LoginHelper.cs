using HCSortingCZP;
using HCSortingCZP.Mediator.Commands;
using HCSortingCZP.View;
using MediatR;

namespace HCSortingCZP.Utils
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
            var rfid = new RFIDInput();
            rfid.ShowDialog();
            if (rfid.DialogResult != true)
            {
                return;
            }

            var aa = await Login(rfid.RFID);
            Console.WriteLine(aa);
        }

        public static async Task<bool> Login(string rfid)
        {
            if (string.IsNullOrWhiteSpace(rfid))
            {
                return false;
            }

            var mediator = App.GetService<IMediator>();
            var aa = await mediator.Send(new LoginCommand(rfid.Trim()));
            if (aa)
            {
                MainWindow.NageTo<LogDashBoardView>();
            }

            return aa;
        }
    }
}
