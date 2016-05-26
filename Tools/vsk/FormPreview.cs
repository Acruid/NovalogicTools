using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using Novalogic._3DI;
using Novalogic.PCX;
using Novalogic.PFF;
using Novalogic.TGA;
using vsk.Rendering;

namespace vsk
{
    /// <summary>
    ///     Form for previewing a file.
    /// </summary>
    public partial class FormPreview : Form
    {
        private ModelRenderer _renderer;

        /// <summary>
        ///     Initializer.
        /// </summary>
        public FormPreview()
        {
            InitializeComponent();
            Text = FormatWindowTitle(string.Empty);
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        /// <summary>
        /// </summary>
        public PffEntry PreviewFile
        {
            set { LoadFile(value); }
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

            var fileContents = entry.GetContents();
            if (fileContents == null)
                return;

            Bitmap img;
            var ext = Path.GetExtension(entry.FilePath)?.Substring(1);
            switch (ext)
            {
                case "PCX":
                    img = PcxConvert.LoadPcx(fileContents);
                    ClientSize = img.Size;
                    pictureBox.Image = img;
                    pictureBox.Visible = true;
                    break;
                case "TGA":
                    img = TgaConvert.LoadTga(fileContents);
                    ClientSize = img.Size;
                    pictureBox.Image = img;
                    pictureBox.Visible = true;
                    break;
                case "JPG":
                    img = LoadBitmap(fileContents);
                    ClientSize = img.Size;
                    pictureBox.Image = img;
                    pictureBox.Visible = true;
                    break;
                case "WAV":
                    using (var stream = new MemoryStream(fileContents))
                    {
                        var simpleSound = new SoundPlayer(stream);
                        simpleSound.Play();
                    }
                    break;
                case "3DI":
                    var file = File3di.Open(fileContents);
                    _renderer = new ModelRenderer(renderControl, file);
                    renderControl.Visible = true;
                    break;
            }
        }

        private static Bitmap LoadBitmap(byte[] fileBytes)
        {
            using (var stream = new MemoryStream(fileBytes))
                return new Bitmap(stream);
        }
    }
}