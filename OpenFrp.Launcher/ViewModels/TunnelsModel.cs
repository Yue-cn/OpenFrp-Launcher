using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrp.Core;
using OpenFrp.Core.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using OpenFrp.Launcher.Controls;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class TunnelsModel : ObservableObject
    {

        public TunnelsModel()
        {
            if (OfAppHelper.LauncherViewModel is not null)
            {
                OfAppHelper.LauncherViewModel.PropertyChanged += (sender, e) =>
                {
                    OnPropertyChanged("PipeRunningState");
                };
            }
        }


        public bool PipeRunningState
        {
            get => OfAppHelper.LauncherViewModel?.PipeRunningState ?? false;
        }


        #region Tunnel Page
        [ObservableProperty]
        public ObservableCollection<OpenFrp.Core.Api.OfApiModel.Response.UserTunnelModel.UserTunnel> userTunnels = new();

        [ObservableProperty]
        public bool isEnableTool;

        public Views.Tunnels? MainPage { get; set; }

        internal bool isRefreshing = false;

        [RelayCommand]
        void Refresh()
        {
            if (!isRefreshing)
            {
                isRefreshing = true;
                IsEnableTool = false;
                MainPage?.RefreshUserTunnels();
            }


        }

        [RelayCommand]
        private void CopyLink(string url)
        {
            try { Clipboard.SetText(url); }
            catch { }
        }

        [RelayCommand]
        private async void RemoveTunnel(object inter)
        {
            if (!hasDialog)
            {
                hasDialog = true;
                int id = Convert.ToInt32(inter);
                var dialog = new ContentDialog()
                {
                    Content = "真的要移除此隧道吗?",
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary,
                    Title = "移除隧道"
                };
                var res = await dialog.ShowAsync();
                if (res == ContentDialogResult.Primary)
                {
                    var resp = await OfApi.RemoveProxy(id);
                    if (resp.Flag)
                    {
                        for (int i = 0; i < UserTunnels.Count; i++)
                        {
                            if (UserTunnels[i].TunnelId == id)
                            {
                                UserTunnels.RemoveAt(i);
                                (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.Navigate(typeof(Views.Tunnels));
                                return;
                            }
                        }
                    }
                }
                hasDialog = false;
            }
        }

        [RelayCommand]
        private async void EditTunnel(Core.Api.OfApiModel.Response.UserTunnelModel.UserTunnel proxy)
        {
            var config = new Core.Api.OfApiModel.Request.EditTunnelData()
            {
                BindDomain = proxy.Domains,
                CustomArgs = proxy.CustomArgs ?? "",
                EncryptMode = proxy.EncryptionMode,
                GZipMode = proxy.ComperssionMode,
                HostRewrite = proxy.HostRewrite,
                LocalAddress = proxy.LocalAddress,
                LocalPort = proxy.LocalPort,
                NodeID = proxy.NodeID,
                TunnelName = proxy.TunnelName,
                TunnelType = proxy.TunnelType,
                RemotePort = proxy.RemotePort,
                URLRoute = proxy.URLRoute,
                RequestFrom = proxy.RequestFrom,
                RequestPass = proxy.RequestPassword,
            };
            if (!hasDialog)
            {
                hasDialog = true;


                var configControl = new TunnelConfig()
                {
                    isCreating = false,
                };
                configControl.SetConfig(config);



                var loader = new ElementLoader()
                {
                    Content = new ScrollViewerEx()
                    {
                        Content = configControl,
                        Padding = new(0, 0, 12, 0),
                    },
                    MaxHeight = 350
                };
                ((ScrollViewerEx)loader.Content).PreviewMouseWheel += (sender, args) =>
                {
                    ((ScrollViewerEx)loader.Content).ExcuteScroll(args);
                };



                var dialog = new ContentDialog()
                {
                    Title = "编辑隧道",
                    Content = loader,
                    PrimaryButtonText = "提交",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };
                dialog.PrimaryButtonClick += async (sender, req) =>
                {
                    loader.Focus();
                    
                    dialog.IsPrimaryButtonEnabled = !(req.Cancel = true);
                    await Task.Delay(150);
                    loader.ShowLoader();
                    var config = configControl.GetConfig(true);
                    config.TunnelID = proxy.TunnelId;
                    config.TunnelType = config.TunnelType?.ToLower();
                    var resp = await OfApi.EditProxy(config);
                    if (resp.Flag)
                    {
                        (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.Navigate(typeof(Views.Tunnels));
                        dialog.Hide();return;
                    }
                    loader.ShowError();
                    loader.PushMessage(loader.ShowContent, resp.Message, "编辑");
                    dialog.IsPrimaryButtonEnabled = true;
                };
                await dialog.ShowAsync();

                hasDialog = false;
            }
        }

        private bool hasDialog = false;



        [RelayCommand]
        internal void ToCreatePage() => (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.Navigate(typeof(Views.CreateTunnel));
        #endregion
    }
}
