using System.Drawing;
using System.Windows.Forms;

namespace SteamAutocrackGUI
{
    partial class BatchGameSelectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Label titleLabel;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
        ///
        /// NOTE: this contains only the static form chrome. The grid, upload
        /// slots, buttons and all data-driven controls are built at runtime in
        /// BuildUi() because they depend on loops, runtime data and lambda
        /// handlers that the WinForms designer cannot serialize.
        /// </summary>
        private void InitializeComponent()
        {
            this.titleLabel = new Label();
            this.SuspendLayout();
            //
            // titleLabel
            //
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Text = "Batch Processor";
            this.titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.titleLabel.ForeColor = Color.FromArgb(100, 200, 255);
            this.titleLabel.Location = new Point(15, 15);
            this.titleLabel.Size = new Size(200, 30);
            this.titleLabel.BackColor = Color.Transparent;
            //
            // BatchGameSelectionForm
            //
            this.Text = "Batch Processor - Select Games";
            this.ClientSize = new Size(760, 580);
            this.MinimumSize = new Size(760, 300);
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.FromArgb(5, 8, 20);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = true; // Show in taskbar since main form hides
            this.Controls.Add(this.titleLabel);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
