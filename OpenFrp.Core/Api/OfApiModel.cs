using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using static OpenFrp.Core.Api.OfApiModel.Response;
using static OpenFrp.Core.ModelHelper;

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
            /// <summary>
            /// 基本模型
            /// </summary>
            public class BaseModel : MessagePraser<BaseModel>
            {
                public BaseModel() { }

                public BaseModel(string? data = null, bool flag = false, string message = "")
                {
                    Data = data;
                    Flag = flag;
                    Message = message;
                }
                public BaseModel(string message = "")
                {
                    Message = message;
                }

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
            /// <summary>
            /// 用户个人模型
            /// </summary>
            public class UserInfoModel : BaseModel
            {
                /// <summary>
                /// 个人的用户信息
                /// </summary>
                [JsonProperty("data")]
                public new UserInfoDataModel Data { get; set; } = new();

                public class UserInfoDataModel
                {
                    /// <summary>
                    /// 用户邮箱
                    /// </summary>
                    [JsonProperty("email")]
                    public string Email { get; set; } = "TestMail@OpenFrp.cn";

                    /// <summary>
                    /// 用户名
                    /// </summary>
                    [JsonProperty("username")]
                    public string UserName { get; set; } = "OpenFrp.App";

                    /// <summary>
                    /// 用户所在组名称
                    /// </summary>
                    [JsonProperty("friendlyGroup")]
                    public string GroupCName { get; set; } = "普通用户";
                    /// <summary>
                    /// 用户所在组
                    /// </summary>
                    [JsonProperty("group")]
                    public string Group { get; set; } = "normal";

                    /// <summary>
                    /// 用户 ID
                    /// </summary>
                    [JsonProperty("id")]
                    public int UserID { get; set; }

                    /// <summary>
                    /// 最大隧道数量
                    /// </summary>
                    [JsonProperty("proxies")]
                    public int MaxProxies { get; set; }
                    /// <summary>
                    /// 已使用的隧道数
                    /// </summary>
                    [JsonProperty("used")]
                    public int UsedProxies { get; set; }

                    /// <summary>
                    /// 可用流量
                    /// </summary>
                    [JsonProperty("traffic")]
                    public long Traffic { get; set; }

                    /// <summary>
                    /// 用户 Token
                    /// </summary>
                    [JsonProperty("token")]
                    public string? UserToken { get; set; }

                    /// <summary>
                    /// 是否已实名
                    /// </summary>
                    [JsonProperty("realname")]
                    public bool isRealname { get; set; }
                    /// <summary>
                    /// 入口带宽速率
                    /// </summary>
                    [JsonProperty("inLimit")]
                    public int InputLimit { get; set; }
                    /// <summary>
                    /// 出口带宽速率
                    /// </summary>
                    [JsonProperty("outLimit")]
                    public int OutputLimit { get; set; }
                }
            }

            /// <summary>
            /// 启动器首页大图模型
            /// </summary>
            public class LauncherPreview : BaseModel
            {
                public LauncherPreview() { }

                /// <summary>
                /// 启动器 首页预览
                /// </summary>
                [JsonProperty("data")]
                public new _LauncherPreviewStruct Data { get; set; } = new();

                public class _LauncherPreviewStruct
                {
                    /// <summary>
                    /// 首页大图的信息
                    /// </summary>
                    [JsonProperty("info")]
                    public LauncherPreviewDataModel Info { get; set; } = new();
                }

                public class LauncherPreviewDataModel
                {
                    /// <summary>
                    /// 标题
                    /// </summary>
                    [JsonProperty("title")]
                    public string Title { get; set; } = "初景革绪风，新阳改故阴";

                    /// <summary>
                    /// 内容
                    /// </summary>
                    [JsonProperty("content")]
                    public string Content { get; set; } = "打开官网";

                    /// <summary>
                    /// 图片 链接
                    /// </summary>
                    [JsonProperty("image")]
                    public string? ImageUrl { get; set; }
                    /// <summary>
                    /// 链接
                    /// </summary>
                    [JsonProperty("link")]
                    public string? Link { get; set; } = "https://www.openfrp.net";
                }
            }

        }
    }
}
