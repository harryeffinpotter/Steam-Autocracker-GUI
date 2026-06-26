using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SteamAutocrackGUI
{
    public class BatchGameItem
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string AppId { get; set; }
        public bool Crack { get; set; } = true;
        public bool Zip { get; set; } = false;
        public bool Upload { get; set; } = false;

        // Crack method tracking (set after cracking): "Goldberg", "Ali", "Goldberg+Steamless", "Ali+Steamless", or empty for clean
        public string CrackMethod { get; set; } = "";

        // Manifest info for clean files sharing
        public string BuildId { get; set; }
        public long LastUpdated { get; set; } // Unix timestamp
        public string Branch { get; set; } = "Public";
        public string Platform { get; set; } // Win64, Win32, etc.
        public Dictionary<string, (string manifest, long size)> InstalledDepots { get; set; } = new Dictionary<string, (string, long)>();
    }

    /// <summary>
    /// Custom progress bar with neon blue gradient styling
    /// </summary>
    public class NeonProgressBar : ProgressBar
    {
        public NeonProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = this.ClientRectangle;

            // Dark background
            using (var bgBrush = new SolidBrush(Color.FromArgb(10, 12, 20)))
            {
                e.Graphics.FillRectangle(bgBrush, rect);
            }

            // Border
            using (var borderPen = new Pen(Color.FromArgb(40, 50, 70)))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
            }

            // Progress fill with neon blue gradient
            if (this.Value > 0)
            {
                int fillWidth = (int)((rect.Width - 4) * ((double)this.Value / this.Maximum));
                if (fillWidth > 0)
                {
                    using (var brush = new LinearGradientBrush(
                        new Rectangle(2, 2, fillWidth, rect.Height - 4),
                        Color.FromArgb(0, 150, 255),   // Bright neon blue
                        Color.FromArgb(0, 100, 200),   // Darker blue
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillRectangle(brush, 2, 2, fillWidth, rect.Height - 4);
                    }

                    // Add glow effect on top
                    using (var glowBrush = new SolidBrush(Color.FromArgb(40, 150, 220, 255)))
                    {
                        e.Graphics.FillRectangle(glowBrush, 2, 2, fillWidth, (rect.Height - 4) / 3);
                    }
                }
            }
        }
    }
}
