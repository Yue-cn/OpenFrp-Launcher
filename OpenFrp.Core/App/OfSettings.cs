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
        [JsonProperty("theme")]
        private ElementTheme _Theme { get; set; }
        /// <summary>
        /// 应用的主题
        /// </summary>
        public ElementTheme Theme
        {
            get => _Theme;
            set
            {
                if (MainWindow is not null)
                {
                    try { ThemeManager.SetRequestedTheme(MainWindow, value); }
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
            await Task.Yield();
            
        }
        /// <summary>
        /// 写出配置
        /// </summary>
        public async ValueTask WriteConfig()
        {
            await Task.Yield();
            Instance = new(); 
        }
        /// <summary>
        /// 应用主窗口（操控台返回 Null)
        /// </summary>
        public static System.Windows.Window? MainWindow
        {
            get => System.Windows.Application.Current?.MainWindow;
        }



    }
}
