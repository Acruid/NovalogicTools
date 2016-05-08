using System;
using System.Windows.Forms;
using Novalogic.Archive;

namespace vsk
{
    /// <summary>
    ///     Form for previewing a file.
    /// </summary>
    public partial class FormPreview : Form
    {
        /// <summary>
        ///     Initializer.
        /// </summary>
        public FormPreview()
        {
            InitializeComponent();
            Text = FormatWindowTitle(string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        public PffEntry PreviewFile
        {
            set
            {
                LoadFile(value);
            }
        }

        private static string FormatWindowTitle(string fileName)
        {
            return $"Preview - {fileName}";
        }

        private void LoadFile(PffEntry entry)
        {
            if (entry == null)
            {
                Text = FormatWindowTitle(string.Empty);

                //TODO: Clear preview

                return;
            }

            Text = FormatWindowTitle(entry.FilePath);

            //TODO: Show preview

            //var fileContents = entry.GetFile();
            //if (fileContents != null) { }

        }
    }
}