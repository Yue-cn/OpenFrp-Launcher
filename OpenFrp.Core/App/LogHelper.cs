using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace OpenFrp.Core.App
{
    public class LogHelper
    {
        public static List<LogContent> AllLogs { get; set; } = new();

        public static ConsoleWrapper[] GetAllWrapper()
        {
            return new ConsoleWrapper[]
            {
                new()
                {
                    UserTunnelModel = new()
                    {
                        TunnelName = "全部"  
                    },
                    Content = AllLogs
                },
                
            }.Concat(ConsoleHelper.ConsoleWrappers.Values.ToArray()).ToArray();
        }
    }
    public class LogContent
    {
        public LogContent() { }

        public LogContent(string? content,TraceLevel level)
        {
            Content = content;
            Level = level;
        }

        [JsonProperty("content")]
        public string? Content { get; set; }

        [JsonProperty("level")]
        public TraceLevel Level { get; set; }
    }

}
