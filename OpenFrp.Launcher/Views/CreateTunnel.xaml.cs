using OpenFrp.Core.Api;
using OpenFrp.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.ApplicationModel.Activation;


namespace OpenFrp.Launcher.Views
{
    /// <summary>
    /// CreateTunnel.xaml 的交互逻辑
    /// </summary>
    public partial class CreateTunnel : Page
    {
        public CreateTunnelsModel TunnelsModel
        {
            get => (CreateTunnelsModel)DataContext;
            set => DataContext = value;
        }

        public CreateTunnel() => InitializeComponent();

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            RefreshNodeList();
            TunnelsModel.Config = Of_TunnelConfig.ProxyConfig =  new();
        }
        private async void RefreshNodeList()
        {
            Of_CreateTunnel_Loader.ShowLoader();
            var resp = await OfApi.GetNodesList();
            if (resp.Flag)
            {
                var nList1 = new List<Core.Api.OfApiModel.Response.NodesModel.NodeInfo>() { new()
                {
                    NodeName = "国内节点",
                    Status = 404,
                    isHeader = true
                } };
                var nList2 = new List<Core.Api.OfApiModel.Response.NodesModel.NodeInfo>() { new()
                {
                    NodeName = "中国香港 / 中国台湾 节点",
                    Status = 404,
                    isHeader = true
                } };
                var nList3 = new List<Core.Api.OfApiModel.Response.NodesModel.NodeInfo>() { new()
                {
                    NodeName = "国外节点",
                    Status = 404,
                    isHeader = true
                } };

                resp.Data.Nodes.ForEach((item) =>
                {
                    (item.NodeClassify switch
                    {
                        Core.Api.OfApiModel.Response.NodesModel.NodeClassify.ChinaMainland => nList1,
                        Core.Api.OfApiModel.Response.NodesModel.NodeClassify.ChinaTW_HK => nList2,
                        _ => nList3,
                    }).Add(item);
                });
                nList1.AddRange(nList2);
                nList1.AddRange(nList3);

                TunnelsModel.NodesList = new(nList1);

                
                
                Of_CreateTunnel_Loader.ShowContent();
            }
            else
            {
                Of_CreateTunnel_Loader.ShowError();
                Of_CreateTunnel_Loader.PushMessage(RefreshNodeList, $"加载出错了嬲,错误信息:{resp.Message}", "重试");
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var grid = (Grid)sender;
            if (TunnelsModel.AdaptiveSmall = !(grid.ActualWidth > 735))
            {
                // < 735
             }
            else
            {

            }

        }
    }
}
