using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Windows.UI.Xaml;
using static OpenFrp.Core.Api.OfApiModel.Response;
using static OpenFrp.Core.ModelHelper;

/*
    Author: Yue(越)
    Github: https://github.com/Yue-cn
    Language: Chinese
 */
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
                public SessionData()
                {

                }

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
            public class RemoveTunnelData : SessionData
            {
                public RemoveTunnelData(string? session, int id) : base(session)
                {
                    TunnelID = id;
                }

                [JsonProperty("proxy_id")]
                public int TunnelID { get; set; }
            }
            /// <summary>
            /// 创建隧道时的Body
            /// </summary>
            public class EditTunnelData : SessionData
            {
                /// <summary>
                /// 隧道名称
                /// </summary>
                [JsonProperty("name")]
                public string? TunnelName { get; set; }
                /// <summary>
                /// 隧道所在的节点ID
                /// </summary>
                [JsonProperty("node_id")]
                public int NodeID { get; set; }
                /// <summary>
                /// 本地链接
                /// </summary>
                [JsonProperty("local_addr")]
                public string? LocalAddress { get; set; } = "127.0.0.1";
                /// <summary>
                /// 本地端口
                /// </summary>
                [JsonProperty("local_port")]
                public int LocalPort { get; set; }
                /// <summary>
                /// 远程端口
                /// </summary>
                [JsonProperty("remote_port")]
                public int? RemotePort { get; set; }
                /// <summary>
                /// 绑定的域名
                /// </summary>
                [JsonProperty("domain_bind")]
                private string _BindDomain
                {
                    get => JsonConvert.SerializeObject(BindDomain);
                    set => BindDomain = JsonConvert.DeserializeObject<string[]>(value) ?? new string[0];
                }

                [JsonIgnore]
                public string[] BindDomain { get; set; } = new string[0];

                [JsonProperty("dataGzip")]
                public bool GZipMode { get; set; }

                [JsonProperty("dataEncrypt")]
                public bool EncryptMode { get; set; }
                /// <summary>
                /// 自定义参数
                /// </summary>
                [JsonProperty("custom")]
                public string CustomArgs { get; set; } = string.Empty;
                /// <summary>
                /// 请求来源
                /// </summary>
                [JsonProperty("request_from")]
                public string? RequestFrom { get; set; }
                /// <summary>
                /// Host重写
                /// </summary>
                [JsonProperty("host_rewrite")]
                public string? HostRewrite { get; set; }
                /// <summary>
                /// 请求密码
                /// </summary>
                [JsonProperty("request_pass")]
                public string? RequestPass { get; set; }
                /// <summary>
                /// URL 路由
                /// </summary>
                [JsonProperty("url_route")]
                public string? URLRoute { get; set; }

                /// <summary>
                /// 隧道类型
                /// </summary>
                [JsonProperty("type")]
                public string? TunnelType { get; set; }
                /// <summary>
                /// Proxy Protocol Version
                /// </summary>
                [JsonIgnore]
                public int ProxyProtocolVersion { get; set; }

                [JsonProperty("proxy_id")]
                public int? TunnelID { get; set; }

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
                public new UserInfoDataModel? Data { get; set; } 

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
            /// 用户隧道模型
            /// </summary>
            public class UserTunnelModel : BaseModel
            {
                /// <summary>
                /// 用户隧道列表
                /// </summary>
                [JsonProperty("data")]
                public new UserTunnelsDataModel Data { get; set; } = new();

                public class UserTunnelsDataModel
                {
                    [JsonProperty("total")]
                    public int Count { get; set; }
                    /// <summary>
                    /// 列表
                    /// </summary>
                    [JsonProperty("list")]
                    public List<UserTunnel> List { get; set; } = new();
                    
                }
                public class UserTunnel
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
                    public int? RemotePort { get; set; }
                    /// <summary>
                    /// 自定义属性
                    /// </summary>
                    [JsonProperty("custom")]
                    public string? CustomArgs { get; set; }
                    /// <summary>
                    /// 域名列表
                    /// </summary>
                    [JsonIgnore]
                    public string[] Domains { get; set; } = new string[0];

                    [JsonProperty("domain")]
                    public string _domains
                    {
                        get => JsonConvert.SerializeObject(Domains);
                        set => Domains = JsonConvert.DeserializeObject<string[]>(value) ?? new string[0];
                    }
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
                    public int TunnelId { get; set; }
                    /// <summary>
                    /// 在隧道头部加上的X-From-Where
                    /// </summary>
                    [JsonProperty("locations")]
                    public string? URLRoute { get; set; }
                    /// <summary>
                    /// 请求来源
                    /// </summary>
                    [JsonProperty("headerXFromWhere")]
                    public string? RequestFrom { get; set; }
                    /// <summary>
                    /// 请求密码
                    /// </summary>
                    [JsonProperty("sk")]
                    public string? RequestPassword { get; set; }

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
                    public string? LocalAddress { get; set; }
                    /// <summary>
                    /// 本地端口
                    /// </summary>
                    [JsonProperty("localPort")]
                    public int LocalPort { get; set; }

                    /// <summary>
                    /// 隧道名称
                    /// </summary>
                    [JsonProperty("proxyName")]
                    public string? TunnelName { get; set; }
                    /// <summary>
                    /// 隧道类型
                    /// </summary>
                    [JsonProperty("proxyType")]
                    public string? TunnelType { get; set; }

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
                    [JsonProperty("useCompression")]
                    public bool ComperssionMode { get; set; }
                    /// <summary>
                    /// 是否有加密传输
                    /// </summary>
                    [JsonProperty("useEncryption")]
                    public bool EncryptionMode { get; set; }

                    [JsonProperty("hostHeaderRewrite")]
                    public string? HostRewrite { get; set; }


                }
            }
            /// <summary>
            /// 节点模型
            /// </summary>
            public class NodesModel : BaseModel
            {
                [JsonProperty("data")]
                public new NodesData Data { get; set; } = new();

                public class NodesData
                {
                    /// <summary>
                    /// 节点列表
                    /// </summary>
                    [JsonProperty("list")]
                    public List<NodeInfo> Nodes { get; set; } = new();
                    /// <summary>
                    /// 数量
                    /// </summary>
                    [JsonProperty("total")]
                    public int Count { get; set; }
                }

                public class NodeInfo
                {
                    /// <summary>
                    /// 节点名称
                    /// </summary>
                    [JsonProperty("name")]
                    public string? NodeName { get; set; }
                    /// <summary>
                    /// 节点 ID
                    /// </summary>
                    [JsonProperty("id")]
                    public int NodeID { get; set; }
                    /// <summary>
                    /// 注释 (实际为 comments)
                    /// </summary>
                    [JsonProperty("comments")]
                    public string? Description { get; set; }
                    /// <summary>
                    /// 是否需要实名
                    /// </summary>
                    [JsonProperty("needRealname")]
                    public bool NeedRealname { get; set; }

                    [JsonProperty("classify")]
                    public NodeClassify NodeClassify { get; set; }
                    /// <summary>
                    /// 节点绑定域名
                    /// </summary>
                    [JsonProperty("hostname")]
                    public string? HostName { get; set; }
                    /// <summary>
                    /// 支持的协议
                    /// </summary>
                    [JsonProperty("protocolSupport")]
                    public NodeProtocolSupport ProtocolSupport { get; set; } = new();
                    /// <summary>
                    /// 节点状态
                    /// </summary>
                    [JsonProperty("status")]
                    public int Status { get; set; }
                    /// <summary>
                    /// 节点所需组
                    /// </summary>
                    [JsonProperty]
                    public string? Group { get; set; }
                    [JsonIgnore]
                    public bool isVipNode
                    {
                        get => Group?.IndexOf("normal") == -1;
                    }
                    /// <summary>
                    /// 最大端口
                    /// </summary>
                    [JsonIgnore]
                    public int MaxumumPort { get; set; }
                    /// <summary>
                    /// 最小端口
                    /// </summary>
                    [JsonIgnore]
                    public int MinimumPort { get; set; }

                    /// <summary>
                    /// 服务器是否满载
                    /// </summary>
                    [JsonProperty("fullyLoaded")]
                    public bool isFully { get; set; }
                    [JsonProperty]
                    private string allowPort
                    {
                        get { return ""; }
                        set
                        {
                            if (!string.IsNullOrEmpty(value))
                            {
                                string[] ports = value.Substring(1, value.Length - 2).Split(',');
                                MinimumPort = Convert.ToInt32(ports[0]);
                                MaxumumPort = Convert.ToInt32(ports[1]);
                            }
                        }
                    }
                    /// <summary>
                    /// 是否为表头 (内部用)
                    /// </summary>
                    [JsonIgnore]
                    public bool isHeader { get; set; }
                    /// <summary>
                    /// 宽带
                    /// </summary>
                    [JsonProperty("bandwidth")]
                    public int NetworkSpeed { get; set; }
                    /// <summary>
                    /// 速率翻倍
                    /// </summary>
                    [JsonProperty("bandwidthMagnification")]
                    public double SppedMagnification { get; set; }
                }
                /// <summary>
                /// 节点在世界的位置
                /// </summary>
                public enum NodeClassify
                {
                    ChinaMainland = 1,
                    ChinaTW_HK = 2,
                    Other = 3,
                }
                /// <summary>
                /// 支持的协议
                /// </summary>
                public class NodeProtocolSupport
                {
                    public ComboBoxItem[] ComboBoxUICollection
                    {
                        get
                        {
                            return new ComboBoxItem[]
                            {
                                new(){Content = "TCP",IsEnabled = TCP},
                                new(){Content = "UDP",IsEnabled = UDP},
                                new(){Content = "HTTP",IsEnabled = HTTP},
                                new(){Content = "HTTPS",IsEnabled = HTTPS},
                                new(){Content = "XTCP",IsEnabled = XTCP},
                                new(){Content = "STCP",IsEnabled = STCP},
                            };
                        }
                    }

                    public string[] SupportedMode
                    {
                        get
                        {
                            List<string> strs = new();
                            ComboBoxUICollection.ToList().ForEach(item =>
                            {
                                if (item.IsEnabled) { strs.Add(item.Content.ToString()); }
                            });
                            return strs.ToArray();
                        }
                    }

                    public int DefualtIndex { get; } = 0;

                    [JsonProperty("tcp")]
                    public bool TCP { get; set; }
                    [JsonProperty("udp")]
                    public bool UDP { get; set; }

                    [JsonProperty("http")]
                    public bool HTTP { get; set; }
                    [JsonProperty("https")]
                    public bool HTTPS { get; set; }

                    [JsonProperty("stcp")]
                    public bool STCP { get; set; }
                    [JsonProperty("xtcp")]
                    public bool XTCP { get; set; }

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
                public new LauncherPreviewDataModel Data { get; set; } = new();

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


            /// <summary>
            /// 更新信息
            /// </summary>
            public class UpdateModel : BaseModel
            {
                [JsonProperty("data")]
                public new UpdateDataModel? Data { get; set; }

                public class UpdateDataModel
                {
                    /// <summary>
                    /// FRPC - 最新版本
                    /// </summary>
                    [JsonProperty("latest_full")]
                    public string? LatestVersion { get; set; }

                    [JsonProperty("launcher")]
                    public LauncherData LauncherInfo { get; set; } = new();

                    public class LauncherData
                    {
                        /// <summary>
                        /// 启动器 - 最新版本
                        /// </summary>
                        [JsonProperty("latest")]
                        public string? LatestVersion { get; set; }
                        /// <summary>
                        /// 更新内容
                        /// </summary>
                        [JsonProperty("content")]
                        public string? Content { get; set; }
                        /// <summary>
                        /// 下载链接
                        /// </summary>
                        [JsonProperty("download_url")]
                        public string? DownloadUrl { get; set; }
                    }
                }
            }

        }
    }
}
