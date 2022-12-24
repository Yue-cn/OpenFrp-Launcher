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
    }
}
