using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static OpenFrp.Core.Api.OfApiModel.Response;
using static OpenFrp.Core.ModelHelper;

namespace OpenFrp.Core.Api
{
    namespace OfApiModel
    {
        public class Request
        {
            /// <summary>
            /// 登录时请求的 BODY
            /// </summary>
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
            /// <summary>
            /// 大部分功能请求时的 BODY
            /// </summary>
            public class SessionData : MessagePraser<SessionData>
            {
                public SessionData(string? session)
                {
                    Session = session;
                }

                [JsonProperty("session")]
                public string? Session { get; set; }
            }
            /// <summary>
            /// 移除隧道时的Body
            /// </summary>
            public class RemoveProxyData : SessionData
            {
                public RemoveProxyData(string? session,int id) : base(session)
                {
                    ProxyID = id;
                }

                [JsonProperty("proxy_id")]
                public int ProxyID { get; set; }
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

            public class UserProxiesModel : BaseModel
            {
                /// <summary>
                /// 用户隧道列表
                /// </summary>
                [JsonProperty("data")]
                public new UserProxiesDataModel Data { get; set; } = new();

                public class UserProxiesDataModel
                {
                    [JsonProperty("total")]
                    public int Count { get; set; }
                    /// <summary>
                    /// 列表
                    /// </summary>
                    [JsonProperty("list")]
                    public List<UserProxies> List { get; set; } = new();
                    
                }
                public class UserProxies
                {
                    /// <summary>
                    /// 内部专用 - 是否运行中
                    /// </summary>
                    [JsonIgnore]
                    public bool isRuuning { get; set; }
                    /// <summary>
                    /// 隧道链接
                    /// </summary>
                    [JsonProperty("connectAddress")]
                    public string? ConnectAddress { get; set; }

                    /// <summary>
                    /// 远程端口号
                    /// </summary>
                    [JsonProperty("remotePort")]
                    public int RemotePort { get; set; }
                    /// <summary>
                    /// 自定义属性
                    /// </summary>
                    [JsonProperty("custom")]
                    public string? CustomArgs { get; set; }
                    /// <summary>
                    /// 域名列表
                    /// </summary>
                    [JsonProperty("domain")]
                    public string[] Domains { get; set; } = new string[]{};
                    /// <summary>
                    /// 节点名称
                    /// </summary>
                    [JsonProperty("friendlyNode")]
                    public string? NodeName { get; set; }
                    /// <summary>
                    /// 节点ID
                    /// </summary>
                    [JsonProperty("node")]
                    public int NodeID { get; set; }

                    /// <summary>
                    /// 隧道 ID
                    /// </summary>
                    [JsonProperty("id")]
                    public int ProxyId { get; set; }
                    /// <summary>
                    /// 在隧道头部加上的X-From-Where
                    /// </summary>
                    [JsonProperty("headerXFromWhere")]
                    public string? X_From_Where { get; set; }

                    /// <summary>
                    /// 上次开启
                    /// </summary>
                    [JsonProperty("lastLogin")]
                    public long Lastlogin { get; set; }
                    /// <summary>
                    /// 上次更新
                    /// </summary>
                    [JsonProperty("lastUpdate")]
                    public long LastUpdate { get; set; }
                    /// <summary>
                    /// 本地IP
                    /// </summary>
                    [JsonProperty("localIp")]
                    public string? LocalIP { get; set; }
                    /// <summary>
                    /// 本地端口
                    /// </summary>
                    [JsonProperty("localPort")]
                    public int LocalPort { get; set; }

                    /// <summary>
                    /// 隧道名称
                    /// </summary>
                    [JsonProperty("proxyName")]
                    public string? ProxyName { get; set; }
                    /// <summary>
                    /// 隧道类型
                    /// </summary>
                    [JsonProperty("proxyType")]
                    public string? ProxyType { get; set; }

                    /// <summary>
                    /// 是否在线
                    /// </summary>
                    [JsonProperty("online")]
                    public bool Online { get; set; }
                    /// <summary>
                    /// 是否启用
                    /// </summary>
                    [JsonProperty("status")]
                    public bool IsEnabled { get; set; }
                    /// <summary>
                    /// 是否开启数据压缩
                    /// </summary>
                    [JsonProperty("useComperssion")]
                    public bool ComperssionMode { get; set; }
                    /// <summary>
                    /// 是否有加密传输
                    /// </summary>
                    [JsonProperty("useEncryption")]
                    public bool EncryptionMode { get; set; }
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
