using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Security.AccessControl;
using Windows.ApplicationModel.VoiceCommands;
using Windows.UI.WebUI;
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
            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.SetAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, AccessControlType.Allow));
            Utils.Debug("Appplication Startup!!");
            _push = push;
            AppStream = _server = new NamedPipeServerStream(Utils.PipeRouteName + (push ? "_PUSH" : ""), PipeDirection.InOut,12,PipeTransmissionMode.Byte,PipeOptions.Asynchronous,BufferSize,BufferSize,pipeSecurity);
            Utils.Debug($"PipeRouteName: {Utils.PipeRouteName + (push ? "_PUSH" : "")}");
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
                    Utils.Debug($"接受到数据 ! Data Content: {response}");

                    if (RequestFunction is not null)
                    {
                        await PushMessageAsync(RequestFunction(response.Action, response));
                    }
                    else
                    {
                        await PushMessageAsync(Execute(response.Action, response));
                    }

                    Utils.Debug("发送数据。");
                    
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
                case PipeModel.OfAction.Get_State:
                    {
                        return new() 
                        { 
                            Flag = true,
                            Action = PipeModel.OfAction.Get_State,
                            FrpMessage = new()
                            {
                                RunningId = ConsoleHelper.RunningTunnels.Keys.ToArray()
                            }
                        };
                    };
                case PipeModel.OfAction.Close_Server:
                    {
                        return new()
                        {
                            Action = PipeModel.OfAction.Server_Closed,
                            Flag = true
                        };
                    }
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
                case PipeModel.OfAction.LoginState_Logout:
                    {
                        OfApi.ClearAccount();
                        return new()
                        {
                            Flag = true,
                            Action = PipeModel.OfAction.LoginState_Logout,
                        };
                    }
                case PipeModel.OfAction.Start_Frpc:
                    {
                        if (OfApi.UserInfoDataModel is null || request.FrpMessage is null)
                        {
                            return new()
                            {
                                Flag = false,
                                Action = PipeModel.OfAction.Start_Frpc
                            };
                        }
                        
                        // 开启Frpc
                        return ConsoleHelper.Launch(request.FrpMessage.Tunnel!);
                    }
                case PipeModel.OfAction.Close_Frpc:
                    {
                        if (OfApi.UserInfoDataModel is null || request.FrpMessage is null)
                        {
                            //正常来说 不会出现此情况
                        }
                        
                        // 关闭Frpc
                        return ConsoleHelper.Stop(request.FrpMessage!.Tunnel!);
                    }
                default:
                    {
                        return new() { Message = "Action Not Found" };
                    }
            }
            
        }
    }
}
