using Newtonsoft.Json;
using OpenFrp.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenFrp.Core.ModelHelper;

namespace OpenFrp.Core.Pipe
{
    namespace PipeModel
    {
        public enum OfAction : int
        {
            /// <summary>
            /// 获取状态。
            /// </summary>
            Get_State = 0,
            /// <summary>
            /// 服务器已关闭。
            /// </summary>
            Server_Closed = 1,
            /// <summary>
            /// 让服务器关闭。
            /// </summary>
            Close_Server = 2,
            /// <summary>
            /// 推送登录状态
            /// </summary>
            LoginState_Push = 3,
            /// <summary>
            /// 登出。
            /// </summary>
            LoginState_Logout = 4,
            /// <summary>
            /// 开启
            /// </summary>
            Start_Frpc = 5,
            /// <summary>
            /// 关闭
            /// </summary>
            Close_Frpc = 6,
            /// <summary>
            /// FRPC 主动关闭
            /// </summary>
            Frpc_Closed = 7,
        }
        public class BaseModel : MessagePraser<BaseModel>
        {
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

            [JsonProperty("frp")]
            public FrpModel? FrpMessage { get; set; }

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
            /// <summary>
            /// 服务端 反馈 - 是否成功
            /// </summary>
            [JsonProperty("flag")]
            internal new bool Flag { get; set; }
        }
        public class ResponseModel : BaseModel
        {

        }
    }
}
