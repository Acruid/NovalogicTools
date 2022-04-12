using System;
using System.IO;
using System.Windows.Forms;
using Novalogic.PFF;

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

        public void OpenFilePreview(PffEntry fileContents)
        {
            if (fileContents == null)
                return;


            //TODO: Reuse window
            var mdiChild = new FormPreview
            {
                MdiParent = this,
                PreviewFile = fileContents
            };
            mdiChild.Show();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var mdiChild = new FormArchive
                {
                    MdiParent = this,
                    ArchiveFile = new FileInfo(openFileDialog.FileName)
                };
                mdiChild.Show();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}