using OpenFrp.Core.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.VoiceCommands;
using OpenFrp.Core.Pipe.PipeModel;
using Newtonsoft.Json;

namespace OpenFrp.Core.App
{
    public class ConsoleHelper
    {
        public static Dictionary<int, ConsoleWrapper> ConsoleWrappers { get; set; } = new();

        public static ResponseModel Launch(Core.Api.OfApiModel.Response.UserTunnelModel.UserTunnel tunnel)
        {
            if (!ConsoleWrappers.ContainsKey(tunnel.TunnelId))
            {
                if (!File.Exists(Utils.Frpc) || !OfApi.LoginState)
                {
                    Utils.Log($"早不到文件,{Utils.Frpc}");
                    return new(OfAction.Start_Frpc,false,"找不到 FRPC 文件");
                }
                try
                {


                    var frpc = new Process()
                    {
                        StartInfo = new()
                        {
                            FileName = Utils.Frpc,
                            Arguments = $"-n -u {OfApi.UserInfoDataModel?.UserToken} -p {tunnel.TunnelId}",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            StandardErrorEncoding = Encoding.UTF8,
                            StandardOutputEncoding = Encoding.UTF8,
                        },
                        EnableRaisingEvents = true
                    };

                    ConsoleWrappers.Add(tunnel.TunnelId, new()
                    {
                        UserTunnelModel = tunnel,
                        Process = frpc
                    });

                    frpc.OutputDataReceived += (sender, e) => Output(e.Data, TraceLevel.Info, tunnel.TunnelId);
                    frpc.ErrorDataReceived+= (sender, e) => Output(e.Data, TraceLevel.Error, tunnel.TunnelId);

                    if (!frpc.Start()) return new(OfAction.Start_Frpc, false, "FRPC 启动失败");

                    frpc.BeginErrorReadLine();
                    frpc.BeginOutputReadLine();

                    frpc.Exited += (sender, e) =>
                    {
                        if (ConsoleWrappers.ContainsKey(tunnel.TunnelId))
                        {
                            ConsoleWrappers.Remove(tunnel.TunnelId);
                        }
                            
                        Launch(tunnel);
                    };

                    return new(OfAction.Start_Frpc, true, "启动成功");
                }
                catch (Exception ex)
                {
                    return new(OfAction.Start_Frpc, false, ex.ToString());
                }

            }
            return new(OfAction.Start_Frpc, false, "对应隧道已启动。");
        }

        public static bool Stop(int id)
        {
            if (ConsoleWrappers.ContainsKey(id))
            {
                if (ConsoleWrappers[id].Process is Process process)
                {
                    if (!process.HasExited)
                    {
                        process.EnableRaisingEvents = false;
                        process.Kill();
                    }
                    
                }
                ConsoleWrappers.Remove(id);
                
            }
            return true;
        }


        static void Output(string? data,TraceLevel level,int tunnelId)
        {
            if (data is not null)
            {
                if (ConsoleWrappers.ContainsKey(tunnelId))
                {
                    ConsoleWrappers[tunnelId].Append(data, level);   
                }   
            }
            
        }
    }
    /// <summary>
    /// 操控台应用 - 实例
    /// </summary>
    public class ConsoleWrapper
    {
        [JsonProperty("tunnel")]
        public Core.Api.OfApiModel.Response.UserTunnelModel.UserTunnel? UserTunnelModel { get; set; }

        public List<LogContent> Content { get; set; } = new();

        internal void Append(string data,TraceLevel level)
        {
            Content.Add(new(data, level));
            LogHelper.AllLogs.Add(new(data, level));
        }

        [JsonIgnore]
        public Process? Process { get; set; }
    }
}
