using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using OpenFrp.Core.Api;
using OpenFrp.Core.Api.OfApiModel;
using OpenFrp.Core.App;
using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Windows;
using static OpenFrp.Core.Api.OfApiModel.Response;

namespace OpenFrp.Core
{
    internal class Program
    {
        #region Win32 Helper
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleExitEvent handler, bool add);
        private delegate bool ConsoleExitEvent(CtrlType sig);
        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        #endregion

        internal async static Task Main(string[] args)
        {
            /*
                特此谢明: SakuraFrp
             */
            if (Utils.ExcutableName != Utils.CorePath)
            {
                MessageBox.Show("请不要重命名 OpenFrp.Core.exe", "Core Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await OfSettings.ReadConfig();

            if (args.Length is 1)
            {
                AppDomain.CurrentDomain.UnhandledException += async (se, c) =>
                {
                    if (!Utils.ServicesMode)
                    {
                        MessageBox.Show(c.ExceptionObject.ToString(),"Core Exception (您需要重新打开启动器才可使用。)");
                    }
                    else
                    {
                        string file = Path.Combine(Utils.AppTempleFilesPath, "error.log");
                        try
                        {

                            if (File.Exists(file)) File.Delete(file);
                            await new StreamWriter(file,false,Encoding.UTF8,4096).WriteAsync(c.ExceptionObject.ToString());
                        }
                        catch
                        {
                            // 没救了
                        }

                    }
                    if (ConsoleHelper.ConsoleWrappers.Count > 0)
                    {
                        ConsoleHelper.ConsoleWrappers.Keys.ToList().ForEach(id =>
                        {
                            ConsoleHelper.Stop(id);
                        });
                    }
                    Utils.KillAllFrpc();
                    if (Utils.isSupportToast) ToastNotificationManagerCompat.Uninstall();
                };

                if (Utils.isSupportToast) ToastNotificationManagerCompat.OnActivated += (args) =>
                {
                    if (string.IsNullOrEmpty(args.Argument)) return;
                    if (args.Argument.IndexOf("--cl") != -1)
                    {
                        try
                        {
                            var thread = new Thread(() =>
                            {
                                try
                                {
                                    Clipboard.SetText(args.Argument.Split(' ')[1]);
                                }
                                catch
                                {
                                    
                                }
                            });
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.IsBackground = true;
                            thread.Start();
                        }
                        catch { }
                    }
                };
                switch (args[0])
                {
                    case "--install":await InstallService();break;
                    case "--uninstall":await UninstallService();break;
                    case "--force-uninstall":ForceUninstallService();break;
                    case "--ws":await LocalPipeWorker();break;
                    case "--frpcp":await InstallFrpc();break;
                    case "--ins":
                        {
                            try 
                            { 
                                Process.Start(new ProcessStartInfo("certutil",$"-addStore root \"{AppDomain.CurrentDomain.BaseDirectory}\\5c5a2d56dc3359a84ad0fd928ba33ccb.cer\"")
                                {
                                    Verb = "runas"
                                });
                                // 等前端需要
                                // AppURLScheme.RegistryKey();
                            }
                            catch { }
                        };break;
                    case "--uins":
                        {
                            try
                            {
                                // 等前端需要
                                // AppURLScheme.UnregistryKey();
                                Process.Start(new ProcessStartInfo("certutil", $"-delStore root \"ZGIT Network\"")
                                {
                                    Verb = "runas"
                                });
                                
                            }
                            catch { }
                        }; break;
                }
                if (Utils.isSupportToast)
                {
                    ToastNotificationManagerCompat.History.Clear();
                    ToastNotificationManagerCompat.Uninstall();
                };
            }
            else if (Utils.ServicesMode)
            {
                ServiceBase.Run(new WindowService());
            }
        }
        /// <summary>
        /// 安装服务
        /// </summary>
        private static async ValueTask InstallService()
        {
            if (Utils.IsServiceInstalled()) return;
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Utils.RunAsAdmin(new ProcessStartInfo(Utils.CorePath, "--install"));
            }
            else
            {
                try
                {
                    var dir = new DirectoryInfo(Utils.ApplicationPath);
                    var acl = dir.GetAccessControl(System.Security.AccessControl.AccessControlSections.Access);
                    acl.SetAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                        new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null),
                        System.Security.AccessControl.FileSystemRights.FullControl,
                        System.Security.AccessControl.InheritanceFlags.ObjectInherit,
                        System.Security.AccessControl.PropagationFlags.None,
                        System.Security.AccessControl.AccessControlType.Allow));
                    dir.SetAccessControl(acl);

                    ManagedInstallerClass.InstallHelper(new string[] { Utils.CorePath });

                    Process.Start(new ProcessStartInfo("sc", "start \"OpenFrp Launcher Service\""));

                    OfSettings.Instance.WorkMode = WorkMode.DeamonService;

                    await OfSettings.Instance.WriteConfig();

                    await Task.Delay(3250);

                    Process.Start(new ProcessStartInfo("explorer", Path.Combine(Utils.ApplicationPath, "OpenFrp.Launcher.exe")));
                }
                catch (Exception ex)
                {
                    Utils.Debug(ex.ToString());
                    //Console.WriteLine("");
                }

            }
        }
        /// <summary>
        /// 卸载服务
        /// </summary>
        private static async ValueTask UninstallService()
        {
            if (Utils.IsServiceInstalled())
            {
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Utils.RunAsAdmin(new ProcessStartInfo(Utils.CorePath, "--uninstall"));
                }
                else
                {
                    ManagedInstallerClass.InstallHelper(new string[] { "-u", Utils.CorePath });
                }
            }
            OfSettings.Instance.WorkMode = WorkMode.DeamonProcess;
            await OfSettings.Instance.WriteConfig();
            Process.Start(new ProcessStartInfo("explorer", Path.Combine(Utils.ApplicationPath, "OpenFrp.Launcher.exe")));
            
        }
        /// <summary>
        /// 强制卸载
        /// </summary>
        private static void ForceUninstallService()
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Utils.RunAsAdmin(new ProcessStartInfo(Utils.CorePath, "--uninstall"));
            }
            else
            {
                if (Utils.IsServiceInstalled())
                {
                    ManagedInstallerClass.InstallHelper(new string[] { "-u", Utils.CorePath });
                    File.Delete(Utils.ApplicationConfigPath);
                    File.Delete(Utils.Frpc);
                    Directory.Delete(Utils.AppTempleFilesPath, true);
                }
            }
        }

        /// <summary>
        /// Launcher - 管道交互
        /// </summary>
        private static async ValueTask LocalPipeWorker()
        {
            
            // 服务端
            var appServer = new Pipe.PipeServer();
            appServer.Start();

            // Debugger.Launch();

            // 连接启动器用的。
            // 既然这里连接成功了，那么下方的可以与前台交互了。

            if (!Utils.ServicesMode)
            {
                Utils.ClientWorker = new Pipe.PipeClient();
                //await Utils.ClientWorker.Start(true);
                // 退出事件
                SetConsoleCtrlHandler((ctrl) =>
                {
                    if (Utils.isSupportToast)
                    {
                        ToastNotificationManagerCompat.History.Clear();
                        ToastNotificationManagerCompat.Uninstall();
                    };
                    // 如果有客户端连接，那么这里需要推送。
                    Utils.ClientWorker.PushMessageAsync(new Pipe.PipeModel.RequestModel()
                    {
                        Action = Pipe.PipeModel.OfAction.Server_Closed
                    }).GetAwaiter().GetResult();
                    Environment.Exit(0);
                    return true;
                }, true);
                appServer.RequestFunction = (request, model) =>
                {
                    switch (request)
                    {
                        case Pipe.PipeModel.OfAction.Get_State:
                            {
                                new Thread(async () => await Utils.ClientWorker.Start(true)).Start();
                                goto default;
                            };
                        default: return appServer.Execute(request, model);
                    }
                };
            }
            while (true)
            {
                if (Utils.ServicesMode)
                {
                    await Task.Delay(1500);
                }
                else
                {
                    switch (Console.ReadLine())
                    {
                        case "user":
                            {
                                Console.WriteLine($"Session: {OfApi.Session}\nAppAuth: {OfApi.Authorization}\nUserName: {OfApi.UserInfoDataModel?.UserName ?? "UnLogin"}");
                            }; break;
                        case "rp":
                            {
                                foreach (var item in App.ConsoleHelper.ConsoleWrappers)
                                {
                                    Console.WriteLine($"Key: {item.Key} , Value: {item.Value}");
                                }
                            }; break;
                        case "debug":
                            {
                                Debugger.Launch();
                            };break;
                    }
                }
            }
        }
        /// <summary>
        /// 安装 FRPC
        /// </summary>
        private static async ValueTask InstallFrpc()
        {
            Console.Title = "OpenFrp Launcher 正在下载 FRPC,请耐心等待。 - www.openfrp.net";
            var updater = await Update.CheckUpdate();
            if (string.IsNullOrEmpty(updater.DownloadUrl) || updater.UpdateFor != Update.UpdateFor.FRPC)
            {
                Console.WriteLine($"API 请求失败,请将截图发送到用户交流群内::: {updater.Content},{updater.DownloadUrl},{updater.UpdateFor}");
                Console.WriteLine($"{(updater.UpdateFor is Update.UpdateFor.Launcher ? "关闭该窗口后打开启动器。" : "请将此截图发送到交流群中。")}");
                Console.ReadKey();
                Environment.Exit(0);
            }
            string file = Path.Combine(Utils.AppTempleFilesPath, $"{updater.DownloadUrl!.GetMD5()}.zip");
            var flag = await Update.DownloadWithProgress(updater.DownloadUrl!, file, (sender, e) =>
            {
                Console.WriteLine(e.ProgressPercentage);
            });
            if (!flag)
            {
                Console.WriteLine($"下载失败,是否已联网？？？,URL: {updater.DownloadUrl}");
                Console.ReadKey();
                return;
            }
            try
            {
                using var zip = new ZipArchive(File.OpenRead(file));
                Directory.CreateDirectory(Path.Combine(Utils.AppTempleFilesPath, "frpc"));
                var dir = new DirectoryInfo(Path.Combine(Utils.AppTempleFilesPath, "frpc"));
                var acl = dir.GetAccessControl(System.Security.AccessControl.AccessControlSections.Access);
                acl.SetAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null),
                    System.Security.AccessControl.FileSystemRights.FullControl,
                    System.Security.AccessControl.InheritanceFlags.ObjectInherit,
                    System.Security.AccessControl.PropagationFlags.None,
                    System.Security.AccessControl.AccessControlType.Allow));
                dir.SetAccessControl(acl);
                if (File.Exists(Path.Combine(Utils.AppTempleFilesPath, "frpc", $"{Utils.FrpcPlatForm}.exe")))
                {
                    File.Delete(Path.Combine(Utils.AppTempleFilesPath, "frpc", $"{Utils.FrpcPlatForm}.exe"));
                }

                zip.ExtractToDirectory(Path.Combine(Utils.AppTempleFilesPath, "frpc"));
                OfSettings.Instance.FRPClientVersion = updater.Content!;
                await OfSettings.Instance.WriteConfig();
                Process.Start(new ProcessStartInfo("explorer", Path.Combine(Utils.ApplicationPath, "OpenFrp.Launcher.exe")));
            }
            catch(Exception ex)
            {
                Console.WriteLine($"请将该日志发送到用户交流群内::: {ex}");
                Console.ReadKey();
            }

        }
    }
}