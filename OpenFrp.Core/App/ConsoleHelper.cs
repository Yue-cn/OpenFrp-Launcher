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
                    //Debugger.Break();
                    new Thread(() =>
                    {
                        try
                        {
                            string hashName = ($"{tunnel.NodeName}-{tunnel.NodeID}-{tunnel.TunnelId}").GetMD5();
                            if (LogHelper.LogsList.ContainsKey(hashName)) LogHelper.LogsList.Remove(hashName);
                            LogHelper.LogsList.Add(hashName, new());
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

                            process.OutputDataReceived += (sender, args) => Output(args.Data, LogsLevel.Info, hashName,tunnel);
                            process.ErrorDataReceived += (sender, args) => Output(args.Data, LogsLevel.Error, hashName,tunnel);
                            process.Exited += (sender, args) =>
                            {
                                Output($"进程已退出，返回值 {process.ExitCode};", LogsLevel.Warning, hashName,tunnel);
                                Launch(tunnel);
                            };

                            if (process.Start())
                            {

                                process.BeginOutputReadLine();
                                process.BeginErrorReadLine();
                                if (!RunningTunnels.ContainsKey(tunnel.TunnelId) &&
                                    !RunningTunnelsWithProcess.ContainsKey(tunnel.TunnelId))
                                {
                                    RunningTunnelsWithProcess.Add(tunnel.TunnelId, process);
                                    RunningTunnels.Add(tunnel.TunnelId, hashName);
                                }

                            }
                        }
                        catch {  }
                    }).Start();
                    return new()
                    {
                        Action = Pipe.PipeModel.OfAction.Start_Frpc,
                        Flag = true,
                    };
                }
                catch(Exception ex)
                {
                    return new()
                    {
                        Action = Pipe.PipeModel.OfAction.Start_Frpc,
                        Message = ex.ToString()
                    };
                }
            }
        }

        public static Pipe.PipeModel.ResponseModel Stop(Api.OfApiModel.Response.UserTunnelModel.UserTunnel tunnel)
        {
            
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



        static void Output(string? data, LogsLevel level,string dicName, Api.OfApiModel.Response.UserTunnelModel.UserTunnel tunnel)
        {
            if (data is null) return;
            
            if (data?.IndexOf("启动成功") != -1)
            {
                Debugger.Break();
                new ToastContentBuilder()
                    .AddText("OpenFrp Launcher")
                    .AddText($"隧道 {tunnel.TunnelName} 启动成功!")
                    .AddButton(new ToastButton("复制连接", $"--cl {tunnel.ConnectAddress}"))
                    .AddButton(new ToastButton("确定", "--ok"))
                    .Show();
            }
            else if (data?.IndexOf("启动失败") != -1)
            {
                new ToastContentBuilder()
                    .AddText("OpenFrp Launcher")
                    .AddText($"隧道 {tunnel.TunnelName} 失败!!!")
                    .AddText(data ?? "")
                    
                    .Show();
            }
            if (!Utils.ServicesMode)
            {
                try
                {
                    LogHelper.LogsList[dicName].Add(new()
                    {
                        Level = level,
                        Content = data
                    });
                }
                catch { }
            }
        }
    }
}
