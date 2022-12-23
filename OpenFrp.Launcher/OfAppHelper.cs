using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Launcher
{
    public class OfAppHelper
    {
        public static ViewModels.SettingModel SettingModel { get; set; } = new();

        public static OpenFrp.Core.Api.OfApiModel.Response.UserInfoModel.UserInfoDataModel UserInfoModel { get; set; } = new();
    }
}
