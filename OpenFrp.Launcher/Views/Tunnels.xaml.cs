using CommunityToolkit.Mvvm.ComponentModel;
using OpenFrp.Core.Api;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenFrp.Launcher.Views
{
    /// <summary>
    /// Tunnels.xaml 的交互逻辑
    /// </summary>
    public partial class Tunnels : Page
    {
        public TunnelsModel TunnelsModel
        {
            get => (TunnelsModel)DataContext;
            set => DataContext = value;
        }
        public Tunnels() => InitializeComponent();

        private void GridView_PreviewMouseWheel(object sender, MouseWheelEventArgs e) => Of_Tunnels_BaseView.ExcuteScroll(e);

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            RefreshUserTunnels();
            TunnelsModel.MainPage = this;
        }
        internal async void RefreshUserTunnels()
        {
            Of_Tunnels_ListLoader.ShowLoader();
            TunnelsModel.UserTunnels?.Clear();
            var lp = await OfApi.GetUserProxies();
            if (!lp.Flag)
            {
                Of_Tunnels_ListLoader.ShowError();
                if (lp.Message.Contains("token已过期"))
                {   
                    if (!await OfAppHelper.RequestLogin(false))
                    {
                        Of_Tunnels_ListLoader.ShowError();
                        Of_Tunnels_ListLoader.PushMessage(RefreshUserTunnels, $"Token失效后重请求失败,请尝试重新登录账户", "重试");
                    }
                    else Of_Tunnels_ListLoader.ShowLoader();
                }
                if (!OfApi.LoginState)
                {
                    Of_Tunnels_ListLoader.PushMessage(() =>
                    {
                        (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.Navigate(typeof(Views.Setting));
                        var item = ((App.Current.MainWindow as WpfSurface)?.OfApp_NavigationView.SettingsItem as NavigationViewItem);
                        if (item is not null) item.IsSelected = true;
                    }, "登录后即可查看。", "登录");
                }
                else
                {
                    
                    Of_Tunnels_ListLoader.PushMessage(RefreshUserTunnels, lp.Message, "重试");
                }

                TunnelsModel.isRefreshing = false;
                return;
            }
            if (lp.Data.Count is 0)
            {
                TunnelsModel.isRefreshing = false;
                TunnelsModel.IsEnableTool = true;
                Of_Tunnels_ListLoader.ShowError();
                Of_Tunnels_ListLoader.PushMessage(TunnelsModel.ToCreatePage, "你还没有一个隧道,创建一个就能看见啦。","创建");
                return;
            }
            if (OfAppHelper.RunningIds.Count != 0)
            {
                lp.Data.List.ForEach((item) =>
                {
                    if (OfAppHelper.RunningIds.Contains(item.TunnelId))
                    {
                        item.isRuuning = true;
                    }
                });
            }

            TunnelsModel.UserTunnels = new(lp.Data.List);
            TunnelsModel.isRefreshing = false;
            TunnelsModel.IsEnableTool = true;
            Of_Tunnels_ListLoader.ShowContent();
        }

        private bool isChanging { get; set; }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {

            ToggleSwitch switcher = (ToggleSwitch)sender;
            var proxy = (Core.Api.OfApiModel.Response.UserTunnelModel.UserTunnel)switcher.DataContext;
            e.Handled = false;
            if (isChanging) return;
            isChanging = true;
            if (switcher.IsOn)
            {
                isChanging = true;
                var resp = await OfAppHelper.PipeClient.PushMessageAsync(new()
                {
                    Action = Core.Pipe.PipeModel.OfAction.Start_Frpc,
                    FrpMessage = new()
                    {
                        Tunnel = proxy
                    }
                });

                if (resp.Message.IndexOf("Unlogin") != -1)
                {
                    OfAppHelper.RestartProcess();
                }

                // PUSH Server
                if (resp.Flag)
                {
                    switcher.IsOn = true;
                    if (!OfAppHelper.RunningIds.Contains(proxy.TunnelId))
                        OfAppHelper.RunningIds.Add(proxy.TunnelId);
                }
                else
                {
                    switcher.IsOn = false;
                    MessageBox.Show(resp.Message);
                }

                isChanging = false;
            }
            // 关闭
            else
            {
                var resp = await OfAppHelper.PipeClient.PushMessageAsync(new()
                {
                    Action = Core.Pipe.PipeModel.OfAction.Close_Frpc,
                    FrpMessage = new()
                    {
                        Tunnel = proxy
                    }
                });
                if (resp.Message.IndexOf("Unlogin") != -1)
                {
                    OfAppHelper.RestartProcess();
                }
                if (resp.Flag)
                {
                    switcher.IsOn = false;
                    if (OfAppHelper.RunningIds.Contains(proxy.TunnelId))
                        OfAppHelper.RunningIds.Remove(proxy.TunnelId);
                }
                else
                {
                    switcher.IsOn = true;
                    MessageBox.Show(resp.Message);
                }
                isChanging = false;
            }

        }

    }
}
