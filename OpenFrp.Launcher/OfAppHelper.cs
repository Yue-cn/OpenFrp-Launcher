﻿using OpenFrp.Core.Api;
using OpenFrp.Core.Api.OfApiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Launcher
{
    public class OfAppHelper
    {
        public static ViewModels.LauncherModel? LauncherViewModel
        {
            get => (ViewModels.LauncherModel?)App.Current?.MainWindow.DataContext;
            set => App.Current.MainWindow.DataContext = value;
        }
        /// <summary>
        /// 设置页面 - 模型
        /// </summary>
        public static ViewModels.SettingModel SettingViewModel { get; set; } = new();
        /// <summary>
        /// 用户信息
        /// </summary>
        public static OpenFrp.Core.Api.OfApiModel.Response.UserInfoModel.UserInfoDataModel UserInfoModel { get; set; } = new();

        /// <summary>
        /// 管道 - 客户端
        /// </summary>
        public static OpenFrp.Core.Pipe.PipeClient PipeClient { get; set; } = new();
        /// <summary>
        /// 管道 - PUSH
        /// </summary>
        public static OpenFrp.Core.Pipe.PipeServer PipeServer { get; set; } = new();
        /// <summary>
        /// 登录且获得用户个人信息
        /// </summary>
        internal static async ValueTask<Response.BaseModel> LoginAndUserInfo(string user,string password,CancellationToken? _token = null)
        {
            var lo_res = await OfApi.Login(user, password);
            if (lo_res.Flag)
            {
                if (_token?.IsCancellationRequested == false)
                {
                    var ui_res = await OfApi.GetUserInfo();
                    if (ui_res.Flag)
                    {
                        UserInfoModel = ui_res.Data;
                        SettingViewModel.LoginState = OfApi.LoginState;
                        return new(null,true, "");
                    }
                    return new($"获取用户信息失败: {ui_res.Message}");
                }
                else
                {
                    return new("用户已取消操作。");
                }

            }
            return new($"{lo_res.Message}");

        }
    }
}
