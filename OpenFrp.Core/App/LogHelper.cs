using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Core.App
{
    internal class LogHelper
    {
        public static Dictionary<string, List<LogsModel>> LogsList { get; set; } = new();


    }

    public class LogsModel
    {
        /// <summary>
        /// 等级
        /// </summary>
        [JsonProperty("level")]
        public LogsLevel Level { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [JsonProperty("content")]
        public string? Content { get; set; }
    }

    public enum LogsLevel
    {
        Info,
        Error,
        Warning,
    }
}
