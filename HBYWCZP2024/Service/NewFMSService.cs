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

        public Task<Result<ScreenRecipeResponse>> ScreenRecipeAsync(ScreenRecipeRequest request) //暂时不用
        {
            return SendFmsRequest<ScreenRecipeResponse>("api/fms/screen/recipe", request);
        }



        public Task<Result<ScreenUploadResponse>> ScreenUploadAsync(ScreenRecipeRequest request)
        {
            return SendFmsRequest<ScreenUploadResponse>("api/fms/screen/upload", request);
        }

        public Task<Result<MpSplitResponse>> MpSplitAsync(MpSplitRequest request)
        {
            return SendFmsRequest<MpSplitResponse>("api/fms/mp/split", request);
        }



        public Task<Result<MpUploadResponse>> MpUploadAsync(MpUploadRequest request)
        {
            return SendFmsRequest<MpUploadResponse>("api/fms/mp/upload", request);
        }

        public async Task<Result<T>> SendFmsRequest<T>(string api, object data) where T : IFmsResponse
        {
            LinxContext linxContext = App.GetService<LinxContext>();
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
            string _url = $"http://{MainStorage.Saves.KeysSave[Name].ECSUrl}{api}";

            _logger.LogInformation("请求ECS地址: {url}", _url);

            try
            {
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
