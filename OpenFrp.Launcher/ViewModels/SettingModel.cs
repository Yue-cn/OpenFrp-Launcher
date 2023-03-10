using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrp.Core;
using OpenFrp.Core.Api;
using OpenFrp.Core.App;
using Windows.ApplicationModel.VoiceCommands;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class SettingModel : ObservableObject
    {
        public SettingModel()
        {
            if (OfAppHelper.LauncherViewModel is not null)
            {
                OfAppHelper.LauncherViewModel.PropertyChanged += (sender, e) =>
                {
                    OnPropertyChanged("PipeRunningState");
                };
            }
        }

        public int ApplicationTheme
        {
            get => (int)OpenFrp.Core.App.OfSettings.Instance.Theme;
            set => OpenFrp.Core.App.OfSettings.Instance.Theme = (ElementTheme)value;
        }
        public int ApplicationWorkMode
        {
            get => (int)OpenFrp.Core.App.OfSettings.Instance.WorkMode;
            set => OpenFrp.Core.App.OfSettings.Instance.WorkMode = (OpenFrp.Core.App.WorkMode)value;
        }
        public bool BypassProxy
        {
            get => OpenFrp.Core.App.OfSettings.Instance.BypassProxy;
            set => OpenFrp.Core.App.OfSettings.Instance.BypassProxy = value;
        }
        public bool AutoLaunchUp
        {
            get => OpenFrp.Core.App.OfSettings.Instance.AutoLaunchUp;
            set => OpenFrp.Core.App.OfSettings.Instance.AutoLaunchUp = value;
        }
        public int ToastMode
        {
            get => (int)OpenFrp.Core.App.OfSettings.Instance.NotifiyMode;
            set => OpenFrp.Core.App.OfSettings.Instance.NotifiyMode = (NotifiyMode)value;
        }
        public OpenFrp.Core.App.OfSettings.ConsoleModel ConsoleSettng
        {
            get => OpenFrp.Core.App.OfSettings.Instance.Console;
            set => OpenFrp.Core.App.OfSettings.Instance.Console = value;
        }

        public bool isSupportedToast { get; } = Utils.isSupportToast;

        /// <summary>
        /// 登录状态 (实际使用请改为<see cref="LoginState"/>)
        /// </summary>
        [ObservableProperty]
        public bool _LoginState;
        /// <summary>
        /// 用户信息 (Model Refer Form: <see cref="OfAppHelper.UserInfoModel"/>)
        /// </summary>
        public OpenFrp.Core.Api.OfApiModel.Response.UserInfoModel.UserInfoDataModel UserInfoData
        {
            get => OfApi.UserInfoDataModel!;
            set => OfApi.UserInfoDataModel = value;
        }

        
        public bool PipeRunningState
        {
            get => OfAppHelper.LauncherViewModel?.PipeRunningState ?? false;
        }

        public string AppVersion => Utils.ApplicationVersions;



        private Flyout? _flyout { get; set; }

        /// <summary>
        /// 登出
        /// </summary>
        [RelayCommand]
        async void Logout()
        {
            var resp = await OfAppHelper.PipeClient.PushMessageAsync(new()
            {
                Action = Core.Pipe.PipeModel.OfAction.LoginState_Logout,
            });
            if (resp.Flag)
            {
                _flyout?.Hide();
                OfApi.ClearAccount();
                LoginState = false;
                UserInfoData = new();
                OfSettings.Instance.Account = new();
                
            }
        }
        /// <summary>
        /// 登录
        /// </summary>
        [RelayCommand]
        async void Login(Button sender)
        {
            if (!LoginState)
            {
                if (!OfAppHelper.HasDialog)
                {
                    OfAppHelper.HasDialog = true;
                    // 弹出登录窗口。
                    var dialog = new Controls.LoginDialog();
                    await dialog.ShowAsync();
                    // 请求用户个人信息已移动到内部逻辑。
                    OfAppHelper.HasDialog = false;
                }
            }
            else
            {
                _flyout = new Flyout()
                {
                    Placement = ModernWpf.Controls.Primitives.FlyoutPlacementMode.LeftEdgeAlignedTop,
                    Content = new Controls.AccountFlyout(),
                };
                _flyout.ShowAt(sender);
            }
        }
        /// <summary>
        /// 安装 / 卸载 服务
        /// </summary>
        [RelayCommand]
        async void ActionServiceMode(Button sender)
        {
            if ((await OfAppHelper.PipeClient.PushMessageAsync(new()
            {
                Action = Core.Pipe.PipeModel.OfAction.Close_Server,
            })).Action == Core.Pipe.PipeModel.OfAction.Server_Closed)
            {
                if (OfAppHelper.LauncherViewModel is not null)
                OfAppHelper.LauncherViewModel.PipeRunningState = false;
            }
            try
            {
                if (ApplicationWorkMode == 0)
                {
                    Process.Start(new ProcessStartInfo(Utils.CorePath, "--install")
                    {
                        CreateNoWindow = true,
                        Verb = "runas"
                    });
                }
                else
                {
                    Process.Start(new ProcessStartInfo(Utils.CorePath, "--uninstall")
                    {
                        CreateNoWindow = true,
                        Verb = "runas",
                    });
                }
            }
            catch { }
            Application.Current.Shutdown();
        }
    }
}
