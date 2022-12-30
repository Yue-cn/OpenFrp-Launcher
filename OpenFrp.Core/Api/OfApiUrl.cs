using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Core.Api
{
    public class OfApiUrl
    {
#if !DEBUG
        /// <summary>
        /// 测试时 内网专用 API 当然可以改自己的
        /// </summary>
        public const string BaseUrl = @"http://192.168.31.74";
        /// <summary>
        /// 启动器用的Url
        /// </summary>
        public const string LauncherApiUrl = @"http://192.168.31.74";
#else
        /// <summary>
        /// 正式使用时，切回官方 Api
        /// </summary>
        public const string BaseUrl = @"https://of-dev-api.bfsea.xyz";
        /// <summary>
        /// 启动器用的Url
        /// </summary>
        public const string LauncherApiUrl = @"https://yueapi.zyghit.cn";
#endif


        /// <summary>
        /// OpenFrp - 登录
        /// <para> 绑定模型: <see cref="OfApiModel.Response.BaseModel"/></para>
        /// </summary>
        public const string Login = $"{BaseUrl}/user/login";

        /// <summary>
        /// OpenFrp - 用户信息
        /// <para> 绑定模型: <see cref="OfApiModel.Response.UserInfoModel"/> </para>
        /// </summary>
        public const string UserInfo = $"{BaseUrl}/frp/api/getUserInfo";

        /// <summary>
        /// OpenFrp - 签到
        /// <para> 绑定模型: <see cref="OfApiModel.Response.BaseModel"/></para>
        /// </summary>
        public const string UserSignin = $"{BaseUrl}/frp/api/userSign";
        /// <summary>
        /// OpenFrp - 启动器信息
        /// <para> 绑定模型: <see cref="OfApiModel.Response.LauncherPreview"/></para>
        /// </summary>
        public const string LauncherInfo = $"{LauncherApiUrl}/launcher/info";
        /// <summary>
        /// OpenFrp - 启动器公告
        /// <para> 绑定模型: <see cref="OfApiModel.Response.BaseModel"/></para>
        /// </summary>
        public const string LauncherBroadCast = $"{LauncherApiUrl}/launcher/broadCast";
    }
}
