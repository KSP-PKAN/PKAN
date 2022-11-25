using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN.GUI
{
    public partial class Main
    {
        private void UpdateChangesDialog(List<ModChange> changeset)
        {
            Changeset.LoadChangeset(
                changeset,
                ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                    .Where(l => l.AlertOnInstall)
                    .ToList());
        }

        private void Changeset_OnSelectedItemsChanged(ListView.SelectedListViewItemCollection items)
        {
            ShowSelectionModInfo(items);
        }

        private void Changeset_OnCancelChanges(bool reset)
        {
            if (reset)
            {
                ManageMods.ClearChangeSet();
                UpdateChangesDialog(null);
            }
            tabController.ShowTab("ManageModsTabPage");
        }

        private void Changeset_OnConfirmChanges(List<ModChange> changeset)
        {
            DisableMainWindow();
            try
            {
                Wait.StartWaiting(InstallMods, PostInstallMods, true,
                    new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                        changeset
                            .Where(change =>
                                // Skip dependencies so auto-installed checkbox is set
                                !(change.Reasons.Any(reason =>
                                    reason is SelectionReason.Depends)))
                            .ToList(),
                        RelationshipResolver.DependsOnlyOpts()));
            }
            catch (InvalidOperationException)
            {
                // Thrown if it's already busy, can happen if the user double-clicks the button. Ignore it.
                // More thread-safe than checking installWorker.IsBusy beforehand.
            }
        }
    }
}
