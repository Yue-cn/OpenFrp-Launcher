using Newtonsoft.Json;
using OpenFrp.Core.Api.OfApiModel;
using OpenFrp.Core.App;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Media.Core;

namespace OpenFrp.Core.Api
{
    public static class OfApi
    {
        public static string? Authorization { get; set; }
        public static string? Session { get; set; }
        public static OfApiModel.Response.UserInfoModel.UserInfoDataModel? UserInfoDataModel { get; set; }
        public static bool LoginState => !string.IsNullOrEmpty(Authorization) && !string.IsNullOrEmpty(Session);

        public static void ClearAccount()
        {
            UserInfoDataModel = null;
            Authorization = Session = null;
        }
        /// <summary>
        /// Api交互 - 登录
        /// </summary>
        /// <param name="user">用户账户</param>
        /// <param name="password">密码</param>
        public static async ValueTask<Response.BaseModel> Login(string? user,string? password)
        {
            return await POST<Response.BaseModel>(
                OfApiUrl.Login,
                new Request.LoginData(
                    user,password).ToStringContent()
                ) ?? new() { Message = "软件请求失败。"};
        }
        /// <summary>
        /// Api交互 - 获取用户个人信息
        /// </summary>
        public static async ValueTask<Response.UserInfoModel> GetUserInfo()
        {
            if (!LoginState)
            {
                return new()
                {
                    Flag = false,
                    Message = "您尚未登录。"
                };
            }
            else
            {
                return await POST<Response.UserInfoModel>(
                    OfApiUrl.UserInfo,
                    new Request.SessionData(
                        Session).ToStringContent()
                    ) ?? new() { Message = "软件请求失败。" };
            }
        }

        /// <summary>
        /// Api交互 - 获取用户个人信息
        /// </summary>
        public static async ValueTask<Response.BaseModel> UserSignin()
        {
            if (!LoginState)
            {
                return new()
                {
                    Flag = false,
                    Message = "您尚未登录。"
                };
            }
            else
            {
                return await POST<Response.BaseModel>(
                    OfApiUrl.UserSignin,
                    new Request.SessionData(
                        Session).ToStringContent()
                    ) ?? new() { Message = "软件请求失败。" };
            }
        }
        /// <summary>
        /// Api交互 - 获取启动器公告
        /// </summary>
        /// <returns></returns>
        public static async ValueTask<Response.BaseModel> GetBroadCast()
        {
            return await GET<Response.BaseModel>(OfApiUrl.LauncherBroadCast) ??
                new()
                {
                    Message = "API请求失败。"
                };
        }
        /// <summary>
        /// Api交互 - 获取启动器信息
        /// </summary>
        public static async ValueTask<Response.LauncherPreview> GetLauncherPreview()
        {
            return await GET<Response.LauncherPreview>(OfApiUrl.LauncherInfo) ??
                new()
                {
                    Message = "API请求失败。"
                };
        }
        /// <summary>
        /// Api交互 - 获取用户隧道
        /// </summary>
        public static async ValueTask<Response.UserProxiesModel> GetUserProxies()
        {
            if (!LoginState)
            {
                return new()
                {
                    Flag = false,
                    Message = "您尚未登录。"
                };
            }
            else
            {
                var result = await POST<Response.UserProxiesModel>(
                    OfApiUrl.UserProxies,
                    new Request.SessionData(
                        Session).ToStringContent()
                    ) ?? new() { Message = "软件请求失败。" };
                if (result.Flag)
                {
                    result.Data.List.ForEach((item) =>
                    {
                        item.ProxyType = item.ProxyType?.ToUpper();
                    });
                }
                return result;
            }
        }

        public static async ValueTask<Response.BaseModel> RemoveProxy(int id)
        {
            return await POST<Response.BaseModel>(
                    OfApiUrl.RemoveProxy,
                    new Request.RemoveProxyData(
                        Session,id).ToStringContent()
                    ) ?? new() { Message = "软件请求失败。" };
        }



        public static async ValueTask<T?> POST<T>(string url,StringContent body)
        {
            var handle = new HttpClientHandler()
            {
                
            };
            if (OfSettings.Instance.BypassProxy) 
            { 
                handle.Proxy = null;
                handle.UseProxy = false;
            }
            HttpClient client = new(handle);
            
            client.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(Authorization)
                ? default : new(Authorization);

            client.Timeout = new TimeSpan(0,0,0,5);

            try
            {
                using var response =  await client.PostAsync(url, body);
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                    if (data is Response.BaseModel res)
                    {
                        // 是否有值 +  是否成功 
                        // 因为要先判断是否成功
                        if (response.Headers.Contains("Authorization")
                            && res.Flag == true
                            && res.Data is not null)
                        {
                            Session = res?.Data;
                            Authorization = response.Headers.GetValues("Authorization").First();
                        }
                    }
                    // 如果不是基本类 & 登录，那么直接返回。
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return default;
        }

        public static async ValueTask<T?> GET<T>(string url)
        {
            var handle = new HttpClientHandler()
            {

            };
            if (OfSettings.Instance.BypassProxy)
            {
                handle.Proxy = null;
                handle.UseProxy = false;
            }
            HttpClient client = new(handle);
            client.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(Authorization)
                ? default : new(Authorization);

            client.Timeout = new TimeSpan(0,0,0,5);

            try
            {
                using var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string str = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<T>(str);
                    // 如果不是基本类 & 登录，那么直接返回。
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return default;
        }

        public static async ValueTask<byte[]?> GET(string url)
        {
            var handle = new HttpClientHandler()
            {

            };
            if (OfSettings.Instance.BypassProxy)
            {
                handle.Proxy = null;
                handle.UseProxy = false;
            }
            HttpClient client = new(handle);

            client.Timeout = new TimeSpan(0,0,0,5);

            try
            {
                using var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return default;
        }
    }
}
