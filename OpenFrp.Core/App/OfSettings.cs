using ModernWpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

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

        [JsonProperty("istoastEnabled")]
        public bool IsToastEnable { get; set; } = true && Utils.isSupportToast;

        [JsonProperty("arts")]
        public List<int> AutoRunTunnel { get; set; } = new();

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
            private List<int> _Password { get; set; } = new();

            [JsonIgnore]
            public string? Password
            {
                get
                {
                    if (_Password is null) return "";
                    var builder = new List<byte>();
                    foreach (var item in _Password)
                    {
                        builder.Add((byte)(item - 12));
                    }
                    return Encoding.UTF8.GetString(builder.ToArray());
                }
                set
                {
                    foreach (var inchar in value?.GetBytes() ?? new byte[0])
                    {
                        _Password?.Add((byte)(inchar + 12));
                    }
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
                using var writer = new StreamWriter(Utils.ApplicationConfigPath);
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
}
 