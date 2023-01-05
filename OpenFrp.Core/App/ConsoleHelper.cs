using OpenFrp.Core.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

namespace OpenFrp.Core.App
{
    public class ConsoleHelper
    {
        public static Dictionary<int,string> RunningTunnels { get; set; } = new Dictionary<int,string>();
        public static Dictionary<int,Process> RunningTunnelsWithProcess { get; set; } = new Dictionary<int,Process>();



        public static Pipe.PipeModel.ResponseModel Launch(Api.OfApiModel.Response.UserTunnelModel.UserTunnel tunnel)
        {
            if (!File.Exists(Utils.Frpc))
            {
                return new()
                {
                    Action = Pipe.PipeModel.OfAction.Start_Frpc,
                    Message = "找不到FRPC",
                };
            }
            else if (!OfApi.LoginState)
            {
                return new()
                {
                    Action = Pipe.PipeModel.OfAction.Start_Frpc,
                    Message = "您尚未登录。",
                };
            }
            else
            {
                try
                {
                    var process = new Process()
                    {
                        StartInfo = new ProcessStartInfo(Utils.Frpc, $"-u {OfApi.UserInfoDataModel?.UserToken} -n -p {tunnel.TunnelId}")
                        {
                            CreateNoWindow = false,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            WorkingDirectory = Path.Combine(Utils.ApplicationPath, "frpc"),
                            StandardErrorEncoding = Encoding.UTF8,
                            StandardOutputEncoding = Encoding.UTF8,
                        },
                        EnableRaisingEvents = true
                    };

                    process.OutputDataReceived += (sender, args) => Output(args.Data, TraceLevel.Info);
                    process.ErrorDataReceived += (sender, args) => Output(args.Data, TraceLevel.Error);
                    process.Exited += (sender, args) => Launch(tunnel);

                    if (process.Start())
                    {
                        process.BeginErrorReadLine();
                        process.BeginOutputReadLine();
                        if (!RunningTunnels.ContainsKey(tunnel.TunnelId) &&
                            !RunningTunnelsWithProcess.ContainsKey(tunnel.TunnelId))
                        {
                            RunningTunnelsWithProcess.Add(tunnel.TunnelId, process);
                            RunningTunnels.Add(tunnel.TunnelId, ($"{tunnel.NodeName}-{tunnel.NodeID}-{tunnel.TunnelId}").GetMD5());
                        }
                        
                        return new()
                        {
                            Action = Pipe.PipeModel.OfAction.Start_Frpc,
                            Flag = true,
                        };
                    }
                }
                catch(Exception ex)
                {
                    return new()
                    {
                        Action = Pipe.PipeModel.OfAction.Start_Frpc,
                        Message = ex.ToString()
                    };
                }
                return new()
                {
                    Action = Pipe.PipeModel.OfAction.Start_Frpc,
                    Message = "未知原因"
                };                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      



            }
        }
        public static Pipe.PipeModel.ResponseModel Stop(Api.OfApiModel.Response.UserTunnelModel.UserTunnel tunnel)
        {
            Debugger.Break();
            try
            {
                
                if (RunningTunnelsWithProcess.ContainsKey(tunnel.TunnelId))
                {
                    if (!RunningTunnelsWithProcess[tunnel.TunnelId].HasExited)
                    {
                        RunningTunnelsWithProcess[tunnel.TunnelId].EnableRaisingEvents = false;

                        RunningTunnelsWithProcess[tunnel.TunnelId].Kill();
                        
                    }
                    RunningTunnelsWithProcess.Remove(tunnel.TunnelId);
                }
                if (RunningTunnels.ContainsKey(tunnel.TunnelId))
                {
                    RunningTunnels.Remove(tunnel.TunnelId);
                }
                return new()
                {
                    Action = Pipe.PipeModel.OfAction.Close_Frpc,
                    Flag = true
                };
            }
            catch { }
            return new()
            {
                Flag = false,
                Action = Pipe.PipeModel.OfAction.Close_Frpc
            };
        }

        static void Output(string? data, TraceLevel level)
        {
            if (!Utils.ServicesMode)
            {
                Console.ForegroundColor = level is TraceLevel.Error ? ConsoleColor.Red : ConsoleColor.Gray;
                Console.WriteLine($"{(level is TraceLevel.Error ? "[E]" : "[I]")} {data}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
