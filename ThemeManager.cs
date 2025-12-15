namespace TacticalOpsQuickJoin;

public sealed class ThemeManager : IDisposable
{
    private readonly Form _form;
    private readonly List<Font> _managedFonts = new();
    private bool _disposed;

    public bool IsDarkMode { get; private set; }

    public ThemeManager(Form form, bool isDarkMode)
    {
        _form = form ?? throw new ArgumentNullException(nameof(form));
        IsDarkMode = isDarkMode;
    }

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (IsDarkMode)
        {
            _form.BackColor = UIConstants.DarkTheme.Background;
            _form.ForeColor = UIConstants.DarkTheme.Foreground;
        }
        else
        {
            _form.BackColor = SystemColors.Control;
            _form.ForeColor = SystemColors.ControlText;
        }
    }

    public void ApplyToDataGridView(DataGridView grid)
    {
        if (grid == null) return;

        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight = UIConstants.HEADER_HEIGHT;
        grid.RowTemplate.Height = UIConstants.ROW_HEIGHT;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        var headerFont = CreateManagedFont(UIConstants.Fonts.HeaderFont);
        var regularFont = CreateManagedFont(UIConstants.Fonts.RegularFont);

        if (IsDarkMode)
        {
            grid.BackgroundColor = UIConstants.DarkTheme.Background;
            grid.GridColor = UIConstants.DarkTheme.GridColor;
            
            grid.ColumnHeadersDefaultCellStyle.BackColor = UIConstants.DarkTheme.HeaderBackground;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = UIConstants.DarkTheme.Foreground;
            grid.ColumnHeadersDefaultCellStyle.Font = headerFont;
            // grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Removed global alignment
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = UIConstants.DarkTheme.HeaderBackground;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = UIConstants.DarkTheme.Foreground;
            
            // Explicitly set header alignment for each column
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.HeaderCell.Style == null)
                {
                    column.HeaderCell.Style = new DataGridViewCellStyle();
                }
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            
            grid.DefaultCellStyle.BackColor = UIConstants.DarkTheme.Background;
            grid.DefaultCellStyle.ForeColor = UIConstants.DarkTheme.Foreground;
            grid.DefaultCellStyle.Font = regularFont;
            grid.DefaultCellStyle.SelectionBackColor = UIConstants.DarkTheme.AccentColor;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            
            grid.AlternatingRowsDefaultCellStyle.BackColor = UIConstants.DarkTheme.AlternatingRow;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = UIConstants.DarkTheme.AccentColor;
        }
        else
        {
            grid.BackgroundColor = SystemColors.Window;
            grid.GridColor = SystemColors.ControlLight;
            
            grid.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
            grid.ColumnHeadersDefaultCellStyle.Font = headerFont;
            // grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Removed global alignment
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Control;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.ControlText;

            // Explicitly set header alignment for each column
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.HeaderCell.Style == null)
                {
                    column.HeaderCell.Style = new DataGridViewCellStyle();
                }
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            
            grid.DefaultCellStyle.BackColor = SystemColors.Window;
            grid.DefaultCellStyle.ForeColor = SystemColors.WindowText;
            grid.DefaultCellStyle.Font = regularFont;
            grid.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            grid.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            
            grid.AlternatingRowsDefaultCellStyle.BackColor = UIConstants.LightTheme.AlternatingRow;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
        }
    }

    public void ApplyToButton(Button button)
    {
        if (button == null) return;

        var buttonFont = CreateManagedFont(UIConstants.Fonts.ButtonFont);
        
        button.BackColor = UIConstants.CommonColors.JoinButtonBackground;
        button.ForeColor = UIConstants.CommonColors.JoinButtonForeground;
        button.FlatStyle = FlatStyle.Standard;
        button.Font = buttonFont;
        button.UseVisualStyleBackColor = false;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.Padding = new Padding(0);
        button.Height = UIConstants.JOIN_BUTTON_HEIGHT;
    }

    public void ApplyToMenuStrip(MenuStrip menuStrip)
    {
        if (menuStrip == null) return;

        if (IsDarkMode)
        {
            menuStrip.Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable());
            menuStrip.BackColor = UIConstants.DarkTheme.Background;
            menuStrip.ForeColor = UIConstants.DarkTheme.Foreground;
            SetMenuColors(menuStrip.Items, UIConstants.DarkTheme.Foreground);
        }
        else
        {
            menuStrip.Renderer = new ToolStripProfessionalRenderer();
            menuStrip.BackColor = SystemColors.MenuBar;
            menuStrip.ForeColor = SystemColors.MenuText;
            SetMenuColors(menuStrip.Items, SystemColors.MenuText);
        }
    }

    public Color GetLabelColor(string labelType) => labelType switch
    {
        "NoResponse" => IsDarkMode ? Color.LightCoral : Color.Red,
        "NoPlayers" => IsDarkMode ? Color.WhiteSmoke : SystemColors.ControlText,
        "WaitingForResponse" => IsDarkMode ? Color.LightSkyBlue : Color.Blue,
        "DownloadState" => IsDarkMode ? Color.WhiteSmoke : SystemColors.ControlText,
        _ => IsDarkMode ? Color.WhiteSmoke : SystemColors.ControlText
    };

    public Color GetPanelBackColor() => IsDarkMode ? UIConstants.DarkTheme.Background : SystemColors.Control;
    public Color GetPanelForeColor() => IsDarkMode ? UIConstants.DarkTheme.Foreground : SystemColors.ControlText;

    private void SetMenuColors(ToolStripItemCollection items, Color color)
    {
        foreach (ToolStripItem item in items)
        {
            item.ForeColor = color;
            if (item is ToolStripMenuItem dropDownItem)
                SetMenuColors(dropDownItem.DropDownItems, color);
        }
    }

    private Font CreateManagedFont(Font sourceFont)
    {
        var font = new Font(sourceFont.FontFamily, sourceFont.Size, sourceFont.Style);
        _managedFonts.Add(font);
        return font;
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        foreach (var font in _managedFonts)
        {
            font?.Dispose();
        }
        _managedFonts.Clear();
        
        _disposed = true;
    }
}

public sealed class DarkMenuColorTable : ProfessionalColorTable
{
    public override Color ToolStripDropDownBackground => UIConstants.DarkTheme.Background;
    public override Color MenuItemSelected => UIConstants.DarkTheme.MenuSelected;
    public override Color MenuItemSelectedGradientBegin => UIConstants.DarkTheme.MenuSelected;
    public override Color MenuItemSelectedGradientEnd => UIConstants.DarkTheme.MenuSelected;
    public override Color MenuItemPressedGradientBegin => UIConstants.DarkTheme.MenuPressed;
    public override Color MenuItemPressedGradientEnd => UIConstants.DarkTheme.MenuPressed;
    public override Color ImageMarginGradientBegin => UIConstants.DarkTheme.Background;
    public override Color ImageMarginGradientMiddle => UIConstants.DarkTheme.Background;
    public override Color ImageMarginGradientEnd => UIConstants.DarkTheme.Background;
    public override Color MenuBorder => UIConstants.DarkTheme.MenuBorder;
    public override Color MenuItemBorder => Color.Transparent;
    public override Color ToolStripBorder => UIConstants.DarkTheme.MenuBorder;
}
