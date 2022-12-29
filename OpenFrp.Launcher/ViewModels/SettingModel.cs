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


        private bool _hasDialog { get; set; }

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
                if (!_hasDialog)
                {
                    _hasDialog = true;
                    // 弹出登录窗口。
                    var dialog = new Controls.LoginDialog();
                    await dialog.ShowAsync();
                    // 请求用户个人信息已移动到内部逻辑。
                }
                _hasDialog = false;
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
                Message = "关闭。"
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
