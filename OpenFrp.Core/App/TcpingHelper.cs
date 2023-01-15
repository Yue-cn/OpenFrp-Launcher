using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace OpenFrp.Core.App
{
    /// <summary>
    /// https://github.dev/TCPingInfoView/TCPingInfoView-Classic/blob/wpf/TCPingInfoViewLib/NetUtils/NetTest.cs
    /// </summary>
    public class TcpingHelper
    {
        public class TcpingResult
        {

            public TcpingResult(long ticks, TCPingLevel level = TCPingLevel.None)
            {
                Ticks = ticks;
                Level = level;
            }

            public long Ticks { get; set; }

            public TCPingLevel Level { get; set; }
        }
        public enum TCPingLevel
        {
            Red,
            Orange,
            Yellow,
            Green,
            None,
        }

        public async static ValueTask<TcpingResult> TcpingTest(IPAddress address,int port,int timeOut,CancellationToken _token = default)
        {
            using (var client = new TcpClient(AddressFamily.InterNetwork))
            {
                var task = client.ConnectAsync(address,port);
                var watch = new Stopwatch();
                watch.Start();
                var result = await Task.WhenAny(Task.Delay(timeOut,_token),task);

                if (result == task)
                {
                    // 成功
                    watch.Stop();
                    return new(watch.ElapsedMilliseconds, watch.ElapsedMilliseconds switch
                    {
                        < 100 => TCPingLevel.Green,
                        < 175 => TCPingLevel.Yellow,
                        _ => TCPingLevel.Orange,
                    });
                }
                else
                {
                    if (_token.IsCancellationRequested)
                    {
                        // 已经取消时,发送"用户已取消。"
                        return new(-1, TCPingLevel.None);
                    }
                    else
                    {
                        return new(-1, TCPingLevel.Red);
                    }
                }
                
            }
        }
    }
}
