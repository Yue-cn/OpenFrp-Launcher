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
        ///  <para>Body Request: <see cref="OfApiModel.Request.LoginData"/></para>
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

        public const string CheckFrpcUpdate = $"{LauncherApiUrl}/launcher/update/frpc";

        public const string CheckLauncherUpdate = $"{LauncherApiUrl}/launcher/update/launcher";
        /// <summary>
        /// 用户隧道
        /// <para> 绑定模型: <see cref="OfApiModel.Response.UserProxiesModel"/></para>
        /// </summary>
        public const string UserProxies = $"{BaseUrl}/frp/api/getUserProxies";
        /// <summary>
        /// 列出节点
        /// <para> 绑定模型: <see cref="OfApiModel.Response.NodesModel"/></para>
        /// </summary>
        public const string NodeList = $"{BaseUrl}/frp/api/getNodeList";
        /// <summary>
        /// 移除隧道
        /// <para>Body Request: <see cref="OfApiModel.Request.RemoveTunnelData"/></para>
        /// <para> 绑定模型: <see cref="OfApiModel.Response.BaseModel"/></para>
        /// </summary>
        public const string RemoveTunnel = $"{BaseUrl}/frp/api/removeProxy";
        /// <summary>
        /// 创建隧道:
        /// <para>Body Request: <see cref="OfApiModel.Request.CreateTunnelData"/></para>
        /// <para> 绑定模型: <see cref="OfApiModel.Response.BaseModel"/></para>
        /// </summary>
        public const string CreateTunnel = $"{BaseUrl}/frp/api/newProxy";
        /// <summary>
        /// 编辑隧道:
        /// <para>Body Request: <see cref="OfApiModel.Request.CreateTunnelData"/></para>
        /// <para> 绑定模型: <see cref="OfApiModel.Response.BaseModel"/></para>
        /// </summary>
        public const string EditTunnel = $"{BaseUrl}/frp/api/editProxy";
    }
}
