using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace StudentReportInitial.Forms
{
    internal static class UIStyleHelper
    {
        public static void ApplyRoundedCorners(Control control, int radius, bool drawBorder = false, Color? borderColor = null)
        {
            if (control == null || radius <= 0)
            {
                return;
            }

            void UpdateRegion()
            {
                if (control.Width <= 0 || control.Height <= 0)
                {
                    return;
                }

                using var path = CreateRoundedRectanglePath(new Rectangle(Point.Empty, control.Size), radius);
                var previousRegion = control.Region;
                control.Region = new Region(path);
                previousRegion?.Dispose();
            }

            void PaintHandler(object? sender, PaintEventArgs e)
            {
                UpdateRegion();

                if (!drawBorder)
                {
                    return;
                }

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = CreateRoundedRectanglePath(new Rectangle(0, 0, control.Width - 1, control.Height - 1), radius);
                using var pen = new Pen(borderColor ?? Color.FromArgb(226, 232, 240));
                e.Graphics.DrawPath(pen, path);
            }

            void ResizeHandler(object? sender, EventArgs e) => UpdateRegion();

            UpdateRegion();
            control.Paint += PaintHandler;
            control.Resize += ResizeHandler;

            control.Disposed += (_, _) =>
            {
                control.Paint -= PaintHandler;
                control.Resize -= ResizeHandler;
            };
        }

        public static void ApplyRoundedButton(Button button, int radius)
        {
            // Store original colors for custom painting
            var originalBackColor = button.BackColor;
            var originalForeColor = button.ForeColor;
            var originalFont = button.Font;

            // Configure button for custom painting
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button.FlatAppearance.MouseDownBackColor = Color.Transparent;
            button.BackColor = Color.Transparent; // Make background transparent so our custom paint shows
            button.Cursor = Cursors.Hand;

            // Track button state
            bool isHovered = false;
            bool isPressed = false;

            void PaintButton(object? sender, PaintEventArgs e)
            {
                var btn = sender as Button;
                if (btn == null) return;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                var rect = new Rectangle(0, 0, btn.Width, btn.Height);
                using var path = CreateRoundedRectanglePath(rect, radius);

                // Determine background color based on button state
                Color backColor = originalBackColor;
                if (!btn.Enabled)
                {
                    backColor = ControlPaint.Light(originalBackColor, 0.3f);
                }
                else if (isPressed)
                {
                    backColor = ControlPaint.Dark(originalBackColor, 0.15f);
                }
                else if (isHovered)
                {
                    backColor = ControlPaint.Light(originalBackColor, 0.1f);
                }

                // Fill rounded rectangle with anti-aliasing - always visible with rounded corners
                using var brush = new SolidBrush(backColor);
                e.Graphics.FillPath(brush, path);

                // Draw text with proper centering
                TextRenderer.DrawText(e.Graphics, btn.Text, originalFont, rect, originalForeColor, 
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            void MouseEnterHandler(object? sender, EventArgs e)
            {
                isHovered = true;
                button.Invalidate();
            }

            void MouseLeaveHandler(object? sender, EventArgs e)
            {
                isHovered = false;
                isPressed = false;
                button.Invalidate();
            }

            void MouseDownHandler(object? sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    isPressed = true;
                    button.Invalidate();
                }
            }

            void MouseUpHandler(object? sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    isPressed = false;
                    button.Invalidate();
                }
            }

            button.Paint += PaintButton;
            button.MouseEnter += MouseEnterHandler;
            button.MouseLeave += MouseLeaveHandler;
            button.MouseDown += MouseDownHandler;
            button.MouseUp += MouseUpHandler;
            button.Resize += (s, e) => button.Invalidate();

            button.Disposed += (_, _) =>
            {
                button.Paint -= PaintButton;
                button.MouseEnter -= MouseEnterHandler;
                button.MouseLeave -= MouseLeaveHandler;
                button.MouseDown -= MouseDownHandler;
                button.MouseUp -= MouseUpHandler;
            };
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();

            if (diameter > rect.Width)
            {
                diameter = rect.Width;
            }

            if (diameter > rect.Height)
            {
                diameter = rect.Height;
            }

            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top left
            path.AddArc(arc, 180, 90);

            // Top right
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}

