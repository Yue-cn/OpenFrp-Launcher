using Newtonsoft.Json;
using OpenFrp.Core.Api;
using OpenFrp.Core.Api.OfApiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace OpenFrp.Core.App
{
    public class Update
    {
        public static async ValueTask<UpdateInfo> CheckUpdate()
        {
            var resp = await OfApi.GET<Response.UpdateModel>("https://of-dev-api.bfsea.xyz/commonQuery/get?key=software") ?? new();
            if (resp.Flag && resp.Data is not null)
            {
                if (resp.Data?.LatestVersion is null)
                {
                    return new UpdateInfo(UpdateFor.None);
                }
                else if (resp.Data.LauncherInfo.LatestVersion?.GetMD5() != Utils.ApplicationVersions.GetMD5())
                {
                    // 启动器有新版本
                    return new UpdateInfo(UpdateFor.Launcher, resp.Data.LauncherInfo.Content, resp.Data.LauncherInfo.DownloadUrl);
                }
                else if (resp.Data.LatestVersion.GetMD5() != OfSettings.Instance.FRPClientVersion.GetMD5())
                {
                    // FRPC 有更新
                    return new UpdateInfo(UpdateFor.FRPC, resp.Data.LatestVersion, $"https://obs.cstcloud.cn/share/obs/zgitnetwork/ofclient/{resp.Data.LatestVersion}/{Utils.FrpcPlatForm}.zip");
                }
            }
            return new UpdateInfo(UpdateFor.None,"请求失败啦啦啦啦API - OpenFrp");
        }

        public static async ValueTask<bool> DownloadWithProgress(string url,string file,DownloadProgressChangedEventHandler onChanged)
        {
            try
            {
                var client = new WebClient();
                if (OfSettings.Instance.BypassProxy)
                {
                    client.Proxy = null;
                }
                client.DownloadProgressChanged += onChanged;
                await client.DownloadFileTaskAsync(url, file);
                return true;
            }
            catch { }
            return false;
        }

        public class UpdateInfo
        {
            public UpdateInfo(UpdateFor updateFor, string? content = null, string? url = null)
            {
                Content = content;
                UpdateFor = updateFor;
                DownloadUrl = url;
            }

            public string? Content { get; set; }
            public UpdateFor UpdateFor { get; set; }
            public string? DownloadUrl { get; set; }
        }
        public enum UpdateFor
        {
            None,
            FRPC,
            Launcher,
        }
    }
}
