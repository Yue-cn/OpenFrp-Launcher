using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OpenFrp.Launcher
{
    public partial class App : Application
    {
        /// <summary>
        /// 应用启动时
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            
        }
        /// <summary>
        /// 链接被点击
        /// </summary>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }
    }
}
