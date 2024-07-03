namespace HBYWCZP2024.Data.Models
{

    /// <summary>
    /// 报警日志（？
    /// </summary>
    public class WarningLog
    {
        public int Id { get; set; }
        public DateTime WarningTime { get; set; } = DateTime.Now;
        public string Message { get; set; } = string.Empty;

        public WarningType WarningType { get; set; } = WarningType.None;

    }

    public enum WarningType
    {
        None,
        Mes,
    }
}