using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Novalogic.PFF;

namespace vsk
{
    /// <summary>
    ///     WinForm for displaying a PFF archive.
    /// </summary>
    public partial class FormArchive : Form
    {
        private readonly ListViewColumnSorter _lvwColumnSorter;
        private PffArchive _archive;

        private FileInfo _info;

        /// <summary>
        ///     Constructor.
        /// </summary>
        public FormArchive()
        {
            InitializeComponent();

            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            _lvwColumnSorter = new ListViewColumnSorter();
            listView.ListViewItemSorter = _lvwColumnSorter;
        }

        /// <summary>
        ///     The path to the currently loaded PFF Archive.
        /// </summary>
        public FileInfo ArchiveFile
        {
            set
            {
                _info = value;
                LoadArchive();
            }
        }

        private void LoadArchive()
        {
            if (_archive != null)
            {
                _archive.Dispose();
                _archive = null;
            }

            Text = "Archive - []";
            listView.Items.Clear();

            if (_info == null)
                return;

            Text = $"Archive - [{_info.FullName}]";

            _archive = PffArchive.Open(_info);
            var entries = _archive.Entries;

            listView.BeginUpdate();
            foreach (var entry in entries)
            {
                var name = entry.FilePath;
                var lvi = new ListViewItem(name);
                lvi.SubItems.Add(entry.PackedTimeUtc.ToLocalTime().ToString(CultureInfo.CurrentCulture));
                lvi.SubItems.Add(name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1
                    ? ""
                    : Path.GetExtension(entry.FilePath)?.Substring(1));
                lvi.SubItems.Add(entry.FileSize.ToString());

                listView.Items.Add(lvi);
            }
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView.EndUpdate();

            ClientSize = new Size(listView.PreferredWidth(), ClientSize.Height);
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != _lvwColumnSorter.SortColumn)
            {
                _lvwColumnSorter.SortColumn = e.Column;
                _lvwColumnSorter.Order = SortOrder.Ascending;
            }
            else
                _lvwColumnSorter.Order = _lvwColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;

            listView.Sort();
            listView.SetSortIcon(_lvwColumnSorter.SortColumn, _lvwColumnSorter.Order);
        }

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var listViewItem = listView.HitTest(e.Location).Item;
            if (listViewItem == null)
                return;

            Program.MainForm.OpenFilePreview(_archive.GetEntry(listViewItem.Text));
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var archiveName = Path.GetFileNameWithoutExtension(_archive.FileInfo.Name);
            var extractDir = !string.IsNullOrWhiteSpace(archiveName) ? archiveName : "EXTRACTED";
            Directory.CreateDirectory(extractDir);

            foreach (ListViewItem item in listView.SelectedItems)
            {
                var entry = _archive.GetEntry(item.Text);
                if (entry != null)
                {
                    var filePath = Path.Combine(extractDir, entry.FilePath);
                    var contents = entry.GetContents();
                    if (contents != null)
                    {
                        File.WriteAllBytes(filePath, contents);
                        File.SetLastWriteTime(filePath, entry.PackedTimeUtc.ToLocalTime());
                    }
                    else
                        throw new Exception($"File {entry.FilePath} exists in archive, but file contents are empty?");
                }
            }
        }
    }
}