using OpenFrp.Core;
using OpenFrp.Core.Api;
using OpenFrp.Core.App;
using OpenFrp.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Toolkit.Uwp.Notifications;
using OpenFrp.Launcher.Views;
using System.Windows.Markup;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf;

namespace OpenFrp.Launcher
{
    /// <summary>
    /// WpfSurface.xaml 的交互逻辑
    /// </summary>
    public partial class WpfSurface : Window
    {
        public WpfSurface()
        {
            InitializeComponent();
        }

        internal LauncherModel LauncherModel
        {
            get => (LauncherModel)DataContext;
            set => DataContext = value;
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);


            GC.RegisterForFullGCNotification(46, 23);
            Directory.CreateDirectory(Utils.AppTempleFilesPath);
            Directory.CreateDirectory(Path.Combine(Utils.AppTempleFilesPath, "static"));
            ShowActivated = true;
            // 启动器更新逻辑
            CheckUpdate();



            OfAppHelper.TaskbarIcon.ContextMenu = new()
            {
                Items =
                {
                    CreateItemWithAction("显示窗口", () =>
                    {
                        Visibility = Visibility.Visible;
                        WindowState = WindowState.Normal;

                        if (this.IsLoaded)Activate();
                        else Show();


                    },new FontIcon(){Glyph = $"\ue73f"}),
                    new Separator(),
                    CreateItemWithAction("强制退出", () =>
                    {
                        if (Process.GetProcessesByName("OpenFrp.Core").FirstOrDefault() is Process process && process.HasExited)
                        {
                            process.Kill();
                        }
                        Utils.KillAllFrpc();
                        App.Current.Shutdown();
                    },new FontIcon(){Glyph = "\ue74d"}),
                    CreateItemWithAction("退出启动器",App.Current.Shutdown,new FontIcon(){Glyph = "\ue89f"}),
                    CreateItemWithAction("彻底退出", async () =>
                    {
                        await OfAppHelper.PipeClient.PushMessageAsync(new()
                        {
                            Action = Core.Pipe.PipeModel.OfAction.Close_Server,
                        });
                        Utils.KillAllFrpc();
                        App.Current.Shutdown();
                    },new FontIcon(){Glyph = "\ue8bb"})
                },
                Placement = System.Windows.Controls.Primitives.PlacementMode.Right,
            };
            OfAppHelper.TaskbarIcon.TrayLeftMouseUp += (sender, args) =>
            {
                Visibility = Visibility.Visible;
                WindowState = WindowState.Normal;
                if (this.IsLoaded) Activate();
                else Show();
            };
            OfApp_NavigationView.ItemInvoked += (s, e) =>
            {
                Type? page = null;
                if (e.IsSettingsInvoked)
                {
                    page = typeof(Views.Setting);
                }
                else
                {
                    page = (s.SelectedItem as NavigationViewItem)?.Tag switch
                    {
                        "Home" => typeof(Views.Home),
                        "About" => typeof(Views.About),
                        "Tunnels" => typeof(Views.Tunnels),
                        "Logs" => typeof(Views.Logs),
                        "Toolkit" => typeof(Views.Toolkit),
                        _ => null
                    };
                }
                if (page is not null)
                OfApp_RootFrame.Navigate(page);
            };
            OfApp_RootFrame.Navigating += (s, e) =>
            {
                if (e.Uri is null)
                {
                    OfApp_RootFrame.RemoveBackEntry();
                }
                else { e.Cancel = true; }
            };
            
            // Defualt Pipe Server 
            // 服务端 单独发给 客户端，不需要客户端先发送请求。
            ServerPipeWorker();
            ClientPipeWorker();
            await OfAppHelper.RequestLogin();


            // 启动器 自动登录逻辑
            

            await Task.Delay(3500);
            // 自动启动 FRPC
            AutoStartup();

            


        }
        
        private async void AutoStartup()
        {
            if (OfSettings.Instance.AutoRunTunnel.Count == 0 || 
                OfSettings.Instance.WorkMode == WorkMode.DeamonService) 
                return;

            var resp = await OfApi.GetUserProxies();
            if (resp.Flag)
            {
                foreach (var tunnel in resp.Data.List)
                {
                    // 已经开启的隧道 就不需要再次开启了
                    if (!OfAppHelper.RunningIds.Contains(tunnel.TunnelId))
                    {
                        var lts = new List<Core.Api.OfApiModel.Response.UserTunnelModel.UserTunnel>();
                        OfSettings.Instance.AutoRunTunnel.ForEach((tunnelId) =>
                        {
                            // 如果开机自启的隧道 与 foreach 中的隧道一致,那么开启
                            if (tunnelId == tunnel.TunnelId)
                            {
                                /*
                                    Date: 2023-1-13
                                    逻辑问题，应是 如果RunningId列表(前提是已请求) 已有隧道，
                                    那么不再请求自启动。()
                                 */
                                if (!OfAppHelper.RunningIds.Contains(tunnelId))
                                {
                                    lts.Add(tunnel);
                                    OfAppHelper.RunningIds.Add(tunnelId);
                                }
                                return;
                            }
                        });
                        var resp2 = await OfAppHelper.PipeClient.PushMessageWithRequestAsync(new()
                        {
                            Action = Core.Pipe.PipeModel.OfAction.Start_Frpc,
                            LaunchTunnels = lts
                        });
                    }
                }
            }
        }

