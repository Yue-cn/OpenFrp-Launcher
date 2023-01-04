using OpenFrp.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.ApplicationModel.VoiceCommands;

namespace OpenFrp.Launcher.Controls
{
    public partial class TunnelConfig : UserControl
    {



        public bool isCreating
        {
            get { return (bool)GetValue(isCreatingProperty); }
            set { SetValue(isCreatingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for isCreating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty isCreatingProperty =
            DependencyProperty.Register("isCreating", typeof(bool), typeof(TunnelConfig), new PropertyMetadata(true));



        public Core.Api.OfApiModel.Request.EditTunnelData ProxyConfig
        {
            get { return (Core.Api.OfApiModel.Request.EditTunnelData)GetValue(ProxyConfigProperty); }
            set { SetValue(ProxyConfigProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProxyConfig.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProxyConfigProperty =
            DependencyProperty.Register("ProxyConfig", typeof(Core.Api.OfApiModel.Request.EditTunnelData), typeof(TunnelConfig), new PropertyMetadata(new Core.Api.OfApiModel.Request.EditTunnelData() { }));




        public Core.Api.OfApiModel.Response.NodesModel.NodeInfo NodeInfo
        {
            get { return (Core.Api.OfApiModel.Response.NodesModel.NodeInfo)GetValue(NodeInfoProperty); }
            set { SetValue(NodeInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NodeInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NodeInfoProperty =
            DependencyProperty.Register("NodeInfo", typeof(Core.Api.OfApiModel.Response.NodesModel.NodeInfo), typeof(TunnelConfig), new PropertyMetadata());

        public Core.Api.OfApiModel.Request.EditTunnelData GetConfig(bool isEditMode = false)
        {
            if (NodeInfo is not null || isEditMode)
            {
                if (!isEditMode)
                {
                    string item = ((ComboBoxItem)((ComboBox)GetTemplateChild("Of_Protocol_ComboBox")).SelectedItem).Content.ToString();
                    ProxyConfig.TunnelType = item.ToLower();


                    var bo1x = (NumberBox)GetTemplateChild("Of_RemotePort_Box");
                    if (bo1x.Value is double.NaN or 0)
                    {
                        if (NodeInfo is not null)
                        {
                            if (NodeInfo.MinimumPort != NodeInfo.MaxumumPort)
                            {
                                bo1x.Value = (ProxyConfig.RemotePort = new Random().Next(NodeInfo.MinimumPort, NodeInfo.MaxumumPort)).Value;
                            }
                            else
                            {
                                bo1x.Value = (ProxyConfig.RemotePort = new Random().Next(1000, 65535)).Value;
                            }
                        }
                        else
                        {
                            bo1x.Value = (ProxyConfig.RemotePort = new Random().Next(1000, 65535)).Value;
                        }
                    }
                    else
                    {
                        bo1x.Value = (ProxyConfig.RemotePort = (int)Math.Round(bo1x.Value)).Value;
                    }
                }



                var bo2x = (NumberBox)GetTemplateChild("Of_LocalPort_Box");
                if (bo2x.Value is double.NaN or 0)
                {
                    bo2x.Value = ProxyConfig.LocalPort = 25565;
                }
                else
                {
                    bo2x.Value = ProxyConfig.LocalPort = (int)Math.Round(bo2x.Value);
                }

                ProxyConfig.NodeID = NodeInfo?.NodeID ?? 0;

                ProxyConfig.Session = OfApi.Session;

                var custom = new StringBuilder();
                if (ProxyConfig.ProxyProtocolVersion is not 0)
                {
                    custom.Append($"proxy_protocol_version = {(ProxyConfig.ProxyProtocolVersion is 1 ? "v1" : "v2")} \n");
                }
                var custom_inp = (TextBox)GetTemplateChild("Of_CustomArgs_Box");

                custom.Append(custom_inp.Text.Replace("\\r\\n","\\n"));

                ProxyConfig.CustomArgs = custom.ToString();

                if (string.IsNullOrEmpty(ProxyConfig.TunnelName))
                {
                    ProxyConfig.TunnelName = ((TextBox)GetTemplateChild("Of_TunnelName_Box")).Text =
                    $"OfApp_{new Random().Next(25565, 89889)}";
                };
            }
            return ProxyConfig;

        }



        public void SetRemotePort(int? port)
        {
            var bo1x = (NumberBox)GetTemplateChild("Of_RemotePort_Box");
            if (bo1x is not null && port is not null) bo1x.Value = port.Value;
        }
        public void SetLocalPort(int port)
        {
            var bo1x = (NumberBox)GetTemplateChild("Of_LocalPort_Box");
            if (bo1x is not null) bo1x.Value = port;
        }
        public void SetLocalAddress(string str)
        {
            var bo1x = (TextBox)GetTemplateChild("Of_TunnelName_Box");
            if (bo1x is not null) ProxyConfig.LocalAddress = str;
        }

        public async void SetConfig(Core.Api.OfApiModel.Request.EditTunnelData createProxy)
        {
            ProxyConfig = createProxy;
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(createProxy.CustomArgs))
            {
                var args = createProxy.CustomArgs.Split(new char[] { '\n' },StringSplitOptions.RemoveEmptyEntries);
               
                foreach (var arg in args)
                {
                    var args2 = arg.Split('=');
                    if (args2.FirstOrDefault().Replace(" ", "") is "proxy_protocol_version")
                    {
                        ProxyConfig.ProxyProtocolVersion = args2.LastOrDefault().Replace(" ", "") switch
                        {
                            "v1" => 1,
                            _ => 2
                        };
                        continue;
                    }
                    
                    builder.Append(arg + "\n");
                    
                }
                
            }

            await Task.Delay(500);

            SetLocalPort(createProxy.LocalPort);
            SetRemotePort(createProxy.RemotePort);
            SetLocalAddress(createProxy.LocalAddress ?? "127.0.0.1");
            if (GetTemplateChild("Of_Protocol_ComboBox") is ComboBox box)
            {
                box.SelectedIndex = 0;
            }
            if (GetTemplateChild("Of_CustomArgs_Box") is TextBox custom_inp)
            {
                custom_inp.Text = ProxyConfig.CustomArgs = builder.ToString();
            }
                
        }
    }
}
