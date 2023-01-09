using Newtonsoft.Json;
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
using Windows.UI.WebUI;
using Microsoft.Toolkit.Uwp.Notifications;
using OpenFrp.Launcher.Views;
using System.Windows.Markup;

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
        private LauncherModel LauncherModel
        {
            get => (LauncherModel)DataContext;
            set => DataContext = value;
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);



            Directory.CreateDirectory(Utils.AppTempleFilesPath);
            Directory.CreateDirectory(Path.Combine(Utils.AppTempleFilesPath, "static"));

            //await OfSettings.ReadConfig();



            OfAppHelper.TaskbarIcon.ContextMenu = new()
            {
                Items =
                {
                    CreateItemWithAction("退出启动器",
                        App.Current.Shutdown,
                        new FontIcon(){Glyph = "\ue89f"}),
                    CreateItemWithAction("彻底退出", async () =>
                    {
                        await OfAppHelper.PipeClient.PushMessageAsync(new()
                        {
                            Action = Core.Pipe.PipeModel.OfAction.Close_Server,
                        });
                        App.Current.Shutdown();
                    },new FontIcon(){Glyph = "\ue8bb"})
                }
            };
            OfAppHelper.TaskbarIcon.TrayMouseDoubleClick += (sender, args) =>
            {
                Visibility = Visibility.Visible;
                Activate();
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
            // 启动器更新逻辑
            CheckUpdate();
            // 启动器 自动登录逻辑
            await OfAppHelper.RequestLogin();
            // 自动启动 FRPC
            AutoStartup();

            


        }
        
        private async void AutoStartup()
        {
            if (OfSettings.Instance.AutoRunTunnel.Count == 0 || OfSettings.Instance.WorkMode == WorkMode.DeamonService) return;
            var resp = await OfApi.GetUserProxies();
            if (resp.Flag)
            {
                foreach (var tunnel in resp.Data.List)
                {
                    OfSettings.Instance.AutoRunTunnel.ForEach(async (tunnelId) =>
                    {
                        if (tunnelId == tunnel.TunnelId)
                        {
                            if (!OfAppHelper.RunningIds.Contains(tunnelId))
                                OfAppHelper.RunningIds.Add(tunnelId);
                            var resp = await OfAppHelper.PipeClient.PushMessageWithRequestAsync(new()
                            {
                                Action = Core.Pipe.PipeModel.OfAction.Start_Frpc,
                                FrpMessage = new()
                                {
                                    Tunnel = tunnel
                                }
                            });
                            if (!resp.Flag)
                            {
                                // $"由于以下原因,FRPC无法开机自启: {resp.Message}"
                                await OfAppHelper.PipeClient.PushMessageWithRequestAsync(new()
                                {
                                    Action = Core.Pipe.PipeModel.OfAction.Push_Logs,
                                    PushLog = $"由于以下原因,FRPC无法开机自启: {resp.Message}"
                                });
                            }
                            return;
                        }
                    });
                    
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
                    var dialog = new ContentDialog()
                    {
                        DefaultButton = ContentDialogButton.Primary,
                        Title = "启动器有更新啦",
                        Content = new TextBlock(new Run(update.Content))
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
                        }
   
                        dialog.Content = "下载失败。";
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

                }
                else
                {
                    LauncherModel.IsFrpchasUpdate = true;
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
                await OfAppHelper.PipeClient.PushMessageWithRequestAsync(new()
                {
                    Action = Core.Pipe.PipeModel.OfAction.LoginState_Push,
                    AuthMessage = new()
                    {
                        Authorization = OfApi.Authorization,
                        UserSession = OfApi.Session,
                        UserDataModel = OfApi.UserInfoDataModel
                    }
                });
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
                                await Task.Delay(1500);
                                ClientPipeWorker(true);
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
        


    }
}
