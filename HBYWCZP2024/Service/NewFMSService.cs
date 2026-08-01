using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using HBYWCZP2024.Mediator.Commands;
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
using System.Threading.Tasks;

namespace HBYWCZP2024.Service
{
    public class NewFMSService
    {
        private readonly HttpClient _httpClient;
        private readonly string _name;
        private readonly ILogger<NewFMSService> _logger;

        public string Name => _name;

        public NewFMSService(HttpClient httpClient, string name, ILogger<NewFMSService> logger)
        {
            _httpClient = httpClient;
            _name = name;
            _logger = logger;
        }

        /// <summary>
        /// 圆柱产品获取配方
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<Result<YZScreenRecipeRespone>> YZScreenRecipeAsync(YZScreenRecipeRequest request)
        {
            return SendFmsRequest<YZScreenRecipeRespone>("/api/fms/screen/recipe", request);
        }

        /// <summary>
        /// 筛选夹住交互
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<Result<YZScreenStepResponse>> YZScreenStepAsync(YZScreenStepRequest request)
        {
            return SendFmsRequest<YZScreenStepResponse>("/api/fms/screen/step",request);
        }

        /// <summary>
        /// 圆柱筛选结果上传
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<Result<YZScreenUploadResponse>> YZScreenUploadAsync(YZScreenUploadRequest request)
        {
            return SendFmsRequest<YZScreenUploadResponse>("/api/fms/screen/upload", request);
        }

        /// <summary>
        /// 圆柱筛选NG叫盘
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<Result<YZCallMaterialResponse>> YZCallMaterialAsync(YZCallMaterialRequest request)
        {
            return SendFmsRequest<YZCallMaterialResponse>("/api/fms/screen/call/material",request);
        }

        /// <summary>
        /// 方形筛选后组盘结果上传
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<Result<ScreenUploadResponse>> ScreenUploadAsync(ScreenUploadRequest request)
        {
            return SendFmsRequest<ScreenUploadResponse>("/api/fms/screen/upload", request);
        }

        /// <summary>
        /// 单拆解绑
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<Result<MpSplitResponse>> MpSplitAsync(MpSplitRequest request)
        {
            return SendFmsRequest<MpSplitResponse>("/api/fms/mp/split", request);
        }

        /// <summary>
        /// 整盘解绑
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<Result<MpUnbindWholeTrayResponse>> MpunbindWholeTrayAsync(MpUnbindWholeTrayRequest request)
        {
            return SendFmsRequest<MpUnbindWholeTrayResponse>("/api/fms/mp/unbindWholeTray", request);
        }

        /// <summary>
        /// 组盘结果上传
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<Result<MpUploadResponse>> MpUploadAsync(MpUploadRequest request)
        {
            return SendFmsRequest<MpUploadResponse>("/api/fms/mp/upload", request);
        }

        /// <summary>
        /// 复测组盘结果上传
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<Result<RetestUploadResponse>> RetestUploadAsync(RetestUploadRequest request)
        {
            return SendFmsRequest<RetestUploadResponse>("/api/fms/retest/upload", request);
        }

        public async Task<Result<T>> SendFmsRequest<T>(string api, object data) where T : IFmsResponse
        {
            using LinxContext linxContext = App.GetService<LinxContext>(); // 用using确保DbContext用完即释放，避免内存堆积
            DateTime reqTime = DateTime.Now;
            string req = JsonSerializer.Serialize(data);
            _logger.LogInformation("请求FMS数据: {data}", req);

            var result = await SendFmsRequest1<T>(api, data);

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
                if ("/api/fms/screen/call/material" == api)
                {
                    return result;
                }

                var err = result.Match(Succ: s => "", Fail: f => f.Message);
                var wres = await mediator.Send(new WarningBoxCommand(
                    Name + "工作站\n" + $"FMS返回错误信息，发生问题 Error:" + err + "\n点击ok重新发送此请求，点击cancel终止此任务！",
                    Button: System.Windows.MessageBoxButton.OKCancel));

                if (wres == System.Windows.MessageBoxResult.OK)
                {
                    return await SendFmsRequest<T>(api, data);
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
        private async Task<Result<T>> SendFmsRequest1<T>(string api, object data) where T : IFmsResponse
        {
            // 这里需要根据实际的ECS服务器地址配置
            string _url = $"http://{MainStorage.Saves.KeysSave[Name].FMSUrl}{api}";

            _logger.LogInformation("请求ECS地址: {url}", _url);

            try
            {
                var res = await _httpClient.PostAsJsonAsync(_url, data);
                //var res = await _httpClient.PostAsJsonAsync(api, data);

                if (res.IsSuccessStatusCode)
                {
                    _logger.LogInformation("请求FMS成功");
                    var resdata = await res.Content.ReadAsStringAsync();

                    _logger.LogInformation("FMS返回数据: {resdata}", resdata);
                    var response = JsonSerializer.Deserialize<T>(resdata);
                    if (response != null)
                    {
                        if (response.Flag == false || response.Code != 200)
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

    }
}
