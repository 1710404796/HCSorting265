using Azure;
using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Service.Models;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Service
{
    /// <summary>
    /// 新MES服务 - 实现ECS接口
    /// </summary>
    public class NewECSService
    {
        private readonly HttpClient _httpClient;
        private readonly string _name;
        private readonly ILogger<NewECSService> _logger;

        public NewECSService(HttpClient httpClient, string name, ILogger<NewECSService> logger)
        {
            _httpClient = httpClient;
            _name = name;
            _logger = logger;
        }

        public string Name => _name;

        #region ECS接口实现

        /// <summary>
        /// 设备进站接口
        /// </summary>
        /// <param name="request">进站请求参数</param>
        /// <returns>进站响应结果</returns>
        public Task<Result<EquipEnterResponse>> EquipEnterAsync(EquipEnterRequest request)
        {
            return SendEcsRequest<EquipEnterResponse>("/ecs/api/equip/enter", request);
        }

        /// <summary>
        /// 设备出站接口
        /// </summary>
        /// <param name="request">出站请求参数</param>
        /// <returns>出站响应结果</returns>
        public Task<Result<EquipOutResponse>> EquipOutAsync(EquipOutRequest request)
        {
            return SendEcsRequest<EquipOutResponse>("/ecs/api/equip/out", request);
        }

        /// <summary>
        /// 设备状态上报
        /// </summary>
        /// <param name="request">状态上报请求参数</param>
        /// <returns>状态上报响应结果</returns>
        public Task<Result<EquipStatusResponse>> EquipStatusReportAsync(EquipStatusRequest[] request)
        {
            return SendEcsRequest<EquipStatusResponse>("/ecs/api/equip/status", request);
        }

        /// <summary>
        /// 库位状态上报(暂时不用)
        /// </summary>
        /// <param name="request">库位状态上报请求参数</param>
        /// <returns>库位状态上报响应结果</returns>
        public Task<Result<StageStatusResponse>> StageStatusReportAsync(StageStatusRequest[] request)
        {
            return SendEcsRequest<StageStatusResponse>("/ecs/api/stage/status", request);
        }

        /// <summary>
        /// 火警信息上报(暂时不用)
        /// </summary>
        /// <param name="request">火警上报请求参数</param>
        /// <returns>火警上报响应结果</returns>
        public Task<Result<FireAlarmResponse>> FireAlarmReportAsync(FireAlarmRequest request)
        {
            return SendEcsRequest<FireAlarmResponse>("/ecs/api/fire/alarm", request);
        }

        public Task<Result<CvResponse>> CvRequestAsync(CvRequest request)
        {
            return SendEcsRequest<CvResponse>("/ecs/api/equip/cvrequest", request);
        }

        #endregion


        #region 私有方法

        /// <summary>
        /// 发送ECS请求
        /// </summary>
        /// <typeparam name="T">响应类型</typeparam>
        /// <param name="api">API地址</param>
        /// <param name="data">请求数据</param>
        /// <returns>响应结果</returns>
        public async Task<Result<T>> SendEcsRequest<T>(string api, object data) where T : IEcsResponse
        {
            LinxContext linxContext = App.GetService<LinxContext>();
            DateTime reqTime = DateTime.Now;
            string req = JsonSerializer.Serialize(data);
            _logger.LogInformation("请求ECS数据: {data}", req);

            var result = await SendEcsRequest1<T>(api, data);

            WcsLog log = result.Match(
                Succ: s => new WcsLog
                {
                    IsSuccess = true,
                    ApiName = api,
                    RequestTime = reqTime,
                    Request = req,
                    Response = JsonSerializer.Serialize(s),
                    ResponseTime = DateTime.Now,
                },
                Fail: f => new WcsLog
                {
                    IsSuccess = false,
                    ApiName = api,
                    RequestTime = reqTime,
                    Request = req,
                    Response = f.Message,
                    ResponseTime = DateTime.Now,
                }
            );

            try
            {
                linxContext.WcsLogs.Add(log);
                linxContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据保存发生错误");
            }

            var mediator = App.GetService<IMediator>();
            if (result.IsSuccess)
            {
                return result;
            }
            else
            {
                var err = result.Match(Succ: s => "", Fail: f => f.Message);
                var wres = await mediator.Send(new WarningBoxCommand(
                    Name + "工作站\n" + $"ECS返回错误信息，发生问题 Error:" + err + "\n点击ok重新发送此请求，点击cancel终止此任务！",
                    Button: System.Windows.MessageBoxButton.OKCancel));

                if (wres == System.Windows.MessageBoxResult.OK)
                {
                    return await SendEcsRequest<T>(api, data);
                }
                else
                {
                    return result;
                }
            }
        }

        /// <summary>
        /// 实际发送ECS请求
        /// </summary>
        /// <typeparam name="T">响应类型</typeparam>
        /// <param name="api">API地址</param>
        /// <param name="data">请求数据</param>
        /// <returns>响应结果</returns>
        private async Task<Result<T>> SendEcsRequest1<T>(string api, object data) where T : IEcsResponse
        {
            // 这里需要根据实际的ECS服务器地址配置
            string _url = $"http://{MainStorage.Saves.KeysSave[Name].ECSUrl}{api}";

            _logger.LogInformation("请求ECS地址: {url}", _url);

            try
            {
                //, options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                var res = await _httpClient.PostAsJsonAsync(_url, data);
                //var res = await _httpClient.PostAsJsonAsync(api, data);

                if (res.IsSuccessStatusCode)
                {
                    _logger.LogInformation("请求ECS成功");
                    var resdata = await res.Content.ReadAsStringAsync();

                    _logger.LogInformation("ECS返回数据: {resdata}", resdata);
                    var response = JsonSerializer.Deserialize<T>(resdata);
                    if (response != null)
                    {
                        if (response.Success == false || response.Code != 200)
                        {
                            return new(new EcsException(response.Message));
                        }
                        return response;
                    }
                    else
                    {
                        return new(new EcsException("json为空"));
                    }
                }
                else
                {
                    return new(new EcsException($"请求ECS失败, {res.StatusCode}"));
                }
            }
            catch (Exception ex)
            {
                return new(new EcsException($"请求ECS异常, {ex.Message}"));
            }
        }

        #endregion
    }

    #region 异常定义

    public class EcsException : Exception
    {
        public EcsException() { }
        public EcsException(string? message) : base(message) { }
        public EcsException(string? message, Exception? innerException) : base(message, innerException) { }
    }

    #endregion

    #region 接口定义

    /// <summary>
    /// ECS响应接口
    /// </summary>
    public interface IEcsResponse
    {
        int Code { get; }
        //string Type { get; }
        string? Message { get; }
        bool Success { get; }
        //string? Extras { get; }
        //string Time { get; }
    }

    public interface IFmsResponse
    {
        int Code { get; }
        //string Type { get; }
        string? Message { get; }
        bool Flag { get; }
        //string? Extras { get; }
        //string Time { get; }
    }

    public abstract class FmsResponseBase : IFmsResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("msg")]
        public string? Message { get; set; }
        [JsonPropertyName("flag")]
        public bool Flag { get; set; }
    }

    #endregion

    #region 设备类型枚举

    /// <summary>
    /// 设备类型代码
    /// </summary>
    public enum EquipType
    {
        OCV = 0,
        化成_FMT = 1,
        DCIR = 2,
        筛选_SEL = 3,
        物流线_CV = 4,
        堆垛机_STACKER = 5,
        扫码枪_BCR = 6,
        组盘机_AEB = 7,
        拆盘机_DAEB = 8,
        其它_OTHER = 24
    }

    /// <summary>
    /// 设备状态枚举
    /// </summary>
    public enum EquipStatus
    {
        I, // 空闲
        T, // 有托盘
        R, // 运行
        E, // 结束
        D, // 禁用
        F  // 故障
    }

    #endregion

    #region 请求模型

    public class CvRequest(string name)

    {
        [JsonPropertyName("equipNum")]
        public string EquipNum => MainStorage.Saves.KeysSave[name].EquipNum;

        [JsonPropertyName("equipType")]
        //public int EquipType { get; set; }
        public EquipmentType EquipType => MainStorage.Saves.KeysSave[name].EquipType;

        [JsonPropertyName("trayCode")]
        public string TrayCode { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("trayType")]
        public string? TrayType { get; set; }

        [JsonPropertyName("cellCode")]
        public string? CellCode { get; set; }


        [JsonPropertyName("dest")]
        public string? Dest { get; set; }
    }


    /// <summary>
    /// 设备进站请求
    /// </summary>

    public class EquipEnterRequest(string name)
    {
        [JsonPropertyName("equipNum")]
        public string EquipNum => MainStorage.Saves.KeysSave[name].EquipNum;

        [JsonPropertyName("equipType")]
        public EquipmentType EquipType => MainStorage.Saves.KeysSave[name].EquipType;

        [JsonPropertyName("trayCode")]
        public string TrayCode { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("trayType")]
        public string? TrayType { get; set; }

        [JsonPropertyName("cellCode")]
        public string? CellCode { get; set; }
    }

    /// <summary>
    /// 设备出站请求
    /// </summary>
    public class EquipOutRequest(string name)
    {
        [JsonPropertyName("equipNum")]
        public string EquipNum => MainStorage.Saves.KeysSave[name].EquipNum;

        [JsonPropertyName("equipType")]
        public EquipmentType EquipType  => MainStorage.Saves.KeysSave[name].EquipType;

        [JsonPropertyName("trayCode")]
        public string TrayCode { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("trayType")]
        public string? TrayType { get; set; }

        [JsonPropertyName("cellCode")]
        public string? CellCode { get; set; }
    }

    /// <summary>
    /// 设备状态上报请求
    /// </summary>
    public class EquipStatusRequest(string name)
    {
        [JsonPropertyName("equipNum")]
        public string EquipNum => MainStorage.Saves.KeysSave[name].EquipNum;

        [JsonPropertyName("equipType")]
        public EquipmentType EquipType => MainStorage.Saves.KeysSave[name].EquipType;

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("status")]
        public string Status => StatusEnum.ToString();
        [JsonIgnore]
        public EquipStatus StatusEnum { get; set; } 

        [JsonPropertyName("trayCode")]
        public string? TrayCode { get; set; }

        [JsonPropertyName("cellCode")]
        public string? CellCode { get; set; } = "";

        [JsonPropertyName("ErrorMsg")]
        public string? ErrorMsg { get; set; } = "";

        [JsonPropertyName("ErrorCode")]
        public string? ErrorCode { get; set; } = "";
    }

    //public enum EquipStatusType


    /// <summary>
    /// 库位状态上报请求
    /// </summary>
    public class StageStatusRequest(string name)
    {
        [JsonPropertyName("equipNum")]
        public string EquipNum  => MainStorage.Saves.KeysSave[name].EquipNum;

        [JsonPropertyName("equipType")]
        public EquipmentType EquipType => MainStorage.Saves.KeysSave[name].EquipType;

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("status")]
        public string Status => StatusEnum.ToString();
        [JsonIgnore]
        public EquipStatus StatusEnum { get; set; }


        [JsonPropertyName("trayCode")]
        public string? TrayCode { get; set; }

        [JsonPropertyName("cellCode")]
        public string? CellCode { get; set; }

        [JsonPropertyName("ErrorMsg")]
        public string? ErrorMsg { get; set; }

        [JsonPropertyName("ErrorCode")]
        public string? ErrorCode { get; set; }
    }

    /// <summary>
    /// 火警上报请求
    /// </summary>
    public class FireAlarmRequest
    {
        [JsonPropertyName("equipNum")]
        public string EquipNum { get; set; } = string.Empty;

        [JsonPropertyName("equipType")]
        public int EquipType { get; set; }

        [JsonPropertyName("wareHouseName")]
        public string WareHouseName { get; set; } = string.Empty;

        [JsonPropertyName("stackerNum")]
        public string StackerNum { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("locationType")]
        public string LocationType { get; set; } = string.Empty;
    }


    public class ScreenRecipeRequest(string name)
    {
        public string trayCode { get; set; } = string.Empty;
        public string equipNum { get; set; } = MainStorage.Saves.KeysSave[name].EquipNum;
        public string trayType { get; set; } = "1";
        [JsonPropertyName("cellList")]
        public CellInfo[] CellList { get; set; } = new CellInfo[0];
    }
    public class MpSplitRequest(string name)
    {
        public string trayCode { get; set; } = string.Empty;
        public string equipNum { get; set; } = MainStorage.Saves.KeysSave[name].EquipNum;
    }

    public class MpUnbindWholeTrayRequest(string name)
    {
        public string trayCode { get; set; } = string.Empty;
        public string equipNum { get; set; } = MainStorage.Saves.KeysSave[name].EquipNum;
    }

    public class MpUploadRequest(string name)
    {
        public string trayCode { get; set; }
        public string equipNum { get; set; } = MainStorage.Saves.KeysSave[name].EquipNum;
        public string ProductModelCode { get; set; } = MainStorage.Saves.KeysSave[name].ProductModelCode;
        public string stepCode { get; set; } = MainStorage.Saves.KeysSave[name].StepCode;
        public string lotNo { get; set; } = string.Empty;
        public Cellcode[] cellCodes { get; set; } = [];
    }

    public class Cellcode
    {
        public string cellCode { get; set; } = string.Empty;
        public int channel { get; set; }
        public int cellType { get; set; }
        public string gradingType { get; set; } = string.Empty;
    }



    #endregion

    #region 响应模型

    /// <summary>
    /// ECS响应基类
    /// </summary>
    public abstract class EcsResponseBase : IEcsResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("extras")]
        public string? Extras { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;
    }

    /// <summary>
    /// 设备进站响应
    /// </summary>
    public class CvResponse : EcsResponseBase
    {

    }


    /// <summary>
    /// 设备进站响应
    /// </summary>
    public class EquipEnterResponse : EcsResponseBase
    {
        [JsonPropertyName("result")]
        public EquipEnterResult? Result { get; set; }
    }

    /// <summary>
    /// 设备出站响应
    /// </summary>
    public class EquipOutResponse : EcsResponseBase
    {
        [JsonPropertyName("result")]
        public EquipOutResult? Result { get; set; }
    }

    /// <summary>
    /// 设备状态上报响应
    /// </summary>
    public class EquipStatusResponse : EcsResponseBase
    {
        [JsonPropertyName("result")]
        public StatusReportResult? Result { get; set; }
    }

    /// <summary>
    /// 库位状态上报响应
    /// </summary>
    public class StageStatusResponse : EcsResponseBase
    {
        [JsonPropertyName("result")]
        public StatusReportResult? Result { get; set; }
    }

    /// <summary>
    /// 火警上报响应
    /// </summary>
    public class FireAlarmResponse : EcsResponseBase
    {
        [JsonPropertyName("result")]
        public StatusReportResult? Result { get; set; }
    }

    public class ScreenRecipeResponse : FmsResponseBase
    {


    }

    public class ScreenUploadResponse : FmsResponseBase
    {

    }
    public class MpSplitResponse : FmsResponseBase
    {

    }

    public class MpUnbindWholeTrayResponse : FmsResponseBase
    { 
    
    }

    public class MpUploadResponse : FmsResponseBase
    {

    }

    #endregion

    #region 响应结果模型

    /// <summary>
    /// 设备进站结果
    /// </summary>
    public class EquipEnterResult
    {
        [JsonPropertyName("channelNum")]
        public int? ChannelNum { get; set; }

        [JsonPropertyName("cellList")]
        //public CellInfo[] CellList { get; set; } = new CellInfo[0];
        public CellInfo[]? CellList { get; set; } = null;

        [JsonPropertyName("trayCode")]
        public string? TrayCode { get; set; }

        [JsonPropertyName("trayType")]
        public string? TrayType { get; set; }

        [JsonPropertyName("productType")]
        public string ProductType { get; set; }

        [JsonPropertyName("ret")]
        public int Ret { get; set; }

        [JsonPropertyName("ngCodes")]
        //public string[] NgCodes { get; set; } = new string[0];
        public string[]? NgCodes { get; set; } = null;
    }

    /// <summary>
    /// 设备出站结果
    /// </summary>
    public class EquipOutResult
    {
        [JsonPropertyName("channelNum")]
        public int? ChannelNum { get; set; }

        [JsonPropertyName("cellList")]
        public CellInfo[] CellList { get; set; } = new CellInfo[0];

        [JsonPropertyName("trayCode")]
        public string? TrayCode { get; set; }

        [JsonPropertyName("trayType")]
        public string? TrayType { get; set; }

        [JsonPropertyName("ret")]
        public int Ret { get; set; }

        [JsonPropertyName("ngCodes")]
        public string[] NgCodes { get; set; } = new string[0];
    }

    /// <summary>
    /// 状态上报结果
    /// </summary>
    public class StatusReportResult
    {
        [JsonPropertyName("ret")]
        public int Ret { get; set; }

        [JsonPropertyName("ngCodes")]
        public string[] NgCodes { get; set; } = new string[0];
    }

    /// <summary>
    /// 电芯信息
    /// </summary>
    public class CellInfo
    {
        [JsonPropertyName("cellCode")]
        public string CellCode { get; set; } = string.Empty;

        [JsonPropertyName("channel")]
        public int Channel { get; set; }

        [JsonPropertyName("cellType")]
        public int CellType { get; set; }
        [JsonPropertyName("gradingType")]
        public string? GradingType { get; set; }
    }




    #endregion

    public enum EquipmentType
    {
        // 设备类型代码为 0: OCV
        OCV = 0,

        // 设备类型代码为 1: 化成 FMT
        FMT = 1,

        // 设备类型代码为 2: DCIR
        DCIR = 2,

        // 设备类型代码为 3: 筛选 SEL
        SEL = 3,

        // 设备类型代码为 4: 物流线 CV
        CV = 4,

        // 设备类型代码为 5: 堆垛机 STACKER
        STACKER = 5,

        // 设备类型代码为 6: 扫码轮 BCR
        BCR = 6,

        // 设备类型代码为 7: 组盘机 AEB
        AEB = 7,

        // 设备类型代码为 8: 拆盘机 DAEB
        DAEB = 8,

        // 设备类型代码为 9: 注液机 LIQUID
        LIQUID = 9,

        // 设备类型代码为 10: 包膜机 LAMINAT
        LAMINAT = 10,

        // 设备类型代码为 11: 充气打扣机 NAIL
        NAIL = 11,

        // 设备类型代码为 12: 清洗机 CLEAR
        CLEAR = 12,

        // 设备类型代码为 13: 后气检机 HELIUM
        HELIUM = 13,

        // 设备类型代码为 14: 密封打扣机 SEAL
        SEAL = 14,

        // 设备类型代码为 15: 外观检测机 INSPECT
        INSPECT = 15,

        // 设备类型代码为 16: 火灾 FIRE
        FIRE = 16,

        // 设备类型代码为 17: 扣束机 RESTRAINT
        RESTRAINT = 17,

        // 设备类型代码为 18: 拧扣机 PININ
        PININ = 18,

        // 设备类型代码为 19: 拧盘机 PINOUT
        PINOUT = 19,

        // 设备类型代码为 20: 换盘机 CEB
        CEB = 20,

        // 设备类型代码为 21: 预充 PRECHARGE
        PRECHARGE = 21,

        // 设备类型代码为 22: 分容机 CAPACITY
        CAPACITY = 22,

        // 设备类型代码为 23: SOC
        SOC = 23,

        // 设备类型代码为 24: 其他 OTHER
        OTHER = 24
    }

}
