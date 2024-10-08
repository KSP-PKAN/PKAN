using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class GameCommandLineOptionsDialog : Form
    {
        public GameCommandLineOptionsDialog()
        {
            InitializeComponent();
            if (Platform.IsMono)
            {
                // Mono's DataGridView has showstopper bugs with AllowUserToAddRows,
                // so use an Add button instead
                CmdLineGrid.AllowUserToAddRows = false;
                AddButton.Visible = true;
            }
        }

        public DialogResult ShowGameCommandLineOptionsDialog(IWin32Window parent,
                                                             List<string> cmdLines,
                                                             string[] defaults)
        {
            rows = cmdLines.Select(cmdLine => new CmdLineRow(cmdLine))
                           .ToList();
            CmdLineGrid.DataSource = new BindingList<CmdLineRow>(rows)
                                     {
                                         AllowEdit   = true,
                                         AllowRemove = true,
                                     };
            this.defaults = defaults;
            return ShowDialog(parent);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Edit the top cell immediately for convenience
            CmdLineGrid.BeginEdit(false);
        }

        public List<string> Results => rows?.Select(row => row.CmdLine)
                                            .Where(str => !string.IsNullOrEmpty(str))
                                            .ToList()
                                           ?? new List<string>();

        private void CmdLineGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Don't auto-select the text when the user clicks on it
            if (e.Control is DataGridViewTextBoxEditingControl tbec)
            {
                BeginInvoke(new Action(() =>
                {
                    tbec.SelectionLength = 0;
                }));
            }
        }

        private void CmdLineGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            // You can't delete the last row
            if (rows?.Count == 1)
            {
                e.Cancel = true;
            }
        }

        private void ResetToDefaultsButton_Click(object? sender, EventArgs? e)
        {
            rows = defaults?.Select(cmdLine => new CmdLineRow(cmdLine))
                            .ToList()
                           ?? new List<CmdLineRow>();
            CmdLineGrid.DataSource = new BindingList<CmdLineRow>(rows)
                                     {
                                         AllowEdit   = true,
                                         AllowRemove = true,
                                     };
        }

        private void AddButton_Click(object? sender, EventArgs? e)
        {
            (CmdLineGrid.DataSource as BindingList<CmdLineRow>)?.AddNew();
            if (//CmdLineGrid.Rows is [.., var last]
                //&& last.Cells is [var first, ..]
                CmdLineGrid.Rows.Count > 0
                && CmdLineGrid.Rows[^1] is DataGridViewRow last
                && last.Cells.Count > 0
                && last.Cells[0] is var first)
            {
                CmdLineGrid.CurrentCell = first;
            }
            CmdLineGrid.BeginEdit(false);
        }

        private void AcceptChangesButton_Click(object? sender, EventArgs? e)
        {
            if (Results.Count < 1)
            {
                // Don't accept an empty grid (shouldn't happen because of row deletion limit above)
                DialogResult = DialogResult.None;
            }
        }

        private string[]?         defaults;
        private List<CmdLineRow>? rows;
    }

    public class CmdLineRow
    {
        // Called when the user clicks on an empty row
        public CmdLineRow()
        {
            CmdLine = "";
        }

        public CmdLineRow(string cmdLine)
        {
            CmdLine = cmdLine;
        }

        public string CmdLine { get; set; }
    }
}
