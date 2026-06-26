using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    /// <summary>
    /// Custom progress bar with neon blue gradient styling for EnhancedShareWindow
    /// </summary>
    public class ShareNeonProgressBar : ProgressBar
    {
        public ShareNeonProgressBar()
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

    /// <summary>
    /// Custom DataGridView that supports transparent background for acrylic effect
    /// </summary>
    public class TransparentDataGridView : DataGridView
    {
        public TransparentDataGridView()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.Opaque, false);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.DoubleBuffered = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Don't paint background - let acrylic show through
        }
    }
}
