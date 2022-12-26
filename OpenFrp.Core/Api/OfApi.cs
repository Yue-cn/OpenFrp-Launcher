using Newtonsoft.Json;
using OpenFrp.Core.Api.OfApiModel;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OpenFrp.Core.Api
{
    public static class OfApi
    {
        private static string? Authorization { get; set; }
        private static string? Session { get; set; }
        public static bool LoginState => !string.IsNullOrEmpty(Authorization) && !string.IsNullOrEmpty(Session);

        public static void ClearAccount() => Authorization = Session = default;
        /// <summary>
        /// Api交互 - 登录
        /// </summary>
        /// <param name="user">用户账户</param>
        /// <param name="password">密码</param>
        public static async ValueTask<Response.BaseModel> Login(string user,string password)
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

        public static async ValueTask<T?> POST<T>(string url,StringContent body)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(Authorization)
                ? default : new(Authorization);
            //client.Timeout = new TimeSpan(0,0,0,5);
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
    }
}
