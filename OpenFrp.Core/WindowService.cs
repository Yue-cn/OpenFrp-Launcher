using OpenFrp.Core.Api;
using OpenFrp.Core.App;
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

            await Task.Delay(1500);

            Api.OfApiModel.Response.UserInfoModel.UserInfoDataModel UserDataModel = new();
            var res1 = await OfApi.Login(OfSettings.Instance.Account.User, OfSettings.Instance.Account.Password);
            var res2 = await OfApi.GetUserInfo();
            string er = "";
            if (!res1.Flag || !res2.Flag)
            {
                // 请求失败的逻辑
                er = $"请求失败。{(res1.Flag ? null : res1.Message)},{(res2.Flag ? null : res2.Message)} \n {OfSettings.Instance}";
            }
            else
            {
                OfApi.Session = res1.Data;
                UserDataModel = res2.Data;
            }
            // 更改部分请求
            appServer.RequestFunction = (request, model) =>
            {
                switch (request)
                {
                    case Pipe.PipeModel.OfAction.Get_State:
                        return new()
                        {
                            Flag = true,
                            Action = Pipe.PipeModel.OfAction.Get_State,
                            AuthMessage = new()
                            {
                                UserDataModel = UserDataModel,
                                Authorization = OfApi.Authorization,
                                UserSession = OfApi.Session
                            },
                            FrpMessage = new()
                            {
                                RunningId = ConsoleHelper.RunningTunnels.Keys.ToArray()
                            },
                            Message = er
                        };
                    default: return appServer.Execute(request, model);
                }
            };

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
                    StartType = ServiceStartMode.Automatic,
                }
            });
        }
    }
}
