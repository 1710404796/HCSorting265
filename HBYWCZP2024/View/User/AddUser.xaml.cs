
using HBYWCZP2024.Attritubes;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HBYWCZP2024.View.User
{
    /// <summary>
    /// AddUser.xaml 的交互逻辑
    /// </summary>
    public partial class AddUser : Window
    {
        public IEnumerable<string> RoleList { get; set; }
        public AddUser()
        {
            DataContext = this;
            RoleList = Enum.GetNames<RoleEnum>().SkipLast(1);
            InitializeComponent();
        }

        public string UserName;
        public string PassWord;
        public string PerName;

        public RoleEnum RoleEnum { get; internal set; }
        public string RFID { get; internal set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox;
            UserName = UName.Text;
            RFID = mm.Password.ToString();
            RoleEnum = (RoleEnum)Enum.Parse(typeof(RoleEnum), MType.Text);
            if (UserName == null || UserName == "")
            {
                MessageBox.Show("用户名不能为空");
            }
            else if (string.IsNullOrWhiteSpace(mm.Password))
            {
                MessageBox.Show("RF不能为空");
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
