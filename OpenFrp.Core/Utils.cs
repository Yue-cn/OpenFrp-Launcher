using OpenFrp.Core.App;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace OpenFrp.Core
{
    public static class Utils
    {
        #region Extend Functions
        /// <summary>
        /// 将<see cref="byte[]"/>转为<see cref="string"/>
        /// </summary>
        public static string GetString(this byte[] bytes,bool isRemoveSpaceChar)
        {
            string str = Encoding.UTF8.GetString(bytes);
            if (isRemoveSpaceChar) { return str.Replace('\0',new char()); }
            return str;
        }
        /// <summary>
        /// 将<see cref="string"/>转为<see cref="byte[]"/>
        /// </summary>
        public static byte[] GetBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        /// <summary>
        /// 获取文本的 MD5
        /// </summary>
        public static string GetMD5(this string str)
        {
            var builder = new StringBuilder();
            byte[] buffer = str.GetBytes();
            foreach (byte item in MD5.Create().ComputeHash(buffer,0,buffer.Length))
            {
                builder.Append(item.ToString("x2"));
            }
            return builder.ToString();
        }
        public static async Task<T?> WithCancelToken<T>(this Task<T> task, CancellationToken _token)
        {
            var src = Task.Run(() => { _token.WaitHandle.WaitOne(5000); });

            if (task != await Task.WhenAny(task, src))
            {
                return default;
            }
            return task.Result;


        }
        public static async ValueTask<OfProcessInfo[]> GetAliveNetworkLink()
        {
            var process = Process.Start(new ProcessStartInfo("netstat.exe", "-ano")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            var dic = new Dictionary<string, string>();
            var pool = new List<Task<OfProcessInfo>>();
            foreach (var str in (await process.StandardOutput.ReadToEndAsync()).Split('\n'))
            {
                var args = str.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                if (args.Length < 3 || !(args[0] is "TCP" || args[0] is "UDP"))
                {
                    continue;
                }
                if (args[1][0] is '[') continue;


                pool.Add(Task.Run<OfProcessInfo>(() =>
                {
                    string pid = args[0] is "UDP" ? args[3] : args[4];
                    if (!dic.ContainsKey(pid))
                    {
                        dic[pid] = "[拒绝访问]";
                        try
                        {
                            dic[pid] = Process.GetProcessById(Convert.ToInt32(pid)).ProcessName;
                        }
                        catch { }
                    }
                    return new()
                    {
                        ProcessName = dic[pid],
                        Address = args[1].Split(':').First(),
                        Port = Convert.ToInt32(args[1].Split(':').Last())
                    };
                }));
            }

            return await Task.WhenAll(pool);
        }
        public class OfProcessInfo
        {
            public string? Address { get; set; }

            public int Port { get; set; }

            public string? ProcessName { get; set; }
        }
        #endregion

        #region Helper Args
        /// <summary>
        /// 应用主窗口（操控台返回 Null)
        /// </summary>
        public static System.Windows.Window? MainWindow
        {
            get => System.Windows.Application.Current?.MainWindow;
        }
        /// <summary>
        /// 是否在系统服务模式
        /// </summary>
        public static bool ServicesMode { get; } = !Environment.UserInteractive;
        /// <summary>
        /// 应用程序 所在目录
        /// </summary>
        public static string ApplicationPath { get; } = $"{AppDomain.CurrentDomain.BaseDirectory}";
        /// <summary>
        /// 管道的昵称
        /// </summary>
        public static string PipeRouteName { get; } = $"OfApp_{ApplicationPath.GetMD5()}";
        /// <summary>
        /// 配置文件的文件目录
        /// </summary>
        public static string ApplicationConfigPath { get; } = Path.Combine(ApplicationPath,"config.json");
        /// <summary>
        /// Core 文件
        /// </summary>
        public static string CorePath { get; } = Path.Combine(ApplicationPath, "OpenFrp.Core.exe");
        /// <summary>
        /// 所在文件名
        /// </summary>
        public static string ExcutableName { get; } = Process.GetCurrentProcess().MainModule.FileName;
        /// <summary>
        /// 存储文件目录
        /// </summary>
        public static string AppTempleFilesPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"OfApp.Launcher");

        public static string ApplicationVersions { get; } = "OpenFrp.Launcher.[v2.0.0].p9aj2";

        public static string FrpcPlatForm { get; } = Environment.Is64BitOperatingSystem ? "frpc_windows_amd64" : "frpc_windows_386";

        public static string Frpc { get; } = Path.Combine(ApplicationPath, "frpc", $"{FrpcPlatForm}.exe");

        #endregion

        /// <summary>
        /// 写日志
        /// </summary>
        internal static void Debug(string s)
        {
            if (!Utils.ServicesMode)
            {
                Console.WriteLine($"[{DateTimeOffset.Now}] {s}");
            }
        }
        /// <summary>
        /// 启动器写日志
        /// </summary>
        public static void WriteLog(string s) => System.Diagnostics.Debug.WriteLine($"[{DateTimeOffset.Now}] {s}");
        /// <summary>
        /// 检查服务是否开启
        /// </summary>
        public static void CheckService()
        {
            using var service = new ServiceController("OpenFrp Launcher Service");
            if (!service.CanStop)
            {
                Process.Start(new ProcessStartInfo("sc", "start \"OpenFrp Launcher Service\"")
                {
                    Verb = "runas",
                    CreateNoWindow = true
                });
            }
        }
        public static void StopService()
        {
            if (OfSettings.Instance.WorkMode is WorkMode.DeamonService)
            {
                Process.Start(new ProcessStartInfo("sc", "stop \"OpenFrp Launcher Service\"")
                {
                    Verb = "runas",
                    CreateNoWindow = true
                });
            }
        }

        

    }
}
