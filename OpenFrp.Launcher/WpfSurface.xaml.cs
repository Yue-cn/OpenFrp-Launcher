using OpenFrp.Core;
using OpenFrp.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.UI.WebUI;

namespace OpenFrp.Launcher
{
    /// <summary>
    /// WpfSurface.xaml 的交互逻辑
    /// </summary>
    public partial class WpfSurface : Window
    {
        public WpfSurface()
        {
            InitializeComponent();
        }
        private LauncherModel LauncherModel
        {
            get => (LauncherModel)DataContext;
            set => DataContext = value;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e); 

            OfApp_NavigationView.ItemInvoked += (s, e) =>
            {
                Type page;
                if (!e.IsSettingsInvoked)
                {
                    
                }
                page = typeof(Views.Setting);
                OfApp_RootFrame.Navigate(page);
            };
            OfApp_RootFrame.Navigating += (s, e) =>
            {
                OfApp_RootFrame.RemoveBackEntry();
            };

            // Defualt Pipe Server 
            // 服务端 单独发给 客户端，不需要客户端先发送请求。
            ServerPipeWorker();
            ClientPipeWorker();

        }

        private async void ClientPipeWorker()
        {
            // 抛弃旧版 Process 检测机制 仅连接失败（卡住）的时候 Kill 后开启。


            await OfAppHelper.PipeClient.Start();
            LauncherModel.PipeRunningState = true;

        }

        private void ServerPipeWorker()
        {
            OfAppHelper.PipeServer.Start(true);
            OfAppHelper.PipeServer.RequestFunction = (request, model) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    switch (request)
                    {
                        // 服务器关闭时 自动发出 Event
                        case Core.Pipe.PipeModel.OfAction.Server_Closed:
                            {
                                LauncherModel.PipeRunningState = false;
                                ClientPipeWorker();
                            }
                            break;
                    }
                });
                return new();
            };
        }
        


    }
}
