using Microsoft.Toolkit.Uwp.Notifications;
using OpenFrp.Core;
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

            if (Process.GetProcessesByName("OpenFrp.Launcher").Length > 1)
            {
                Environment.Exit(0);
            }

            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += (w, s) =>
                {
                    MessageBox.Show(s.ExceptionObject.ToString());
                };
            }

            await OfSettings.ReadConfig();

            if (!Utils.IsServiceInstalled() && OfSettings.Instance.WorkMode == WorkMode.DeamonService)
            {
                OfSettings.Instance.WorkMode = WorkMode.DeamonProcess;
            }
            else if (Utils.IsServiceInstalled() && OfSettings.Instance.WorkMode == WorkMode.DeamonProcess)
            {
                OfSettings.Instance.WorkMode = WorkMode.DeamonService;
            }

            // App 运行前 守护进程检测 未开启无法进入应用
            if (OfSettings.Instance.WorkMode == WorkMode.DeamonProcess)
            {
                try
                {
                    Process? process = Process.GetProcessesByName("OpenFrp.Core").FirstOrDefault();
                    if (process is null || process?.HasExited == true)
                    {
                        Process.Start(new ProcessStartInfo(Utils.CorePath, "--ws")
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        });
                    }
                }
                catch { }
            }
            else
            {
                if (!Utils.CheckService()) await Task.Delay(3250);  
            }
            var wind = new WpfSurface();

            wind.ShowActivated = true;

            wind.Show();

            if (e.Args.FirstOrDefault() is "--minimize")
            {
                wind.Visibility = Visibility.Collapsed;
            }



        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            
            OfSettings.Instance.AutoRunTunnel = OfAppHelper.RunningIds;
            await OfSettings.Instance.WriteConfig();
        }

        /// <summary>
        /// 链接被点击
        /// </summary>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var h = (Hyperlink)sender;
            if (h.NavigateUri is not null && 
                h.Parent.GetType() != typeof(HyperlinkButton))
            {
                Process.Start(h!.NavigateUri.ToString());
            }
            
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            
        }


    }
}
