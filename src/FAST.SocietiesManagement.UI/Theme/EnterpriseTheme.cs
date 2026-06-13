using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace FAST.SocietiesManagement.UI.Theme
{
    public static class EnterpriseTheme
    {
        public static readonly Color AppBackground = Color.FromArgb(244, 247, 251);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceAlt = Color.FromArgb(248, 250, 252);
        public static readonly Color Border = Color.FromArgb(226, 232, 240);
        public static readonly Color Text = Color.FromArgb(15, 23, 42);
        public static readonly Color MutedText = Color.FromArgb(100, 116, 139);
        public static readonly Color Primary = Color.FromArgb(37, 99, 235);
        public static readonly Color PrimaryHover = Color.FromArgb(29, 78, 216);
        public static readonly Color PrimaryDark = Color.FromArgb(30, 64, 175);
        public static readonly Color Success = Color.FromArgb(5, 150, 105);
        public static readonly Color Warning = Color.FromArgb(217, 119, 6);
        public static readonly Color Danger = Color.FromArgb(220, 38, 38);
        public static readonly Color BlueSoft = Color.FromArgb(219, 234, 254);
        public static readonly Color GreenSoft = Color.FromArgb(209, 250, 229);
        public static readonly Color PurpleSoft = Color.FromArgb(237, 233, 254);
        public static readonly Color OrangeSoft = Color.FromArgb(254, 215, 170);
        public static readonly Color SidebarTop = Color.FromArgb(11, 18, 32);
        public static readonly Color SidebarBottom = Color.FromArgb(21, 45, 93);

        public static readonly Font TitleFont = new("Segoe UI", 22, FontStyle.Bold);
        public static readonly Font SectionFont = new("Segoe UI Semibold", 12, FontStyle.Bold);
        public static readonly Font BodyFont = new("Segoe UI", 10, FontStyle.Regular);
        public static readonly Font SmallFont = new("Segoe UI", 9, FontStyle.Regular);
        public static readonly Font ButtonFont = new("Segoe UI Semibold", 9, FontStyle.Bold);

        public static void ApplyToScreen(Control root)
        {
            root.BackColor = AppBackground;
            root.Font = BodyFont;
            foreach (Control child in root.Controls)
            {
                ApplyRecursive(child);
            }
        }

        private static void ApplyRecursive(Control control)
        {
            switch (control)
            {
                case Guna2DataGridView gunaGrid:
                    StyleGrid(gunaGrid);
                    break;
                case DataGridView grid:
                    StyleGrid(grid);
                    break;
                case Guna2Panel panel:
                    if (panel.FillColor == Color.Empty || panel.FillColor == Color.Transparent)
                    {
                        panel.FillColor = Surface;
                    }
                    panel.BorderRadius = Math.Max(panel.BorderRadius, 8);
                    panel.ShadowDecoration.Enabled = true;
                    panel.ShadowDecoration.Depth = 8;
                    break;
                case Panel panel when panel.BorderStyle == BorderStyle.FixedSingle:
                    panel.BackColor = BlueSoft;
                    panel.BorderStyle = BorderStyle.None;
                    break;
                case Button button:
                    StyleButton(button);
                    break;
                case Guna2Button button:
                    StyleButton(button);
                    break;
                case TextBox textBox:
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = Surface;
                    textBox.ForeColor = Text;
                    textBox.Font = BodyFont;
                    break;
                case Guna2TextBox textBox:
                    textBox.BorderRadius = Math.Max(textBox.BorderRadius, 8);
                    textBox.BorderColor = Border;
                    textBox.FocusedState.BorderColor = Primary;
                    textBox.FillColor = Surface;
                    textBox.ForeColor = Text;
                    textBox.PlaceholderForeColor = MutedText;
                    textBox.Font = BodyFont;
                    break;
                case ComboBox comboBox:
                    comboBox.FlatStyle = FlatStyle.Flat;
                    comboBox.BackColor = Surface;
                    comboBox.ForeColor = Text;
                    comboBox.Font = BodyFont;
                    break;
                case Label label:
                    StyleLabel(label);
                    break;
            }

            foreach (Control child in control.Controls)
            {
                ApplyRecursive(child);
            }
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Surface;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.ReadOnly = true;
            grid.EditMode = DataGridViewEditMode.EditProgrammatically;
            grid.AllowUserToResizeRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 23, 42);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = BlueSoft;
            grid.DefaultCellStyle.SelectionForeColor = Text;
            grid.DefaultCellStyle.Font = SmallFont;
            grid.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
            grid.AlternatingRowsDefaultCellStyle.BackColor = BlueSoft;
            grid.RowTemplate.Height = 38;
            grid.ColumnHeadersHeight = 42;
            grid.DataBindingComplete -= ClearGridSelection;
            grid.DataBindingComplete += ClearGridSelection;
            grid.ClearSelection();
            grid.CurrentCell = null;
        }

        private static void ClearGridSelection(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (sender is not DataGridView grid)
            {
                return;
            }

            grid.ClearSelection();
            grid.CurrentCell = null;
        }

        public static void StyleButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.ForeColor = Color.White;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;

            if (button.BackColor == Color.Empty || button.BackColor == SystemColors.Control)
            {
                button.BackColor = Primary;
            }
            button.FlatAppearance.MouseOverBackColor = Darken(button.BackColor, 0.12f);
            button.FlatAppearance.MouseDownBackColor = Darken(button.BackColor, 0.2f);
        }

        public static void StyleButton(Guna2Button button)
        {
            button.BorderRadius = Math.Max(button.BorderRadius, 8);
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
            button.ForeColor = Color.White;

            if (button.FillColor == Color.Empty || button.FillColor == Color.Transparent)
            {
                button.FillColor = Primary;
            }
            button.HoverState.FillColor = Darken(button.FillColor, 0.12f);
            button.HoverState.ForeColor = Color.White;
            button.PressedColor = Darken(button.FillColor, 0.2f);
        }

        private static void StyleLabel(Label label)
        {
            label.UseMnemonic = false;
            label.BackColor = Color.Transparent;
            if (label.Font.Size >= 20)
            {
                label.Font = TitleFont;
                if (label.ForeColor == Color.Empty || label.ForeColor == SystemColors.ControlText)
                {
                    label.ForeColor = Text;
                }
            }
            else if (label.Font.Bold)
            {
                label.Font = SectionFont;
                if (label.ForeColor == Color.Empty || label.ForeColor == SystemColors.ControlText)
                {
                    label.ForeColor = Text;
                }
            }
            else if (label.ForeColor == Color.Empty || label.ForeColor == SystemColors.ControlText)
            {
                label.Font = SmallFont;
                label.ForeColor = MutedText;
            }
        }

        private static Color Darken(Color color, float amount)
        {
            if (color == Color.Empty || color == Color.Transparent)
            {
                return PrimaryHover;
            }

            var factor = Math.Clamp(1f - amount, 0f, 1f);
            return Color.FromArgb(
                color.A,
                (int)(color.R * factor),
                (int)(color.G * factor),
                (int)(color.B * factor));
        }
    }
}
