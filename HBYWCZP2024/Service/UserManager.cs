//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace HBYWCZP2024.Service;
//public class UserManager
//{
//    private readonly ILogger<UserManager> _logger;
//    private readonly MainViewModel mainViewModel;
//    private readonly IMediator _mediator;
//    bool _superUser = false;

//    int nowLevel = 0;

//    public string CurrUser = string.Empty;
//    private IEnumerable<User> nowUsers;

//    public UserManager(RPContext dataContext, ILogger<UserManager> logger, MainViewModel mainViewModel, IMediator mediator)
//    {
//        DataContext = dataContext;
//        this._logger = logger;
//        this.mainViewModel = mainViewModel;
//        _mediator = mediator;
//    }

//    public bool CreateUser(string uid, string name, PowerType powerType = PowerType.处理者)
//    {
//        var user = DataContext.Users.FirstOrDefault(s => s.RFID == uid);
//        //if (user == null)
//        {
//            DataContext.Users.Add(new Data.Model.User
//            {
//                RFID = uid,
//                UserName = name,
//                PowerType = powerType
//            });
//            DataContext.SaveChanges();
//        }
//        //else
//        //{
//        //    _logger.LogError($"{uid[3..]}***{uid[..^3]}用户已创建");
//        //}
//        //DataContext.Users.Add(new Data.Model.User { });
//        _logger.LogInformation($"{uid[3..]}***{uid[..^3]}用户创建成功");

//        return true;
//    }

//    public bool DeleteUser(string uid)
//    {
//        return true;
//    }

//    //public bool CheckRFID(string rfid) { return true; }

//    public void CheckRFID(string rfid)
//    {
//        if (rfid == "linliulanjing")
//        {
//            // 超级用户
//            _superUser = true;
//            //return true;
//        }
//        //var user = DataContext.Users.FirstOrDefault(s => s.RFID == rfid);
//        //if (user == null)
//        //{
//        //    return false;
//        //}
//        //return true;
//    }

//    public bool CheckPower(int level)
//    {
//        // 这里要记载操作人员

//        //if (_superUser || !MainViewModel.NeedAuth) { return true; }
//        if (_superUser) { return true; }
//        if (nowUsers != null && nowUsers.Any(s => s.PowerType == (PowerType)level))
//        {
//            return true;
//        }
//        var rfid = new RFIDInput();
//        rfid.ShowDialog();
//        if (rfid.DialogResult != true)
//        {
//            return false;
//        }

//        CheckRFID(rfid.RFID);
//        if (_superUser)
//        {
//            _logger.LogInformation("进入超级用户模式");
//            return true;
//        }
//        var user = DataContext.Users.Where(s => s.RFID == rfid.RFID).ToList();
//        if (user.Count == 0) { return false; }
//        _logger.LogInformation("user: {0} 进行操作", user[0].UserName);
//        CurrUser = user[0].UserName;
//        var userPowers = user.Select(s => s.PowerType).ToList();
//        return userPowers.Contains((PowerType)level) || userPowers.Contains(PowerType.管理员);
//        switch (level)
//        {
//            case 0:
//                return true;
//            case 1:
//                if (userPowers.Contains(PowerType.管理员)) return true;
//                return false;
//            case 2: break;
//            case 3: break;
//            default:
//                break;
//        }



//        return false;
//    }

//    internal async Task<bool> CheckLogin()
//    {
//        if (_superUser) { return true; }

//        Login login = new Login();
//        var res = login.ShowDialog();

//        if (res ?? false)
//        {
//            if (login.Pwd == "linliulanjing")
//            {
//                // 超级用户
//                _superUser = true;
//                await _mediator.Send(new UpdateUserLevelCommand((int)PowerType.管理员));

//                return true;
//            }
//            var user = DataContext.Users.Where(s => s.UserName == login.User && s.RFID == login.Pwd);

//            if (user.Count() != 0)
//            {
//                _logger.LogInformation("user: {user} 登陆", login.User);
//                nowUsers = user;
//                if (nowUsers.Any(s => s.PowerType == PowerType.管理员))
//                {
//                    await _mediator.Send(new UpdateUserLevelCommand((int)PowerType.管理员));

//                }
//                else
//                {
//                    await _mediator.Send(new UpdateUserLevelCommand((int)PowerType.处理者));

//                }

//            }
//        }

//        return false;

//    }

//    internal void Reset()
//    {
//        nowUsers = new List<User>();
//        _superUser = false;
//        _logger.LogInformation($"超时锁定");
//    }

//}
