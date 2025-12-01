using System.Drawing;

namespace StudentReportInitial.Theming
{
    internal sealed class ThemePalette
    {
        public required Color FormBackColor { get; init; }
        public required Color PanelBackColor { get; init; }
        public required Color ControlBackColor { get; init; }
        public required Color SecondaryBackColor { get; init; }
        public required Color ControlForeColor { get; init; }
        public required Color AccentBackColor { get; init; }
        public required Color AccentForeColor { get; init; }
        public required Color BorderColor { get; init; }
        public required Color InputBackColor { get; init; }
        public required Color InputForeColor { get; init; }
        public required Color DataGridAlternateColor { get; init; }
        public required Color DataGridGridColor { get; init; }
    }

    internal static class LightThemePalette
    {
        public static readonly ThemePalette Value = new ThemePalette
        {
            FormBackColor = Color.White,
            PanelBackColor = Color.FromArgb(248, 250, 252),
            ControlBackColor = Color.White,
            SecondaryBackColor = Color.FromArgb(241, 245, 249),
            ControlForeColor = Color.FromArgb(31, 41, 55),
            AccentBackColor = Color.FromArgb(59, 130, 246),
            AccentForeColor = Color.White,
            BorderColor = Color.FromArgb(203, 213, 225),
            InputBackColor = Color.White,
            InputForeColor = Color.FromArgb(31, 41, 55),
            DataGridAlternateColor = Color.FromArgb(248, 250, 252),
            DataGridGridColor = Color.FromArgb(226, 232, 240)
        };
    }

    internal static class DarkThemePalette
    {
        public static readonly ThemePalette Value = new ThemePalette
        {
            FormBackColor = Color.FromArgb(24, 24, 24),
            PanelBackColor = Color.FromArgb(30, 30, 30),
            ControlBackColor = Color.FromArgb(42, 42, 42),
            SecondaryBackColor = Color.FromArgb(48, 48, 48),
            ControlForeColor = Color.White,
            AccentBackColor = Color.FromArgb(99, 102, 241),
            AccentForeColor = Color.White,
            BorderColor = Color.FromArgb(64, 64, 64),
            InputBackColor = Color.FromArgb(52, 52, 52),
            InputForeColor = Color.White,
            DataGridAlternateColor = Color.FromArgb(38, 38, 38),
            DataGridGridColor = Color.FromArgb(64, 64, 64)
        };
    }
}

