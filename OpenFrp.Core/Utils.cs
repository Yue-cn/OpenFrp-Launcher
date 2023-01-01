using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace OpenFrp.Core
{
    public static class Utils
    {
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

        /// <summary>
        /// 应用主窗口（操控台返回 Null)
        /// </summary>
        public static System.Windows.Window? MainWindow
        {
            get => System.Windows.Application.Current?.MainWindow;
        }
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


        //private static StreamWriter? _writer = Utils.ServicesMode ? new StreamWriter(Path.Combine(ApplicationPath, "logs.txt")) : default;

        internal static void Debug(string s)
        {
            if (!Utils.ServicesMode)
            {
                Console.WriteLine($"[{DateTimeOffset.Now}] {s}");
            }
            //else
            //{
            //    _writer?.WriteLineAsync($"[{DateTimeOffset.Now}] {s}");
            //}
        }
        
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
    }
}
