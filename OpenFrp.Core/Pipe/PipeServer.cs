using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Security.AccessControl;

namespace OpenFrp.Core.Pipe
{
    public class PipeServer : Pipe.PipeStream
    {
        private NamedPipeServerStream? _server { get; set; }

        public bool State { get; private set; }

        public void Start(bool push = false)
        {
            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.SetAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, AccessControlType.Allow));
            Utils.Debug("Appplication Startup!!");
            AppStream = _server = new NamedPipeServerStream(Utils.PipeRouteName + (push ? "_PUSH" : ""), PipeDirection.InOut,12,PipeTransmissionMode.Byte,PipeOptions.Asynchronous,BufferSize,BufferSize,pipeSecurity);
            Utils.Debug($"PipeRouteName: {Utils.PipeRouteName + (push ? "_PUSH" : "")}");
            // 开始侦听连接
            Utils.Debug("开始侦听！");
            BeginLisenting();
        }
        private bool BeginLisenting()
        {
            if (_server is not null)
            {
                _server.BeginWaitForConnection(OnConnected, _server);
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
                        Utils.Debug("已断开联机");
                        _server.Disconnect();
                        State = false;
                        break;
                    }
                    Utils.Debug($"接受到数据 ! {response}");

                    await PushMessage(Execute(response.Action));

                    Utils.Debug("发送数据。");
                }
                else break;
            }
            BeginLisenting();
        }

        private PipeModel.ResponseModel Execute(PipeModel.OfAction action)
        {

            switch (action)
            {
                case PipeModel.OfAction.CLIENT_TO_SERVER:
                    {
                        return new() 
                        { 
                            Flag = true,
                            Message = "success",
                            Action = PipeModel.OfAction.SERVER_TO_CLIENT 
                        };
                    };
                default:
                    {
                        return new() { Message = "Action Not Found" };
                    }
            }
            
        }
    }
}
