using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using OpenFrp.Core.Api;
using OpenFrp.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenFrp.Core.ModelHelper;

/*
   Author: 越(Yue)
   Github: https://github.com/Yue_cn/
 */
namespace OpenFrp.Core.Pipe
{
    namespace PipeModel
    {
        public enum OfAction : int
        {
            /// <summary>
            /// 获取状态。
            /// </summary>
            Get_State,
            /// <summary>
            /// 服务器已关闭。
            /// </summary>
            Server_Closed,
            /// <summary>
            /// 让服务器关闭。
            /// </summary>
            Close_Server,
            /// <summary>
            /// 推送登录状态
            /// </summary>
            LoginState_Push,
            /// <summary>
            /// 登出。
            /// </summary>
            LoginState_Logout,
            /// <summary>
            /// 开启
            /// </summary>
            Start_Frpc,
            /// <summary>
            /// 关闭
            /// </summary>
            Close_Frpc,
            /// <summary>
            /// FRPC 主动关闭
            /// </summary>
            Frpc_Closed,
            /// <summary>
            /// 获取日志
            /// </summary>
            Get_Logs,
            /// <summary>
            /// 推送日志
            /// </summary>
            Push_Logs,
            /// <summary>
            /// 推送配置
            /// </summary>
            Push_Config,
            /// <summary>
            /// 推送通知 (在服务模式下,通知无法使用，只能借助客户端。)
            /// </summary>
            Push_AppNotifiy
        }
        public class BaseModel : MessagePraser<BaseModel>
        {
            public BaseModel()
            {

            }
            public BaseModel(OfAction action, bool flag, string message)
            {
                Action = action;
                Flag = flag;
                Message = message;
            }



            /// <summary>
            /// 所操作的行为
            /// </summary>
            [JsonProperty("action")]
            public OfAction Action { get; set; }

            /// <summary>
            /// 服务端 反馈 - 是否成功
            /// </summary>
            [JsonProperty("flag")]
            public bool Flag { get; set; }

            /// <summary>
            /// 服务端 反馈 - (错误)信息
            /// </summary>
            [JsonProperty("msg")]
            public string Message { get; set; } = string.Empty;

            /// <summary>
            /// 用户Login模型
            /// </summary>
            [JsonProperty("auth")]
            public AuthModel? AuthMessage { get; set; }
            /// <summary>
            /// FRP信息 (开启 / 关闭 / 查询运行状态)
            /// </summary>
            [JsonProperty("frp")]
            public FrpModel? FrpMessage { get; set; }

            /// <summary>
            /// 配置文件
            /// </summary>
            [JsonProperty("config")]
            public OfSettings? Config { get; set; }

            public class AuthModel
            {
                /// <summary>
                /// 用户信息模型
                /// </summary>
                public Api.OfApiModel.Response.UserInfoModel.UserInfoDataModel? UserDataModel { get; set; }

                /// <summary>
                /// 应用的 AppSession iD
                /// </summary>
                public string? UserSession { get; set; }
                /// <summary>
                /// 应用的 Authorization
                /// </summary>
                public string? Authorization { get; set; }

            }

            public class FrpModel
            {

                [JsonProperty("runningIds")]
                public int[]? RunningId { get; set; }

                [JsonProperty("tunnel")]
                public Api.OfApiModel.Response.UserTunnelModel.UserTunnel? Tunnel { get; set; }
            }


        }
        public class RequestModel : BaseModel
        {

            public RequestModel()
            {

            }

            public RequestModel(OfAction action, bool flag, string message) : base(action, flag, message)
            {

            }
            /// <summary>
            /// 服务端 反馈 - 是否成功
            /// </summary>
            [JsonProperty("flag")]
            internal new bool Flag { get; set; }

            [JsonProperty("pushLogs")]
            public string? PushLog { get; set; }

            [JsonProperty("toast")]
            public ToastContentModel? ToastContent { get; set; }

            public class ToastContentModel
            {
                [JsonProperty("success")]
                public bool IsSuccessful { get; set; }

                [JsonProperty("userTunnel")]
                public Core.Api.OfApiModel.Response.UserTunnelModel.UserTunnel? UserTunnel { get; set; }

                [JsonProperty("data")]
                public string? Data { get; set; }
            }
        }
        public class ResponseModel : BaseModel
        {
            public ResponseModel()
            {

            }

            public ResponseModel(OfAction action, bool flag, string message) : base(action, flag, message)
            {

            }

            [JsonProperty("logs")]
            public LogsModel? Logs { get; set; }


            public class LogsModel
            {
                [JsonProperty("consoleWrapppers")]
                public ConsoleWrapper[]? ConsoleWrappers { get; set; }
            }
        }
    }
}
