using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Core.Api
{
    public class OfApiUrl
    {
#if DEBUG
        /// <summary>
        /// 测试时 内网专用 API 当然可以改自己的
        /// </summary>
        public const string BaseUrl = @"http://192.168.31.72:5110/";
#else
        /// <summary>
        /// 正式使用时，切回官方 Api
        /// </summary>
        public const string BaseUrl = @"https://of-dev-api.bfsea.xyz";
#endif
        /// <summary>
        /// OpenFrp - 登录
        /// <para> 绑定模型: <see cref="OfApiModel.BaseModel"/></para>
        /// </summary>
        public const string Login = $"{BaseUrl}/user/login";
    }
}
