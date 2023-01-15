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
                if (!OfApi.LoginState)
                {
                    Utils.Log($"找不到文件,{OfApi.UserInfoDataModel}");
                    return new(OfAction.Start_Frpc, false, "用户未登录。");
                }
                if (!File.Exists(Utils.Frpc))
                {
                    Utils.Log($"找不到文件,{Utils.Frpc}");
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
                    frpc.ErrorDataReceived += (sender, e) => Output(e.Data, TraceLevel.Error, tunnel.TunnelId);

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


        static async void Output(string? data,TraceLevel level,int tunnelId)
        {
            if (data is not null)
            {

                if (ConsoleWrappers.ContainsKey(tunnelId))
                {
                    ConsoleWrappers[tunnelId].Append(data, level);
                    if (Utils.ServicesMode) return;
                    if (OfSettings.Instance.NotifiyMode == NotifiyMode.ToastNotification && Utils.isSupportToast)
                    {
                        if (data.Contains("启动成功"))
                        {
                            new ToastContentBuilder()
                                .AddText($"隧道 {ConsoleWrappers[tunnelId].UserTunnelModel?.TunnelName} 启动成功!")
                                .AddText($"点击复制按钮，分享给你的朋友吧!")
                                .AddAttributionText($"{ConsoleWrappers[tunnelId].UserTunnelModel?.TunnelType},{ConsoleWrappers[tunnelId].UserTunnelModel?.LocalAddress}:{ConsoleWrappers[tunnelId].UserTunnelModel?.LocalPort}")
                                .AddButton("复制连接", ToastActivationType.Foreground, $"--cl {ConsoleWrappers[tunnelId].UserTunnelModel?.ConnectAddress}")
                                .AddButton("确定", ToastActivationType.Foreground, "")
                                .Show();
                        }
                        else if (data.Contains("启动失败"))
                        {
                            new ToastContentBuilder()
                                .AddText($"隧道 {ConsoleWrappers[tunnelId].UserTunnelModel?.TunnelName} 启动失败。")
                                .AddText(data)
                                .AddAttributionText($"远程端口: {ConsoleWrappers[tunnelId].UserTunnelModel?.RemotePort}")
                                .AddButton("确定", ToastActivationType.Foreground, "")
                                .Show();
                        }
                    }
                    else if (OfSettings.Instance.NotifiyMode is NotifiyMode.NotifiyIcon)
                    {
                        if (Utils.ClientWorker is not null && Utils.ClientWorker.State)
                        {
                            if (data.Contains("启动成功"))
                            {
                                await Utils.ClientWorker.PushMessageAsync(new()
                                {
                                    Action = OfAction.Push_AppNotifiy,
                                    ToastContent = new()
                                    {
                                        IsSuccessful = true,
                                        UserTunnel = ConsoleWrappers[tunnelId].UserTunnelModel
                                    }
                                });
                            }
                            else if (data.Contains("启动失败"))
                            {
                                await Utils.ClientWorker.PushMessageAsync(new()
                                {
                                    Action = OfAction.Push_AppNotifiy,
                                    ToastContent = new()
                                    {
                                        Data = data,
                                        IsSuccessful = false,
                                    }
                                });
                            }
                        }
                    }


                    // 此处不考虑 系统服务 模式下的通知

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
