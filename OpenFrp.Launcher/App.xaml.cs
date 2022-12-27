using OpenFrp.Core.App;
using System;
using System.CodeDom;
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
        protected override async void OnStartup(StartupEventArgs e)
        {

            if (isSupportDarkMode)
            {
                Launcher.Properties.UxTheme.AllowDarkModeForApp(true);
                Launcher.Properties.UxTheme.ShouldSystemUseDarkMode();
            }
            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += (w, s) =>
                {
                    MessageBox.Show(s.ExceptionObject.ToString());
                };
            }
            
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            await OfSettings.Instance.WriteConfig();
        }

        /// <summary>
        /// 链接被点击
        /// </summary>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            
            var h = (Hyperlink)sender;
            if (h.NavigateUri is not null && 
                h.Parent.GetType() != typeof(HyperlinkButton))
            {
                Process.Start(h!.NavigateUri.ToString());
            }
            
        }
    }
}
