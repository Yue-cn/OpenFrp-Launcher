using ModernWpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
}
