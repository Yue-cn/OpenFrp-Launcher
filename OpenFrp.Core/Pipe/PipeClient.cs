 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using Newtonsoft.Json;

namespace OpenFrp.Core.Pipe
{
    public class PipeClient : Pipe.PipeStream
    {
        private NamedPipeClientStream? _client { get; set; }

        public bool State { get; private set; }

        public async ValueTask Start(bool push = false)
        {
            AppStream = _client = new NamedPipeClientStream(".", Utils.PipeRouteName + (push ? "_PUSH" : ""),
                PipeDirection.InOut, PipeOptions.Asynchronous);
            await _client.ConnectAsync();
            State = true;
        }
        public async ValueTask<PipeModel.ResponseModel> PushMessage(PipeModel.RequestModel request)
        {
            if (State)
            {
                
                return await base.PushMessageWithRequest(request) ??
                    new() { Message = "后台处理错误,请稍后重试。" };
            }
            return new() { Message = "尚未连接到后台,请稍后重试。" };
        }
    }
}
