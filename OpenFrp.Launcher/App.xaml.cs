using Microsoft.Toolkit.Uwp.Notifications;
using OpenFrp.Core;
using OpenFrp.Core.App;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
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
       
        private Process? process = Process.GetProcessesByName("OpenFrp.Core").FirstOrDefault();

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

            if (!Debugger.IsAttached || true)
            {
                AppDomain.CurrentDomain.UnhandledException += (w, s) =>
                {
                    MessageBox.Show(s.ExceptionObject.ToString());
                };
            }

            await OfSettings.ReadConfig();

            Microsoft.Win32.SystemEvents.SessionEnding += async (se, ev) =>
            {
                await OfSettings.Instance.WriteConfig();
            };

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
                    
                    if (process is null || process?.HasExited == true)
                    {
                        RegistryEvent();
                        void RegistryEvent()
                        {
                            process = Process.Start(new ProcessStartInfo(Utils.CorePath, "--ws")
                            {
                                CreateNoWindow = true,
                                UseShellExecute = false
                            });
                            process.EnableRaisingEvents = true;
                            process.Exited += (s, e) =>
                            {
                                try
                                {
                                    App.Current.Dispatcher.Invoke(async () =>
                                    {
                                        await Task.Delay(1500);
                                        var wind = ((WpfSurface)App.Current.MainWindow);
                                        if (wind is not null && wind.Visibility != Visibility.Collapsed && wind.IsLoaded == false)
                                        {
                                            RegistryEvent();
                                            wind.LauncherModel.PipeRunningState = false;
                                            wind.ClientPipeWorker(true);
                                        }
                                    });
                                }
                                catch { }
                            };
                        }
                    }
                }
                catch { }
            }
            // 系统服务模式
            else
            {
                if (!Utils.CheckService()) await Task.Delay(3250);  
            }

            if (!File.Exists(Utils.Frpc))
            {
                OfSettings.Instance.FRPClientVersion = "**";
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
            if (process is not null)
            process.EnableRaisingEvents = false;
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
