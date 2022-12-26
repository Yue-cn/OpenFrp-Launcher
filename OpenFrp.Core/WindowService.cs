using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Core
{
    /// <summary>
    /// Windows Service 基本实现
    /// </summary>
    public class WindowService : ServiceBase
    {
        private Pipe.PipeClient appClient { get; set; } = new Pipe.PipeClient();

        protected override async void OnStart(string[] args)
        {
            //Debugger.Launch();
            var appServer = new Pipe.PipeServer();
            appServer.Start();
            await appClient.Start(true);
        }
        protected override void OnStop()
        {
            if (appClient.State)
            {
                appClient.PushMessageAsync(new Pipe.PipeModel.RequestModel()
                {
                    Action = Pipe.PipeModel.OfAction.Server_Closed
                }).GetAwaiter().GetResult();
            }
        }
    }
    /// <summary>
    /// 兼容安装器
    /// </summary>
    [RunInstaller(true)]
    public class WindowsServicesInstaller : Installer
    {
        public WindowsServicesInstaller()
        {
            Installers.AddRange(new Installer[]  {
                new ServiceProcessInstaller()
                {
                    Account = ServiceAccount.LocalService,
                    Username = null,
                    Password = null,
                },
                new ServiceInstaller()
                {
                    ServiceName = "OpenFrp Launcher Service",
                    DisplayName = "OpenFrp Launcher Deamon Service",
                    Description = "OpenFrp 启动器 后台进程。",
                    StartType = ServiceStartMode.Manual,
                }
            });
        }
    }
}
