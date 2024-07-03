using System.ComponentModel.DataAnnotations.Schema;

namespace HBYWCZP2024.Data.Models
{
    public enum PowerType
    {
        处理者,
        管理员,
        匿名,
        // 额外权限
        添加者,
    }
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PassWord { get; set; } = string.Empty;
        public string PerName { get; set; } = string.Empty;
        public State State { get; set; } = State.Enable;
        /// <summary>
        /// RFID
        /// </summary>
        public string RFID { get; set; } = string.Empty;
        public PowerType PowerType { get; set; }
        [NotMapped]
        public string RFStar => RFID.Length < 4 ? string.Empty : $"{RFID[..4]}********";
    }

    public enum State
    {
        Disable,
        Enable
    }
}
