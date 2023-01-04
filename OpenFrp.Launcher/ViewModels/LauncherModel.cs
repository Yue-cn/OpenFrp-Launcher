using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrp.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class LauncherModel : ObservableObject
    {
        /// <summary>
        /// 管道运行状态 (实际请改用<see cref="PipeRunningState"/>)
        /// </summary>
        [ObservableProperty]
        public bool pipeRunningState;

        [ObservableProperty]
        public bool isFrpchasUpdate;

        [RelayCommand]
        async void InstallFrpc()
        {
            var dialog = new ContentDialog()
            {
                Content = "FRPC安装包会关闭FRPC进程,请保证您不在使用远程桌面服务!!!",
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = "安装",
                CloseButtonText = "取消",
                Title = "安装"
            };
            if(await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if ((await OfAppHelper.PipeClient.PushMessageAsync(new()
                {
                    Action = Core.Pipe.PipeModel.OfAction.Close_Server,
                    Message = "关闭。"
                })).Action == Core.Pipe.PipeModel.OfAction.Server_Closed)
                {
                    if (OfAppHelper.LauncherViewModel is not null)
                        OfAppHelper.LauncherViewModel.PipeRunningState = false;
                }
                Process.Start(new ProcessStartInfo(Utils.CorePath, "--frpcp")
                {
                    UseShellExecute = false,
                    Verb = "runas"
                });
                Environment.Exit(0);
            }
            
        }
    }
}
