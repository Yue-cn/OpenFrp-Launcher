using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OpenFrp.Core;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Windows.Data;
using System.Globalization;
using OpenFrp.Core.Api;
using OpenFrp.Launcher.Controls;
using System.Windows.Controls;
using ListView = ModernWpf.Controls.ListView;
using System.Windows.Documents;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class CreateTunnelsModel : ObservableObject
    {
        #region CreatetTunnel Page
        /// <summary>
        /// 自适应-小 模式
        /// </summary>
        [ObservableProperty]
        public bool adaptiveSmall;
        /// <summary>
        /// 转主页面
        /// </summary>
        [RelayCommand]
        void ToMainPage() => (App.Current.MainWindow as WpfSurface)?.OfApp_RootFrame.GoBack();

        /// <summary>
        /// 是否正在监听
        /// </summary>
        private bool isLanLisening = false;

        /// <summary>
        /// 获得 Minecraft LAN 列表
        /// </summary>
        [RelayCommand]
        async void RefreshLanList(Controls.TunnelConfig page)
        {
            CancellationTokenSource _sourec = new();
            if (!isLanLisening)
            {
                isLanLisening = true;
                try
                {
                    using var uc = new UdpClient(4445);
                    uc.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"));

                    UdpReceiveResult response = await uc.ReceiveAsync().WithCancelToken(_sourec.Token);
                    if (response.Buffer is not null)
                    {
                        var addresss = await Dns.GetHostAddressesAsync(Dns.GetHostName()).WithCancelToken(_sourec.Token);
                        if (addresss is not null)
                        {
                            foreach (var address in addresss)
                            {
                                if (address.ToString() == response.RemoteEndPoint.Address.ToString())
                                {
                                    page.SetLocalPort(Convert.ToInt32(response.Buffer.GetString(false).Split('[')[3].Split(']')[1]));
                                    //page.Input_Port_OiJKV1QiLCJhbGc.Text = (Config.LocalPort = ;
                                    //page.Input_Address_OiJKV1QiLCJhbGc.Text = Config.LocalAddress = "localhost";
                                }
                            }


                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                }
                isLanLisening = false;

            }

        }


        [RelayCommand]
        async void ListHosting(Controls.TunnelConfig page)
        {
            if (!hasDialog)
            {
                hasDialog = false;

                var loader = new ElementLoader()
                {
                    IsLoading = true
                };
                var dialog = new ContentDialog()
                {
                    Title = "列表 (双击选择)",
                    Content = loader,
                    CloseButtonText = "关闭",
                };
                dialog.Loaded += async (sender, args) =>
                {
                    var links = (await Utils.GetAliveNetworkLink()).ToList();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        var view = new ListView()
                        {
                            Width = 350,
                            MaxHeight = 300,
                            ItemsSource = links,
                            ItemTemplate = (DataTemplate)App.Current.Resources["ProcessListTemplate"]
                        };
                        loader.Content = view;
                        view.MouseDoubleClick += (sender, args) =>
                        {
                            if (view.SelectedIndex is not -1)
                            {
                                var item = links[view.SelectedIndex];
                                page.SetLocalPort(item.Port);
                                dialog.Hide();
                            }

                        };

                    });
                    



                    loader.IsLoading = false;
                };
                await dialog.ShowAsync();
                hasDialog = false;
            }

        }


        private bool hasDialog = false;

        [RelayCommand]
        async void Sumbit(Views.CreateTunnel page)
        {
            
            if (!hasDialog)
            {
                hasDialog = true;

                Config = page.Of_TunnelConfig.GetConfig();

                var loader = new ElementLoader() { IsLoading = true };

                var dialog = new ContentDialog()
                {
                    PrimaryButtonText = "确定",
                    IsPrimaryButtonEnabled = false,
                    Title = "创建隧道",
                    Content = loader
                };
                dialog.Loaded += async (sender, args) =>
                {
                    var resp = await OfApi.CreateProxy(Config);
                    dialog.IsPrimaryButtonEnabled = true;
                    if (resp.Flag) { dialog.Hide(); ToMainPage(); }
                    else
                    {
                        loader.IsLoading = false;

                        loader.Content = new TextBlock(new Run(resp.Message))
                        {
                            TextWrapping = TextWrapping.Wrap
                        };
                    }
                };
                await dialog.ShowAsync();
                hasDialog = false;
            }



        }


        /// <summary>
        /// 用户信息 (Model Refer Form: <see cref="OfAppHelper.UserInfoModel"/>)
        /// </summary>
        public OpenFrp.Core.Api.OfApiModel.Response.UserInfoModel.UserInfoDataModel UserInfoData
        {
            get => OfApi.UserInfoDataModel!;
            set => OfApi.UserInfoDataModel = value;
        }

        [ObservableProperty]
        public Core.Api.OfApiModel.Request.EditTunnelData config = new();

        [ObservableProperty]
        public ObservableCollection<Core.Api.OfApiModel.Response.NodesModel.NodeInfo> nodesList = new();
        #endregion




    }

    public class DomainsStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string[] strs)
            {
                if (strs.Length > 0)
                {
                    var builder = new StringBuilder();

                    foreach (var str in strs)
                    {
                        builder.Append(str + ",");

                    }
                    return builder.ToString().Remove(builder.Length - 1);
                }
                return "";
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str.Split(',');
            }
            return new string[0];
        }
    }



}