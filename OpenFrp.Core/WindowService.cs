using OpenFrp.Core.Api;
using OpenFrp.Core.App;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace OpenFrp.Core
{
    /// <summary>
    /// Windows Service 基本实现
    /// </summary>
    public class WindowService : ServiceBase
    {
        protected override async void OnStart(string[] args)
        {
            var appServer = new Pipe.PipeServer();
            appServer.Start();
            await Task.Delay(1000);

            // 五次重连 都失败那么Pass
            int linkCount = 0;
            while(!await GetLink() || linkCount >= 5)
            {
                linkCount++;
                Utils.Log($"登录失败,重试 {linkCount} 次，共5次。等待 {3 * linkCount}s.", TraceLevel.Warning);
                
                await Task.Delay(3000 * linkCount);
            };
            if (linkCount >= 5) Utils.Log("五次登录都失败了，是否已连接到互联网???", TraceLevel.Error);
            else Utils.Log("登录成功!!!");


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
                                UserDataModel = OfApi.UserInfoDataModel,
                                Authorization = OfApi.Authorization,
                                UserSession = OfApi.Session
                            },
                            FrpMessage = new()
                            {
                                RunningId = ConsoleHelper.ConsoleWrappers.Keys.ToArray()
                            },
                        };
                    default: return appServer.Execute(request, model);
                }
            };

            Utils.ClientWorker = new();
            await Utils.ClientWorker.Start(true);

        }
        /// <summary>
        /// 登录并获取账户信息。
        /// </summary>
        /// <returns></returns>
        private async ValueTask<bool> GetLink()
        {
            try
            {
                var res1 = await OfApi.Login(OfSettings.Instance.Account.User, OfSettings.Instance.Account.Password);
                var res2 = await OfApi.GetUserInfo();
                if (!res1.Flag || !res2.Flag)
                {
                    // 请求失败的逻辑
                    Utils.Log($"请求失败。Messages: [{res1.Message},{res2.Message}]", TraceLevel.Warning);
                    return false;
                }
                else
                {
                    OfApi.Session = res1.Data;
                    OfApi.UserInfoDataModel = res2.Data;

                    // 如果配置文件内自动运行的数量不为0
                    // 那么这里开始进行配置对
                    if (OfSettings.Instance.AutoRunTunnel.Count is not 0)
                    {
                        var res3 = await OfApi.GetUserProxies();

                        if (res3.Flag && res3.Data.Count > 0) res3.Data.List.ForEach(tunnel =>
                        {
                            OfSettings.Instance.AutoRunTunnel.ForEach((tunnelId) =>
                            {
                                if (tunnelId == tunnel.TunnelId)
                                {
                                    var resp = ConsoleHelper.Launch(tunnel);
                                    if (!resp.Flag)
                                    {
                                        Utils.Log($"由于以下原因,隧道 {tunnel.TunnelName} 无法完成开机自启 (服务): {resp.Message}");
                                    }
                                    return;
                                }
                            });
                        });
                        else Utils.Log($"请求失败,TunnelsMessage: [{res3.Data.Count},{res3.Message}]", TraceLevel.Warning);
                    }
                }
                return true;
            }
            catch {  }

            return false;

        }

        protected override void OnStop()
        {
            // 关闭客户端
            if (Utils.ClientWorker?.State == true)
            {
                Utils.ClientWorker?.PushMessageAsync(new Pipe.PipeModel.RequestModel()
                {
                    Action = Pipe.PipeModel.OfAction.Server_Closed
                }).GetAwaiter().GetResult();
            }
            // 关闭所有后台隧道
            if (ConsoleHelper.ConsoleWrappers.Count != 0)
            {
                foreach (var item in ConsoleHelper.ConsoleWrappers.Keys.ToArray())
                {
                    ConsoleHelper.Stop(item);
                }
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
