using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Security.AccessControl;
using OpenFrp.Core.Api;
using OpenFrp.Core.App;


namespace OpenFrp.Core.Pipe
{
    public class PipeServer : Pipe.PipeStream
    {
        private NamedPipeServerStream? _server { get; set; }

        public bool State { get; private set; }

        public Func<PipeModel.OfAction, PipeModel.RequestModel, PipeModel.ResponseModel>? RequestFunction { get; set; }

        public Action? OnDisconnect { get; set; }

        public Action? OnClose { get; set; }

        private bool _push { get; set; }

        public void Start(bool push = false)
        {
            //Debugger.Break();
            PipeSecurity pipeSecurity = new();
            pipeSecurity.SetAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, AccessControlType.Allow));
            
            _push = push;
            AppStream = _server = new NamedPipeServerStream(Utils.PipeRouteName + (push ? "_PUSH" : ""), PipeDirection.InOut,12,PipeTransmissionMode.Byte,PipeOptions.Asynchronous,BufferSize,BufferSize,pipeSecurity);
            Utils.Debug($"PipeRouteName: {Utils.PipeRouteName + (push ? "_PUSH" : "")}");
            Utils.Log($"管道名称: {Utils.PipeRouteName + (push ? "_PUSH" : "")}");
            // 开始侦听连接
            Utils.Debug("开始侦听！");
            BeginLisenting();
        }

        public void Disconnect() => _server?.Disconnect();

        public void Close()
        {
            _server?.Close();
        }

        private bool BeginLisenting()
        {
            if (_server is not null)
            {

                try
                {
                    _server.BeginWaitForConnection(OnConnected, _server);
                }
                catch
                {
                    _server.Close();
                    Start(_push);
                    State = false;
                    return false;
                }
                State = true;
                return true;
            }
            // 一般指的是服务器"对象"没了 一般不会走这里
            return false;
        }

        private async void OnConnected(IAsyncResult o)
        {
            Utils.Debug("获得连接 !!!");
            _server!.EndWaitForConnection(o);
            while (State)
            {
                if (State)
                {
                    var response = await RevicedWithPrase();
                    
                    if (response is null)
                    {
                        Utils.Debug("已断开连接。");
                        if (OnDisconnect is not null) OnDisconnect();
                        if (_server?.IsConnected == true)
                        {
                            _server.Disconnect();
                        }
                        State = false;
                        break;
                    }
                    Utils.Debug($"接受到数据 !");

                    PipeModel.ResponseModel resp;

                    if (RequestFunction is not null)
                    {
                        resp = RequestFunction(response.Action, response);
                        
                    }
                    else
                    {
                        resp = Execute(response.Action, response);
                    }

                    await PushMessageAsync(resp);
                    Utils.Debug("发送数据.");

                    
                    //if (isExceptioned) { State = isExceptioned = false;  }

                    if (response.Action == PipeModel.OfAction.Close_Server)
                    {
                        _server?.Disconnect();
                        Environment.Exit(0);
                    }
                }
                else
                {
                    _server?.Disconnect();
                    break;
                }
            }
            await Task.Delay(250);
            BeginLisenting();
        }

        internal PipeModel.ResponseModel Execute(PipeModel.OfAction action, PipeModel.RequestModel request)
        {
            switch (action)
            {
                // 获取服务器的状态 / 信息
                case PipeModel.OfAction.Get_State:
                    {
                        return new() 
                        { 
                            Flag = true,
                            Action = PipeModel.OfAction.Get_State,
                            FrpMessage = new()
                            {
                                RunningId = ConsoleHelper.ConsoleWrappers.Keys.ToArray()
                            }
                        };
                    };
                // 服务器关闭了
                case PipeModel.OfAction.Close_Server:
                    {
                        foreach (int item in ConsoleHelper.ConsoleWrappers.Keys.ToArray())
                        {
                            if (!OfSettings.Instance.AutoRunTunnel.Contains(item))
                                OfSettings.Instance.AutoRunTunnel.Add(item);
                            ConsoleHelper.Stop(item);
                        }
                        return new()
                        {
                            Action = PipeModel.OfAction.Server_Closed,
                            Flag = true
                        };
                        
                    }
                // 推送登录信息
                case PipeModel.OfAction.LoginState_Push:
                    {
                        OfApi.Authorization = request.AuthMessage?.Authorization;
                        OfApi.Session = request.AuthMessage?.UserSession;
                        OfApi.UserInfoDataModel = request.AuthMessage?.UserDataModel;
                        return new()
                        {
                            Action = PipeModel.OfAction.LoginState_Push,
                            Flag = true
                        };
                    }
                // 推送登出信息
                case PipeModel.OfAction.LoginState_Logout:
                    {
                        OfApi.ClearAccount();
                        return new()
                        {
                            Flag = true,
                            Action = PipeModel.OfAction.LoginState_Logout,
                        };
                    }
                // 推送配置
                case PipeModel.OfAction.Push_Config:
                    {
                        var ww = OfSettings.Instance.AutoRunTunnel.ToList();
                        OfSettings.Instance = request.Config!;
                        OfSettings.Instance.AutoRunTunnel = ww;
                        return new()
                        {
                            Action = PipeModel.OfAction.Push_Config,
                            Flag = true
                        };
                    };
                // 开启 FRPC
                case PipeModel.OfAction.Start_Frpc:
                    {
                        if (request.LaunchTunnels.Count is not 0)
                        {
                            request.LaunchTunnels.ForEach(tunnel =>
                            {
                                var resq = ConsoleHelper.Launch(tunnel);
                                if (!resq.Flag)
                                {
                                    LogHelper.AllLogs.Add(new($"由于以下原因,隧道 {tunnel.NodeName} 无法进行开机自启: {resq.Message}",System.Diagnostics.TraceLevel.Warning));
                                }
                            });
                            return new(PipeModel.OfAction.Start_Frpc, true, "");
                        }
                        return ConsoleHelper.Launch(request.FrpMessage!.Tunnel!);
                    }
                // 关闭 FRPC
                case PipeModel.OfAction.Close_Frpc:
                    {
                        return new (PipeModel.OfAction.Close_Frpc, ConsoleHelper.Stop(request.FrpMessage!.Tunnel!.TunnelId),"");
                    }
                // 推送 LOG
                case PipeModel.OfAction.Get_Logs:
                    {
                        if (LogHelper.AllLogs.Count > 500)
                        {
                            ConsoleHelper.ConsoleWrappers.Values.ToList().ForEach(item =>
                            {
                                item.Content.Clear();
                            });
                            LogHelper.AllLogs.Clear();
                        }

                        return new()
                        {
                            Action = PipeModel.OfAction.Get_Logs,
                            Flag = true,
                            Logs = new()
                            {
                                ConsoleWrappers = LogHelper.GetAllWrapper()
                            }
                        };
                        
                    }
                // 上传 LOG
                case PipeModel.OfAction.Push_Logs:
                    {
                        Utils.Log(request.PushLog ?? "");
                        return new(PipeModel.OfAction.Push_Logs, true, "");
                    };
                // 清除日志
                case PipeModel.OfAction.Clear_Logs:
                    {
                        if (request.FrpMessage?.Tunnel is Api.OfApiModel.Response.UserTunnelModel.UserTunnel tunnel)
                        {
                            if (ConsoleHelper.ConsoleWrappers.ContainsKey(tunnel.TunnelId))
                            {
                                ConsoleHelper.ConsoleWrappers[tunnel.TunnelId].Content.Clear();
                            }
                        }
                        else
                        {
                            ConsoleHelper.ConsoleWrappers.Values.ToList().ForEach(value => value.Content.Clear());
                            LogHelper.AllLogs.Clear();
                        }
                        return new(PipeModel.OfAction.Clear_Logs, true, "");
                    };
                default:
                    {
                        return new() { Message = "Action Not Found" };
                    }
            }
            
        }
    }
}
