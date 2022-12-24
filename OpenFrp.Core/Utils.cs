using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        /// <summary>
        /// 应用程序 所在目录
        /// </summary>
        public static string ApplicationPath { get; } = $"{AppDomain.CurrentDomain.BaseDirectory}";
        /// <summary>
        /// 管道的昵称
        /// </summary>
        public static string PipeRouteName { get; } = $"OfApp_{ApplicationPath.GetMD5()}";

        internal static void Debug(string s) => Console.WriteLine($"[{DateTimeOffset.Now}] {s}");
        

    }
}
