using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenFrp.Core.Api.OfApiModel.Response;

namespace OpenFrp.Core.Api
{
    namespace OfApiModel
    {
        public class Request
        {
            public class LoginData : MessagePraser<BaseModel>
            {
                public LoginData(string? userName, string? password)
                {
                    UserName = userName;
                    Password = password;
                }

                [JsonProperty("user")]
                public string? UserName { get; set; }

                [JsonProperty("password")]
                public string? Password { get; set; } 
            }

            public class SessionData : MessagePraser<SessionData>
            {
                public SessionData(string? session)
                {
                    Session = session;
                }

                [JsonProperty("session")]
                public string? Session { get; set; }
            }
        }
        public class Response
        {
            public class UserInfoModel : BaseModel
            {
                /// <summary>
                /// 个人的用户信息
                /// </summary>
                [JsonProperty("data")]
                public new UserInfoDataModel Data { get; set; } = new();

                public class UserInfoDataModel
                {
                    [JsonProperty("email")]
                    public string Email { get; set; } = "TestMail@OpenFrp.cn";
                    [JsonProperty("username")]
                    public string UserName { get; set; } = "OpenFrp.App";
                }
            }

            public class BaseModel : MessagePraser<BaseModel>
            {
                /// <summary>
                /// API 返回的数据内容
                /// </summary>
                [JsonProperty("data")]
                public string? Data { get; set; }

                /// <summary>
                /// API 返回的状态
                /// </summary>
                [JsonProperty("flag")]
                public bool Flag { get; set; }

                /// <summary>
                /// API 返回的消息
                /// </summary>
                [JsonProperty("msg")]
                public string Message { get; set; } = string.Empty;
            }
        }
        public class MessagePraser<T>
        {

            /// <summary>
            /// 从文本解析 JSON。
            /// </summary>
            /// <param name="str">JSON String</param>
            /// <returns><typeparamref name="T"/></returns>
            public static T? PraseFrom(string str)
            {
                return JsonConvert.DeserializeObject<T>(str);
            }
            /// <summary>
            /// 返回该对象的 JSON 文本。
            /// </summary>
            /// <returns>JSON String</returns>
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
            /// <summary>
            /// 返回该对象的 <see cref="StringContent"/>
            /// </summary>
            /// <returns></returns>
            public StringContent ToStringContent()
            {
                return new(ToString(), Encoding.UTF8, "application/json");
            }
        }


    }
}
