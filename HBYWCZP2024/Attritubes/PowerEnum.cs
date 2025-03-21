namespace HBYWCZP2024.Attritubes
{
    /// <summary>
    /// 软件所有权限
    /// </summary>
    public enum PowerEnum
    {
        报警日志,
        WCS交互日志,
        工位状态,
        拆盘日志,
        组盘日志,
        权限配置表,
        用户管理界面,
        电芯计数,
        调度号工位表,
        条码规则表,
        权限配置,
        程序参数设定,
        交互日志,
        日志窗口,
        重新匹配PLC节点,
        接口使能配置,
        关闭软件
    }

    public enum RoleEnum
    {
        操作员 = 1,
        技师 = 2,
        me技术员 = 3,
        ME工程师 = 4,
        系统管理员 = 5,

        SuperAdmin = 999
    }

}