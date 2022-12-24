using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class LauncherModel : ObservableRecipient
    {
        /// <summary>
        /// 管道运行状态 (实际请改用<see cref="PipeRunningState"/>)
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public bool pipeRunningState;

        [RelayCommand]
        void GC_Collet()
        {
            GC.Collect(0,GCCollectionMode.Optimized);
            
        }
    }
}
