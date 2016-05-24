namespace vsk
{
    partial class FormPreview
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.renderControl = new vsk.Rendering.RenderControl();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(520, 437);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "8888";
            // 
            // renderControl
            // 
            this.renderControl.BackColor = System.Drawing.Color.Black;
            this.renderControl.ClearColor = System.Drawing.Color.Black;
            this.renderControl.CursorVisible = false;
            this.renderControl.Icon = null;
            this.renderControl.Location = new System.Drawing.Point(12, 25);
            this.renderControl.Name = "renderControl";
            this.renderControl.Size = new System.Drawing.Size(496, 400);
            this.renderControl.TabIndex = 1;
            this.renderControl.Title = null;
            this.renderControl.VSync = true;
            this.renderControl.WindowBorder = OpenTK.WindowBorder.Resizable;
            this.renderControl.WindowState = OpenTK.WindowState.Normal;
            this.renderControl.X = 0;
            this.renderControl.Y = 0;
            // 
            // FormPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 437);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.renderControl);
            this.Controls.Add(this.pictureBox);
            this.Name = "FormPreview";
            this.Text = "FormPreview";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private Rendering.RenderControl renderControl;
        private System.Windows.Forms.Label label1;
    }
}