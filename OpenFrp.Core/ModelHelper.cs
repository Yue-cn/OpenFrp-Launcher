using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Core
{
    public class ModelHelper
    {
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
            /// <summary>
            /// 返回该对象的 <see cref="StringContent"/>
            /// </summary>
            /// <returns></returns>
            public StringContent ToStringContent()
            {
                return new(ToString(), Encoding.UTF8, "application/json");
            }
        }
    }
}
