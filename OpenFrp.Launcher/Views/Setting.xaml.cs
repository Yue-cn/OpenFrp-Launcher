﻿using OpenFrp.Core.App;
using System;
using System.Collections.Generic;
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
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Page
    {
        public Setting()
        {
            InitializeComponent();
            this.DataContext = OfAppHelper.SettingViewModel;
            this.Unloaded += async (sender, args) =>
            {
                var sb = new OfSettings();
                sb = OfSettings.Instance;
                sb.AutoRunTunnel = null!;
                await OfAppHelper.PipeClient.PushMessageAsync(new()
                {
                    Action = Core.Pipe.PipeModel.OfAction.Push_Config,
                    Config = sb
                });
            };
        }

    }
}
