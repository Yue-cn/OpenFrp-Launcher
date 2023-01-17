using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrp.Core.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class HomeModel : ObservableObject
    {
        /// <summary>
        /// 用户信息 (Model Refer Form: <see cref="OfAppHelper.UserInfoModel"/>)
        /// </summary>
        public OpenFrp.Core.Api.OfApiModel.Response.UserInfoModel.UserInfoDataModel UserInfoData
        {
            get => OfApi.UserInfoDataModel!;
            set => OfApi.UserInfoDataModel = value;
        }

        [ObservableProperty]
        public bool adaptiveSmall;

        [ObservableProperty]
        public OpenFrp.Core.Api.OfApiModel.Response.LauncherPreview.LauncherPreviewDataModel launcherPreviewData = new();

        [ObservableProperty]
        public ObservableCollection<HomeViewModels.UserInfoViewModel> userInfoViewModels = new();

        public class HomeViewModels
        {
            public class UserInfoViewModel
            {
                public string IconElement { get; set; } = "\xe713";

                public string Title { get; set; } = "Unknown";

                public string Content { get; set; } = "Content";
            }
        }

        private bool _hasDialog { get; set; }

        /// <summary>
        /// 签到
        /// </summary>
        [RelayCommand]
        async void Signin(Views.Home page)
        {
            if (!OfAppHelper.HasDialog)
            {
                OfAppHelper.HasDialog = true;
                var loader = new Controls.ElementLoader()
                {
                    IsLoading = true,
                    ProgressRingSize = 75
                };
                var dialog = new ContentDialog()
                {
                    PrimaryButtonText = "关闭",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = loader,
                    Title = "每日签到",
                    IsPrimaryButtonEnabled = false,
                };
                dialog.Loaded += async (sender, args) =>
                {
                    var resp = await OfApi.UserSignin();
                    var resp1 = await OfApi.GetUserInfo();

                    if (!resp.Flag || !resp1.Flag)
                    {
                        if (resp.Message.Contains("token已过期") || resp1.Message.Contains("token已过期"))
                        {
                            if (!await OfAppHelper.RequestLogin(false))
                            {
                                loader.Content = $"Token失效后重请求失败,请尝试重新登录账户";
                            }
                        }
                    }
                    loader.Content = $"{(string.IsNullOrEmpty(resp.Data) ? resp1.Message : resp.Data)}";
                    if (resp1.Flag) { OfApi.UserInfoDataModel = resp1.Data; }
                    loader.ShowContent();
                    dialog.IsPrimaryButtonEnabled = true;
                };
                
                await dialog.ShowAsync();

                page.Of_Home_UserInfoLoader.ShowLoader();
                await Task.Delay(500);
                page.RefreshUserInfo();
                OfAppHelper.HasDialog = false;
            }
        }
        /// <summary>
        /// 打开官网
        /// </summary>
        [RelayCommand]
        void OpeninWeb() => Process.Start(@"https://www.openfrp.net");
    }
}
