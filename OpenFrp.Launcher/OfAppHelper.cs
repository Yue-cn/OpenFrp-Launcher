using OpenFrp.Core;
using OpenFrp.Core.Api;
using OpenFrp.Core.Api.OfApiModel;
using OpenFrp.Core.App;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Launcher
{
    public class OfAppHelper
    {
        /// <summary>
        /// 启动器 - 主模型
        /// </summary>
        public static ViewModels.LauncherModel LauncherViewModel
        {
            get => (ViewModels.LauncherModel)App.Current?.MainWindow.DataContext!;
            set => App.Current.MainWindow.DataContext = value;
        }
        /// <summary>
        /// 设置页面 - 模型
        /// </summary>
        public static ViewModels.SettingModel SettingViewModel { get; set; } = new();

        public static List<int> RunningIds { get; set; } = new();
        /// <summary>
        /// 管道 - 客户端
        /// </summary>
        public static OpenFrp.Core.Pipe.PipeClient PipeClient { get; set; } = new();
        /// <summary>
        /// 管道 - PUSH
        /// </summary>
        public static OpenFrp.Core.Pipe.PipeServer PipeServer { get; set; } = new();

        public static Hardcodet.Wpf.TaskbarNotification.TaskbarIcon TaskbarIcon = new()
        {
            Icon = new System.Drawing.Icon(Path.Combine(Utils.ApplicationPath,"favicon.ico")),
            ToolTipText = "OpenFrp Launcher正在运行，单击显示窗口。"
        };

        public static void RestartProcess()
        {
            Utils.StopService();
            

            LauncherViewModel.PipeRunningState = false;
            (App.Current.MainWindow as WpfSurface)?.ClientPipeWorker(true);
        }

        internal static bool isLoading { get; set; }
        /// <summary>
        /// 登录且获得用户个人信息
        /// </summary>
        internal static async ValueTask<Response.BaseModel> LoginAndUserInfo(string? user,string? password,CancellationToken? _token = null)
        {
            isLoading = true;
            // 登录 1
            var lo_res = await OfApi.Login(user, password);
            if (lo_res.Flag)
            {
                if (_token is null || _token?.IsCancellationRequested == false)
                {
                    // 获取用户信息
                    var ui_res = await OfApi.GetUserInfo();
                    if (ui_res.Flag)
                    {
                        if (_token is null || _token?.IsCancellationRequested == false)
                        {
                            // 推送用户星系
                            var pipe_resp = await OfAppHelper.PipeClient.PushMessageWithRequestAsync(new()
                            {
                                Action = Core.Pipe.PipeModel.OfAction.LoginState_Push,
                                AuthMessage = new()
                                {
                                    Authorization = OfApi.Authorization,
                                    UserSession = OfApi.Session,
                                    UserDataModel = ui_res.Data
                                }
                            });
                            if (pipe_resp.Flag)
                            {
                                if (_token is null || _token?.IsCancellationRequested == false)
                                {
                                    isLoading = false;
                                    OfApi.UserInfoDataModel = ui_res.Data;
                                    SettingViewModel.LoginState = OfApi.LoginState;
                                    return new(null, true, "");
                                }
                            }
                            else 
                            {
                                isLoading = false;
                                return new($"推送给服务端失败: {pipe_resp.Message}"); 
                            }
                        }
                    }
                    else 
                    {
                        isLoading = false;
                        return new($"获取用户信息失败: {ui_res.Message}");
                    } 
                }
                isLoading = false;
                return new("用户已取消操作。");
            }
            isLoading = false;
            return new($"{lo_res.Message}");

        }

        internal static async ValueTask<bool> RequestLogin()
        {
            // 有账户 走下面逻辑
            if (OfSettings.Instance.Account.HasAccount)
            {
                if (OfSettings.Instance.WorkMode is WorkMode.DeamonService)
                {
                    // 服务模式
                    var result = await PipeClient.PushMessageWithRequestAsync(new()
                    {
                        Action = Core.Pipe.PipeModel.OfAction.Get_State
                    });
                    // 从服务器中获取配置
                    if (result.Flag)
                    {
                        if (!string.IsNullOrEmpty(result.AuthMessage?.Authorization) &&
                            result.AuthMessage?.UserDataModel is not null)
                        {
                            OfApi.UserInfoDataModel = result.AuthMessage?.UserDataModel!;
                            LauncherViewModel!.PipeRunningState = true;
                            SettingViewModel.LoginState = true;
                            OfApi.Authorization = result.AuthMessage?.Authorization;
                            OfApi.Session = result.AuthMessage?.UserSession;
                            RunningIds = result.FrpMessage?.RunningId?.ToList() ?? new List<int>();
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    // 守护进程
                    var result = await LoginAndUserInfo(OfSettings.Instance.Account.User, OfSettings.Instance.Account.Password);
                    if (result.Flag)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }
    }
}
