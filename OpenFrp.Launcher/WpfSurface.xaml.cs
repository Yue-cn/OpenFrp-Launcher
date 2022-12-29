using Newtonsoft.Json;
using OpenFrp.Core;
using OpenFrp.Core.Api;
using OpenFrp.Core.App;
using OpenFrp.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


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

            System.IO.Directory.CreateDirectory(Utils.AppTempleFilesPath);
            System.IO.Directory.CreateDirectory(Path.Combine(Utils.AppTempleFilesPath,"static"));

            await OfSettings.ReadConfig();

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
                        _ => null
                    };
                }
                if (page is not null)
                OfApp_RootFrame.Navigate(page);
            };
            OfApp_RootFrame.Navigating += (s, e) => OfApp_RootFrame.RemoveBackEntry();
            // Defualt Pipe Server 
            // 服务端 单独发给 客户端，不需要客户端先发送请求。
            ServerPipeWorker();
            ClientPipeWorker();
            // 启动器 自动登录逻辑
            if (OfSettings.Instance.Account.HasAccount)
            {
                if (OfSettings.Instance.WorkMode is WorkMode.DeamonService)
                {
                    // 服务模式
                    var result = await OfAppHelper.PipeClient.PushMessageWithRequestAsync(new()
                    {
                        Action = Core.Pipe.PipeModel.OfAction.Get_State
                    });
                    if (result.Flag)
                    {

                        if (!string.IsNullOrEmpty(result.AuthMessage?.Authorization) &&
                            result.AuthMessage?.UserDataModel is not null)
                        {
                            OfApi.UserInfoDataModel = result.AuthMessage?.UserDataModel!;
                            OfAppHelper.LauncherViewModel!.PipeRunningState = true;
                            OfAppHelper.SettingViewModel.LoginState = true;
                        }
                    }
                }
                else
                {
                    //await Task.Delay(1000);
                    
                    var result = await OfAppHelper.LoginAndUserInfo(OfSettings.Instance.Account.User, OfSettings.Instance.Account.Password);
                    if (result.Flag)
                    {

                    }
                }
            }

        }

        private async void ClientPipeWorker()
        {
            // 抛弃旧版 Process 检测机制 仅连接失败（卡住）的时候 Kill 后开启。
            if (OfSettings.Instance.WorkMode == WorkMode.DeamonProcess)
            {
                try
                {
                    Process? process = Process.GetProcessesByName("OpenFrp.Core").FirstOrDefault();
                    if (process is null)
                    {
                        Process.Start(new ProcessStartInfo(Utils.CorePath, "--ws")
                        {
                            CreateNoWindow = false,
                            UseShellExecute = false
                        });
                    }
                }
                catch { }
                
            }
            else
            {
                Utils.CheckService();
            }

            await OfAppHelper.PipeClient.Start();
            
            if (OfSettings.Instance.WorkMode is WorkMode.DeamonProcess)
            {
                await OfAppHelper.PipeClient.PushMessageAsync(new()
                {
                    Action = Core.Pipe.PipeModel.OfAction.Get_State
                });
            }
            LauncherModel.PipeRunningState = true;

        }

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
                                ClientPipeWorker();
                            }
                            break;
                    }
                });
                return new();
            };
        }
        


    }
}
