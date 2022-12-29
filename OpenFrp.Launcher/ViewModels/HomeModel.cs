﻿using CommunityToolkit.Mvvm.ComponentModel;
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
                public object IconElement { get; set; } = new SymbolIcon(Symbol.Play);

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
            if (!_hasDialog)
            {
                _hasDialog = true;
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
                    await Task.Delay(500);
                    loader.Content = resp.Message;
                    loader.ShowContent();
                    dialog.IsPrimaryButtonEnabled = true;
                };
                await dialog.ShowAsync();
                page.Of_UserInfo_Loader.ShowLoader();
                await Task.Delay(750);
                page.RefreshUserInfo();
            }
            _hasDialog = false;

        }
        /// <summary>
        /// 打开官网
        /// </summary>
        [RelayCommand]
        void OpeninWeb() => Process.Start(@"https://www.openfrp.net");
    }
}
