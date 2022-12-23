using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Core.Api
{
    namespace OfApiModel
    {
        public class Request
        {

        }
        public class Response
        {
            public class BaseModel : MessagePraser<BaseModel>
            {



                /// <summary>
                /// API 返回的数据内容
                /// </summary>
                [JsonProperty("data")]
                public string? Data { get; set; }

                /// <summary>
                /// API 返回的状态
                /// </summary>
                [JsonProperty("flag")]
                public bool Flag { get; set; }

                /// <summary>
                /// API 返回的消息
                /// </summary>
                [JsonProperty("msg")]
                public string Message { get; set; } = string.Empty;
            }
        }
        public class MessagePraser<T>
        {

            /// <summary>
            /// 从文本解析 JSON。
            /// </summary>
            /// <param name="str">JSON String</param>
            /// <returns><typeparamref name="T"/></returns>
            public static T? PraseFrom(string str)
            {
                return JsonConvert.DeserializeObject<T>(str);
            }
            /// <summary>
            /// 返回该对象的 JSON 文本。
            /// </summary>
            /// <returns>JSON String</returns>
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }


    }
}
