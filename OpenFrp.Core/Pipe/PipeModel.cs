using Newtonsoft.Json;
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
            /// 客户端发送给服务端(Dev)
            /// </summary>
            CLIENT_TO_SERVER,
            /// <summary>
            /// 服务端发送给客户端(Dev)
            /// </summary>
            SERVER_TO_CLIENT
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
