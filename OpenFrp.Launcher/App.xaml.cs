using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace OpenFrp.Launcher
{
    public partial class App : Application
    {
        private bool isSupportDarkMode = Environment.OSVersion.Version.Major == 10 && 
            Environment.OSVersion.Version.Build >= 17763;
       
        /// <summary>
        /// 应用启动时
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine();
            if (isSupportDarkMode)
            {
                Launcher.Properties.UxTheme.AllowDarkModeForApp(true);
                Launcher.Properties.UxTheme.ShouldSystemUseDarkMode();
            }
            
        }

        /// <summary>
        /// 链接被点击
        /// </summary>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var h = (Hyperlink)sender;
            if (h.NavigateUri is not null)
            {
                Process.Start(h.NavigateUri.ToString());
            }
            
        }
    }
}
