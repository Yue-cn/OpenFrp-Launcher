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
        public int ApplicationTheme
        {
            get => (int)OpenFrp.Core.App.OfSettings.Instance.Theme;
            set => OpenFrp.Core.App.OfSettings.Instance.Theme = (ElementTheme)value;
        }


        [ObservableProperty]
        public bool _LoginState;

        public OpenFrp.Core.Api.OfApiModel.Response.UserInfoModel.UserInfoDataModel UserInfoData
        {
            get => OfAppHelper.UserInfoModel;
            set => OfAppHelper.UserInfoModel = value;
        }

        private bool _hasDialog { get; set; }

        private Flyout? _flyout { get; set; }

        [RelayCommand]
        void Logout()
        {
            _flyout?.Hide();
            OfApi.ClearAccount();
            LoginState = false;
            UserInfoData = new();
            
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
                    LoginState = OfApi.LoginState;
                    var info = await OfApi.GetUserInfo();
                    if (info.Flag)
                    {
                        UserInfoData = info.Data;
                    }
                    
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