        private MenuItem CreateItemWithAction(string name, Action action, object icon)
        {
            var app2 = new MenuItem() { Header = name, Icon = icon };
            app2.Click += (sender, args) => action();
            return app2;
        }
        /// <summary>
        /// 窗口关闭
        /// </summary>
        protected override async void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            if (OfSettings.Instance.HasIconTips)
            {
                var dialog = new ContentDialog()
                {
                    CloseButtonText = "确定",
                    DefaultButton = ContentDialogButton.Close,
                    Title = "OpenFrp Launcher",
                    Content = new SimpleStackPanel()
                    {
                        Children =
                        {
                            new Image()
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/OpenFrp.Launcher;component/Resourecs/notip.png")),
                                Width = 300,
                                Stretch = Stretch.UniformToFill
                            },
                            new TextBlock(new Run("本软件已创建图标在您的任务栏上。"))
                        },
                        Spacing = 8
                    }
                };
                await dialog.ShowAsync();
                OfSettings.Instance.HasIconTips = false;
            }
            Visibility = Visibility.Collapsed;
            //OfApp_NavigationView.DisplayMode == NavigationViewDisplayMode.
        }
        /// <summary>
        /// 检测更新
        /// </summary>
        internal async void CheckUpdate()
        {
            var update = await Update.CheckUpdate();
            if (update.UpdateFor is not Update.UpdateFor.None)
            {
                if (update.UpdateFor is Update.UpdateFor.Launcher)
                {
                    if (App.Current.MainWindow.Visibility == Visibility.Collapsed)
                    {
                        App.Current.MainWindow.Visibility = Visibility.Visible;
                    }
                    if (OfAppHelper.HasDialog)
                    {
                        OfAppHelper.TaskbarIcon.ShowBalloonTip("OpenFrp.Launcher", "应用有更新，请打开关于选项卡,点击\"检查更新\"", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                        return;
                    }
                    OfAppHelper.HasDialog = true;
                    var dialog = new ContentDialog()
                    {
                        DefaultButton = ContentDialogButton.Primary,
                        Title = "启动器有更新啦",
                        Content = new TextBlock(new Run(update.Content + $"\n现在是 {Utils.ApplicationVersions}"))
                        {
                            Width = 350,
                            MinHeight = 150,
                            TextWrapping = TextWrapping.Wrap,
                        },
                        PrimaryButtonText = "下载并安装",
                        CloseButtonText = "取消",
                    };
                    
                    dialog.PrimaryButtonClick += async (sender, args) =>
                    {
                        args.Cancel = true;
                        string file = Path.Combine(Utils.AppTempleFilesPath, $"{update.DownloadUrl?.GetMD5()}.exe");

                        dialog.IsPrimaryButtonEnabled = false;
                        dialog.CloseButtonText = null;

                        var client = new HttpClient();
                        var progress = new ProgressRing()
                        {
                            Width = 70,
                            Height = 70,
                            IsIndeterminate = false,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        if (File.Exists(file))
                        {
                            progress.Value = 100;
                            Install();
                            return;
                        }
                        dialog.Content = new Grid()
                        {
                            Width = 350,
                            MinHeight = 150,
                            Children =
                            {
                                progress
                            }

                        };
                        var isSuccess = await Update.DownloadWithProgress(update.DownloadUrl!,file, (sender, args) =>
                        {
                            progress.Value = args.ProgressPercentage;
                        });
                        if (isSuccess)
                        {
                            Install();
                            Hide();
                            OfAppHelper.HasDialog = false;
                        }
   
                        dialog.Content = "下载失败。";
                        dialog.PrimaryButtonText = "";
                        dialog.CloseButtonText = "确定";
                        if (File.Exists(file)) { File.Delete(file); }
                    };

                    await dialog.ShowAsync();

                    async void Install()
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(Path.Combine(Utils.AppTempleFilesPath, $"{update.DownloadUrl?.GetMD5()}.exe"))
                            {
                                Verb = "runas"
                            });
                            Utils.StopService();
                            if ((await OfAppHelper.PipeClient.PushMessageAsync(new()
                            {
                                Action = Core.Pipe.PipeModel.OfAction.Close_Server,
                                Message = "关闭。"
                            })).Action == Core.Pipe.PipeModel.OfAction.Server_Closed)
                            {
                                if (OfAppHelper.LauncherViewModel is not null)
                                    OfAppHelper.LauncherViewModel.PipeRunningState = false;
                            }
                            Environment.Exit(0);
                        }
                        catch { }
                        dialog.Hide();
                        return;
                    }

                    return;

                }
                else
                {
                    LauncherModel.IsFrpchasUpdate = true;
                    // 如果没有更新，还没有FRPC文件,那么直接下载。
                    if (!File.Exists(Utils.Frpc))
                    {
                        await OfAppHelper.PipeClient.PushMessageAsync(new()
                        {
                            Action = Core.Pipe.PipeModel.OfAction.Close_Server,
                        });

                        Process.Start(new ProcessStartInfo(Utils.CorePath, "--frpcp")
                        {
                            UseShellExecute = false,
                            Verb = "runas"
                        });

                        Application.Current.Shutdown();

                        return;
                    }
                }
            }

        }
        /// <summary>
        /// Pipe Service
        /// </summary>
        internal async void ClientPipeWorker(bool restart = false)
        {
            await OfAppHelper.PipeClient.Start();
            
            if (OfSettings.Instance.WorkMode is WorkMode.DeamonProcess)
            {
                // 寻找正在运行的隧道
                var resp =  await OfAppHelper.PipeClient.PushMessageAsync(new()
                {
                    Action = Core.Pipe.PipeModel.OfAction.Get_State
                });
                if (resp.Flag)
                {
                    OfAppHelper.RunningIds = resp.FrpMessage!.RunningId?.ToList() ?? new List<int>();
                }
            }
            LauncherModel.PipeRunningState = true;

            await Task.Delay(500);

            if (restart && OfApi.LoginState)
            {
                // 发送账户凭证
                var resp = await OfAppHelper.PipeClient.PushMessageWithRequestAsync(new()
                {
                    Action = Core.Pipe.PipeModel.OfAction.LoginState_Push,
                    AuthMessage = new()
                    {
                        Authorization = OfApi.Authorization,
                        UserSession = OfApi.Session,
                        UserDataModel = OfApi.UserInfoDataModel
                    }
                });
                if (!resp.Flag) { OfApi.ClearAccount(); }
            }

        }
        /// <summary>
        /// Pipe Service
        /// </summary>
        private void ServerPipeWorker()
        {
            OfAppHelper.PipeServer.Start(true);
            OfAppHelper.PipeServer.RequestFunction = (request, model) =>
            {
                App.Current.Dispatcher.Invoke(async () =>
                {
                    switch (request)
                    {
                        // 服务器关闭时 自动发出 Event
                        case Core.Pipe.PipeModel.OfAction.Server_Closed:
                            {
                                LauncherModel.PipeRunningState = false;
                                if (OfSettings.Instance.WorkMode == WorkMode.DeamonService)
                                {
                                    Utils.CheckService();
                                    ClientPipeWorker(true);
                                }
                            }
                            break;
                        case Core.Pipe.PipeModel.OfAction.Frpc_Closed:
                            {
                                var resp = await OfAppHelper.PipeClient.PushMessageAsync(new()
                                {
                                    Action = Core.Pipe.PipeModel.OfAction.Get_State
                                });
                                if (resp.Flag)
                                {
                                    OfAppHelper.RunningIds = resp.FrpMessage!.RunningId?.ToList() ?? new List<int>();
                                }
                                (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.Navigate(typeof(Views.Tunnels));
                            }break;
                        case Core.Pipe.PipeModel.OfAction.Push_AppNotifiy:
                            {
                                if (model.ToastContent?.IsSuccessful == true)
                                {
                                    OfAppHelper.TaskbarIcon.ShowBalloonTip($"隧道 {model.ToastContent?.UserTunnel?.TunnelName} 启动成功!", $"使用 {model.ToastContent?.UserTunnel?.ConnectAddress} 来连接到你的隧道。", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                                }
                                else
                                {
                                    OfAppHelper.TaskbarIcon.ShowBalloonTip($"隧道 {model.ToastContent?.UserTunnel?.TunnelName} 启动失败!", model.ToastContent?.Data, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                                }
                            }
                            break;
                    }
                });
                return new();
            };
        }

        protected override void OnActivated(EventArgs e)
        {
            Visibility = Visibility.Visible;
            base.OnActivated(e);
        }
    }
}
