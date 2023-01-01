using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrp.Core.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class TunnelsModel : ObservableObject
    {
        [ObservableProperty]
        public bool isEnableTool;

        public SettingModel SettingModel
        {
            get => OfAppHelper.SettingViewModel;
        }

        [ObservableProperty]
        public ObservableCollection<OpenFrp.Core.Api.OfApiModel.Response.UserProxiesModel.UserProxies> userProxies = new();

        internal bool isRefreshing = false;

        [RelayCommand]
        void Refresh(Views.Tunnels page)
        {
            if (!isRefreshing)
            {
                isRefreshing = true;
                IsEnableTool = false;
                page.RefreshUserTunnels();
            }


        }

        [RelayCommand]
        void CopyLink(string url)
        {
            try { Clipboard.SetText(url); }
            catch { }
        }

        [RelayCommand]
        async void RemoveProxy(object inter)
        {
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
                    for (int i = 0; i < UserProxies.Count; i++)
                    {
                        if (UserProxies[i].ProxyId == id)
                        {
                            UserProxies.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }

        [RelayCommand]
        void ToCreatePage()
        {
            (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.Navigate(typeof(Views.CreateTunnel));
        }
        [RelayCommand]
        void ToMainPage()
        {
            (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.GoBack();
        }
    }
}
