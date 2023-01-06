using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFrp.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Launcher.ViewModels
{
    public partial class LogModel : ObservableObject
    {
        [ObservableProperty]
        public Dictionary<string, List<LogsModel>> logsList = new();

        public List<LogsModel> LogValue
        {
            get
            {
                if (LogsList.ContainsKey(SelectedLog))
                {
                    return LogsList[SelectedLog];
                }
                return new();
            }

        }
        [RelayCommand]
        async internal void RefreshList(Views.Logs page)
        {
            var resp = await OfAppHelper.PipeClient.PushMessageAsync(new()
            {
                Action = Core.Pipe.PipeModel.OfAction.Get_Logs
            });
            if (resp.LogMessage?.LogsList is null || resp.LogMessage?.LogsList.Count == 0) return;
            LogsList = resp.LogMessage?.LogsList!;

            page.Items.GetBindingExpression(ItemsRepeater.ItemsSourceProperty)?.UpdateTarget();
        }


        public OfSettings.ConsoleModel ConsoleWrapper
        {
            get => OfSettings.Instance.Console;
            set => OfSettings.Instance.Console = value;
        }

        [ObservableProperty]
        public string selectedLog = "";
    }
}
