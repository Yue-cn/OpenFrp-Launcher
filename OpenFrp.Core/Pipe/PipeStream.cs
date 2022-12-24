using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Core.Pipe
{
    /// <summary>
    /// Pipe 的流控制。
    /// </summary>
    public class PipeStream
    {
        public System.IO.Pipes.PipeStream? AppStream { get; set; }

        /// <summary>
        /// 缓存区块大小
        /// </summary>
        public int BufferSize { get; set; } = 1536;

        public PipeStream() { }

        public PipeStream(System.IO.Pipes.PipeStream? appStream)
        {
            AppStream = appStream;
        }

        public async ValueTask<byte[]> Reviced()
        {
            return await Task.Run(() =>
            {
                if (AppStream is not null)
                {
                    try
                    {
                        byte[] buffer = new byte[BufferSize];
                        if (AppStream.Read(buffer, 0, BufferSize) > 0)
                        {
                            return buffer;
                        }
                    }
                    catch { }
                }
                return new byte[0];
            });
        }
        public async ValueTask<PipeModel.RequestModel?> RevicedWithPrase()
        {
            var request = JsonConvert.DeserializeObject<PipeModel.RequestModel>((await Reviced()).GetString(true));
            return request;
        }
        public async ValueTask PushMessage(PipeModel.BaseModel message)
        {
            await Task.Run(() =>
            {
                if (AppStream is not null)
                {
                    try
                    {
                        byte[] bytes = message.ToString().GetBytes();
                        AppStream.Write(bytes, 0, bytes.Length);
                    }
                    catch { }
                }
            });
        }
        public async ValueTask<PipeModel.ResponseModel> PushMessageWithRequest(PipeModel.RequestModel request)
        {
            await PushMessage(request);
            return JsonConvert.DeserializeObject<PipeModel.ResponseModel>((await Reviced()).GetString(true)) ??
                new() { Message = "后台处理错误,请稍后重试。" };
        }
        
    }
}
