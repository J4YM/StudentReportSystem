using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using StudentReportInitial.Properties;

namespace StudentReportInitial.Theming
{
    internal enum AppTheme
    {
        Light,
        Dark
    }

    internal static class ThemeManager
    {
        private static readonly ConditionalWeakTable<Control, ThemeMetadata> metadata = new();

        public static event EventHandler? ThemeChanged;

        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

        public static ThemePalette CurrentPalette => CurrentTheme == AppTheme.Dark
            ? DarkThemePalette.Value
            : LightThemePalette.Value;

        public static void Initialize()
        {
            CurrentTheme = Settings.Default.IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }

        public static void ToggleTheme()
        {
            SetTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
        }

        public static void SetTheme(AppTheme theme)
        {
            if (CurrentTheme == theme)
            {
                return;
            }

            CurrentTheme = theme;
            Settings.Default.IsDarkMode = theme == AppTheme.Dark;
            Settings.Default.Save();

            ApplyThemeToOpenForms();
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void ApplyThemeToOpenForms()
        {
            foreach (Form form in Application.OpenForms)
            {
                ApplyTheme(form);
            }
        }

        public static void ApplyTheme(Control control)
        {
            if (control == null)
            {
                return;
            }

            var palette = CurrentPalette;
            ApplyTheme(control, palette);
            control.Invalidate();
        }

        private static void ApplyTheme(Control control, ThemePalette palette)
        {
            switch (control)
            {
                case Form form:
                    form.BackColor = palette.FormBackColor;
                    form.ForeColor = palette.ControlForeColor;
                    break;

                case Panel or GroupBox:
                    control.BackColor = palette.PanelBackColor;
                    control.ForeColor = palette.ControlForeColor;
                    break;

                case LinkLabel link:
                    link.LinkColor = palette.AccentBackColor;
                    link.ActiveLinkColor = palette.AccentBackColor;
                    link.VisitedLinkColor = palette.AccentBackColor;
                    link.ForeColor = palette.ControlForeColor;
                    break;

                case Label label:
                    label.ForeColor = palette.ControlForeColor;
                    break;

                case Button button:
                    ApplyButtonTheme(button, palette);
                    break;

                case TextBox or MaskedTextBox:
                    control.BackColor = palette.InputBackColor;
                    control.ForeColor = palette.InputForeColor;
                    break;

                case ComboBox comboBox:
                    comboBox.BackColor = palette.InputBackColor;
                    comboBox.ForeColor = palette.InputForeColor;
                    break;

                case NumericUpDown numericUpDown:
                    numericUpDown.BackColor = palette.InputBackColor;
                    numericUpDown.ForeColor = palette.InputForeColor;
                    break;

                case DataGridView grid:
                    ApplyDataGridTheme(grid, palette);
                    break;

                default:
                    if (control is not PictureBox)
                    {
                        control.BackColor = palette.ControlBackColor;
                        control.ForeColor = palette.ControlForeColor;
                    }
                    break;
            }

            foreach (Control child in control.Controls)
            {
                ApplyTheme(child, palette);
            }
        }

        private static void ApplyButtonTheme(Button button, ThemePalette palette)
        {
            var data = metadata.GetValue(button, _ => new ThemeMetadata(button.BackColor, button.ForeColor));

            button.FlatStyle = FlatStyle.Flat;
            button.ForeColor = palette.ControlForeColor;
            button.FlatAppearance.BorderColor = palette.BorderColor;

            if (IsNeutralColor(data.OriginalBackColor))
            {
                button.BackColor = palette.SecondaryBackColor;
            }
            else
            {
                button.BackColor = data.OriginalBackColor;
            }
        }

        private static void ApplyDataGridTheme(DataGridView grid, ThemePalette palette)
        {
            grid.BackgroundColor = palette.PanelBackColor;
            grid.EnableHeadersVisualStyles = false;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = palette.DataGridGridColor;

            var headerStyle = grid.ColumnHeadersDefaultCellStyle;
            headerStyle.BackColor = palette.SecondaryBackColor;
            headerStyle.ForeColor = palette.ControlForeColor;

            var defaultStyle = grid.DefaultCellStyle;
            defaultStyle.BackColor = palette.ControlBackColor;
            defaultStyle.ForeColor = palette.ControlForeColor;
            defaultStyle.SelectionBackColor = palette.AccentBackColor;
            defaultStyle.SelectionForeColor = palette.AccentForeColor;

            grid.AlternatingRowsDefaultCellStyle.BackColor = palette.DataGridAlternateColor;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = palette.AccentBackColor;
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = palette.AccentForeColor;
        }

        private static bool IsNeutralColor(Color color)
        {
            if (color.IsEmpty || color == SystemColors.Control || color == Color.Transparent)
            {
                return true;
            }

            ReadOnlySpan<Color> neutralColors =
            [
                Color.White,
                Color.FromArgb(248, 250, 252),
                Color.FromArgb(241, 245, 249)
            ];

            foreach (var neutral in neutralColors)
            {
                if (neutral.ToArgb() == color.ToArgb())
                {
                    return true;
                }
            }

            return false;
        }

        private sealed record ThemeMetadata(Color OriginalBackColor, Color OriginalForeColor);
    }
}

