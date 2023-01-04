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
        public string FRPClientVersion { get; set; } = "OpenFRP_0.46.0_e9886afa_20221227 **";
        /// <summary>
        /// 用户的账户
        /// </summary>
        [JsonProperty("account")]
        public AccountModel Account { get; set; } = new();
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
            public string? Password { get; set; }
        }

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
