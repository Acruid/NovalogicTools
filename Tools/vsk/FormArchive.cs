using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Novalogic.Archive;

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

            _archive = PffArchive.Open(_info);
            var entries = _archive.Entries;

            listView.Items.Clear();
            listView.BeginUpdate();
            foreach (var entry in entries)
            {
                var name = entry.FilePath;
                var lvi = new ListViewItem(name);
                lvi.SubItems.Add(entry.PackedTimeUTC.ToLocalTime().ToString(CultureInfo.CurrentCulture));
                lvi.SubItems.Add(name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1
                    ? ""
                    : Path.GetExtension(entry.FilePath)?.Substring(1));
                lvi.SubItems.Add(entry.FileSize.ToString());

                listView.Items.Add(lvi);
            }
            listView.EndUpdate();
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == _lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                _lvwColumnSorter.Order = _lvwColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                _lvwColumnSorter.SortColumn = e.Column;
                _lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listView.Sort(); //TODO: Make dates and ints sort properly.

            // Show the sort icon.
            listView.SetSortIcon(_lvwColumnSorter.SortColumn, _lvwColumnSorter.Order);
        }
    }
}