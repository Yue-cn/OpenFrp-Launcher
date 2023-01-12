using ModernWpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace OpenFrp.Core.App
{
    /// <summary>
    /// OpenFrp Launcher - 启动器设置
    /// </summary>
    public class OfSettings
    {
        /// <summary>
        /// 设置实例
        /// </summary>
        public static OfSettings Instance { get; set; } = new();

        #region Property

        /// <summary>
        /// 应用的主题 (Private to Set)
        /// </summary>
        private ElementTheme _Theme { get; set; }
        /// <summary>
        /// 应用的主题
        /// </summary>
        [JsonProperty("theme")]
        public ElementTheme Theme
        {
            get => _Theme;
            set
            {
                if (Utils.MainWindow is not null)
                {
                    try { ThemeManager.SetRequestedTheme(Utils.MainWindow, value); }
                    catch { }
                }
                _Theme = value;
            }
        }
        /// <summary>
        /// 软件运行模式(Process / Service)
        /// </summary>
        [JsonProperty("workmode")]
        public WorkMode WorkMode { get; set; }
        /// <summary>
        /// 是否绕过代理
        /// </summary>
        [JsonProperty("bypassProxy")]
        public bool BypassProxy { get; set; } = true;

        
        /// <summary>
        /// Frpc的版本
        /// </summary>
        [JsonProperty("frpcVersion")]
        public string FRPClientVersion { get; set; } = "**";

        [JsonProperty("notifiyMode")]
        public NotifiyMode NotifiyMode { get; set; } = Utils.isSupportToast ? NotifiyMode.ToastNotification : NotifiyMode.NotifiyIcon;

        [JsonProperty("arts")]
        public List<int> AutoRunTunnel { get; set; } = new();

        [JsonProperty("autoLaunchup")]
        public bool AutoLaunchUp { get; set; } = false;

        [JsonProperty("hasTips")]
        public bool HasIconTips { get; set; } = true;

        /// <summary>
        /// 用户的账户
        /// </summary>
        [JsonProperty("account")]
        public AccountModel Account { get; set; } = new();
        /// <summary>
        /// 操控台设置
        /// </summary>
        [JsonProperty("console")]
        public ConsoleModel Console { get; set; } = new();

        #endregion

        /// <summary>
        /// 账户模型
        /// </summary>
        public class AccountModel
        {
            public AccountModel() { }

            public AccountModel(string? user, string? password)
            {
                User = user;
                Password = password;
            }
            [JsonIgnore]
            public bool HasAccount
            {
                get => !string.IsNullOrEmpty(User) || !string.IsNullOrEmpty(Password);
            }

            [JsonProperty("user")]
            public string? User { get; set; }

            [JsonProperty("password")]
            private string? _Password { get; set; }


            private byte[] Entropy = new byte[20];

            [JsonIgnore]
            public string? Password
            {
                get
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(_Password))
                        {
                            Entropy = ((User?.GetMD5() + Utils.PipeRouteName).GetBytes() ?? new byte[0]);
                            List<byte> _ = new();
                            var sb = _Password?.Split(','); sb.ToList().ForEach(x =>
                            {
                                _.Add(byte.Parse(x));
                            });
                            return (ProtectedData.Unprotect(_.ToArray(), Entropy, DataProtectionScope.CurrentUser)).GetString(false);
                        }
                    }
                    catch { }
                    return "";
                }
                set
                {
                    Entropy = ((User?.GetMD5() + Utils.PipeRouteName).GetBytes() ?? new byte[0]);
                    var _ = new StringBuilder();
                    ProtectedData.Protect(value?.GetBytes(), Entropy, DataProtectionScope.CurrentUser)
                        .ToList<byte>().ForEach(x => _.Append("," + (int)x));
                    _Password = _.ToString().Remove(0,1);
                }
            }
        }
        /// <summary>
        /// 操控台模型
        /// </summary>
        public class ConsoleModel
        {
            [JsonProperty("fontSize")]
            public double FontSize { get; set; } = 14;

            [JsonProperty("fontName")]
            public string FontName { get; set; } = "微软雅黑";
        }

        /// <summary>
        /// 将对象转为 JSON 文本
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        /// <summary>
        /// 读取配置
        /// </summary>
        public static async ValueTask ReadConfig()
        {
            if (File.Exists(Utils.ApplicationConfigPath))
            {
                try
                {
                    using var reader = new StreamReader(Utils.ApplicationConfigPath);
                    JsonConvert.DefaultSettings = () => new() { NullValueHandling = NullValueHandling.Ignore };
                    var json = JsonConvert.DeserializeObject<OfSettings>(await reader.ReadToEndAsync());
                    if (json is not null)
                    {
                        Instance = json;
                        return;
                    }
                    throw new NullReferenceException("JSON 解析得到 NULL。");
                }
                catch(Exception ex)
                {
                    Utils.WriteLog(ex.ToString());
                }
            }
        }
        /// <summary>
        /// 写出配置
        /// </summary>
        public async ValueTask WriteConfig()
        {
            try
            {
                using var writer = new StreamWriter(Utils.ApplicationConfigPath,false, Encoding.UTF8, bufferSize: 2048);
                
                await writer.WriteLineAsync(JsonConvert.SerializeObject(Instance));
            }
            catch(Exception ex)
            {
                Utils.WriteLog(ex.ToString());
            }
        }
    }
    public enum WorkMode
    {
        DeamonProcess,
        DeamonService
    }
    public enum NotifiyMode
    {
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled,
        /// <summary>
        /// Windows 7 以及更高版本
        /// </summary>
        NotifiyIcon,
        /// <summary>
        /// Windows 10 以及更高版本
        /// </summary>
        ToastNotification
    }
}
 