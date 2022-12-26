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
using OpenFrp.Core.Api;

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
            get => OfAppHelper.UserInfoModel;
            set => OfAppHelper.UserInfoModel = value;
        }

        
        public bool PipeRunningState
        {
            get => OfAppHelper.LauncherViewModel?.PipeRunningState ?? false;
        }


        private bool _hasDialog { get; set; }

        private Flyout? _flyout { get; set; }

        [RelayCommand]
        async void Logout()
        {
            await Task.Yield();
            if (true)
            {
                _flyout?.Hide();
                OfApi.ClearAccount();
                LoginState = false;
                UserInfoData = new();
            }
        }

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
    }
}
