using System;
using System.IO;
using System.Windows.Forms;

namespace vsk
{
    /// <summary>
    ///     Parent form of the MDI system.
    /// </summary>
    public partial class FormParent : Form
    {
        /// <summary>
        ///     Initializer.
        /// </summary>
        public FormParent()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var mdiChild = new FormArchive
                {
                    MdiParent = this
                };
                mdiChild.Show();
                mdiChild.ArchiveFile = new FileInfo(openFileDialog.FileName);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}