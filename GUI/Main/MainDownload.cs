using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    public partial class Main
    {
        private NetAsyncModulesDownloader? downloader;

        private void ModInfo_OnDownloadClick(GUIMod gmod)
        {
            StartDownload(gmod);
        }

        public void StartDownload(GUIMod? module)
        {
            if (module == null || !module.IsCKAN)
            {
                return;
            }

            ShowWaitDialog();
            if (downloader != null)
            {
                Task.Factory.StartNew(() =>
                {
                    // Just pass to the existing worker
                    downloader.DownloadModules(new List<CkanModule> { module.ToCkanModule() });
                });
            }
            else
            {
                // Start up a new worker
                Wait.StartWaiting(CacheMod, PostModCaching, true, module);
            }
        }

        [ForbidGUICalls]
        private void CacheMod(object? sender, DoWorkEventArgs? e)
        {
            if (e != null
                && e.Argument is GUIMod gm
                && Manager?.Cache != null)
            {
                downloader = new NetAsyncModulesDownloader(currentUser, Manager.Cache, userAgent);
                downloader.DownloadProgress += OnModDownloading;
                downloader.StoreProgress    += OnModValidating;
                downloader.OverallDownloadProgress += currentUser.RaiseProgress;
                Wait.OnCancel += downloader.CancelDownload;
                downloader.DownloadModules(new List<CkanModule> { gm.ToCkanModule() });
                e.Result = e.Argument;
            }
        }

        public void PostModCaching(object? sender, RunWorkerCompletedEventArgs? e)
        {
            if (downloader != null)
            {
                Wait.OnCancel -= downloader.CancelDownload;
                downloader = null;
            }
            // Can't access e.Result if there's an error
            if (e?.Error != null)
            {
                switch (e.Error)
                {

                    case CancelledActionKraken exc:
                        // User already knows they cancelled, get out
                        HideWaitDialog();
                        EnableMainWindow();
                        break;

                    default:
                        FailWaitDialog(Properties.Resources.DownloadFailed,
                                       e.Error.ToString(),
                                       Properties.Resources.DownloadFailed);
                        break;

                }
            }
            else
            {
                // Close progress tab and switch back to mod list
                HideWaitDialog();
                EnableMainWindow();
                ModInfo.SwitchTab("ContentTabPage");
            }
        }

        [ForbidGUICalls]
        private void UpdateCachedByDownloads(CkanModule? module)
        {
            var allGuiMods = ManageMods.AllGUIMods();
            var affectedMods =
                module?.GetDownloadsGroup(allGuiMods.Values
                                                    .Select(guiMod => guiMod.ToModule())
                                                    .OfType<CkanModule>())
                       .Select(other => allGuiMods[other.identifier])
                      ?? allGuiMods.Values;
            foreach (var otherMod in affectedMods)
            {
                otherMod.UpdateIsCached();
            }
        }

        [ForbidGUICalls]
        private void OnCacheChanged(NetModuleCache? prev)
        {
            if (prev != null)
            {
                prev.ModStored -= OnModStoredOrPurged;
                prev.ModPurged -= OnModStoredOrPurged;
            }
            if (Manager.Cache != null)
            {
                Manager.Cache.ModStored += OnModStoredOrPurged;
                Manager.Cache.ModPurged += OnModStoredOrPurged;
            }
            UpdateCachedByDownloads(null);
            ModInfo.RefreshModContentsTree();
        }

        [ForbidGUICalls]
        private void OnModStoredOrPurged(CkanModule? module)
        {
            UpdateCachedByDownloads(module);

            if (module == null
                || ModInfo.SelectedModule?.Identifier == module.identifier)
            {
                ModInfo.RefreshModContentsTree();
            }
        }
    }
}
