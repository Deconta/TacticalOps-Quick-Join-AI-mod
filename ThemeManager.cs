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
            _form.BackColor = UIConstants.LightTheme.Background;
            _form.ForeColor = UIConstants.LightTheme.Foreground;
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
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.RowTemplate.Height = UIConstants.ROW_HEIGHT;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.BackgroundColor = IsDarkMode ? UIConstants.DarkTheme.Surface : UIConstants.LightTheme.Surface;
        grid.GridColor = IsDarkMode ? UIConstants.DarkTheme.GridColor : UIConstants.LightTheme.GridColor;
        grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        grid.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
        grid.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
        grid.AdvancedColumnHeadersBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
        grid.AdvancedColumnHeadersBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;

        var headerFont = CreateManagedFont(UIConstants.Fonts.HeaderFont);
        var starHeaderFont = CreateManagedFont(UIConstants.Fonts.StarFont);
        var regularFont = CreateManagedFont(UIConstants.Fonts.RegularFont);

        if (IsDarkMode)
        {
            grid.ColumnHeadersDefaultCellStyle.BackColor = UIConstants.DarkTheme.HeaderBackground;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = UIConstants.DarkTheme.Foreground;
            grid.ColumnHeadersDefaultCellStyle.Font = headerFont;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = UIConstants.DarkTheme.HeaderBackground;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = UIConstants.DarkTheme.Foreground;
            
            ApplyHeaderStyles(grid, headerFont, starHeaderFont);
            
            grid.DefaultCellStyle.BackColor = UIConstants.DarkTheme.Surface;
            grid.DefaultCellStyle.ForeColor = UIConstants.DarkTheme.Foreground;
            grid.DefaultCellStyle.Font = regularFont;
            grid.DefaultCellStyle.SelectionBackColor = UIConstants.DarkTheme.AccentColor;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            
            grid.AlternatingRowsDefaultCellStyle.BackColor = UIConstants.DarkTheme.AlternatingRow;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = UIConstants.DarkTheme.AccentColor;
        }
        else
        {
            grid.ColumnHeadersDefaultCellStyle.BackColor = UIConstants.LightTheme.HeaderBackground;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = UIConstants.LightTheme.Foreground;
            grid.ColumnHeadersDefaultCellStyle.Font = headerFont;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = UIConstants.LightTheme.HeaderBackground;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = UIConstants.LightTheme.Foreground;

            ApplyHeaderStyles(grid, headerFont, starHeaderFont);
            
            grid.DefaultCellStyle.BackColor = UIConstants.LightTheme.Surface;
            grid.DefaultCellStyle.ForeColor = UIConstants.LightTheme.Foreground;
            grid.DefaultCellStyle.Font = regularFont;
            grid.DefaultCellStyle.SelectionBackColor = UIConstants.LightTheme.AccentColor;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            
            grid.AlternatingRowsDefaultCellStyle.BackColor = UIConstants.LightTheme.AlternatingRow;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = UIConstants.LightTheme.AccentColor;
        }
    }

    public void ApplyToButton(Button button)
    {
        if (button == null) return;

        var buttonFont = CreateManagedFont(UIConstants.Fonts.ButtonFont);
        
        button.BackColor = UIConstants.CommonColors.JoinButtonBackground;
        button.ForeColor = UIConstants.CommonColors.JoinButtonForeground;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = IsDarkMode ? UIConstants.DarkTheme.AccentHover : UIConstants.LightTheme.AccentHover;
        button.FlatAppearance.MouseDownBackColor = IsDarkMode ? UIConstants.DarkTheme.AccentColor : UIConstants.LightTheme.AccentColor;
        button.Font = buttonFont;
        button.UseVisualStyleBackColor = false;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.Padding = new Padding(0);
        button.Height = UIConstants.JOIN_BUTTON_HEIGHT;
        button.Cursor = Cursors.Hand;
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
            menuStrip.Renderer = new ToolStripProfessionalRenderer(new LightMenuColorTable());
            menuStrip.BackColor = UIConstants.LightTheme.Background;
            menuStrip.ForeColor = UIConstants.LightTheme.Foreground;
            SetMenuColors(menuStrip.Items, UIConstants.LightTheme.Foreground);
        }

        menuStrip.Font = CreateManagedFont(UIConstants.Fonts.MenuFont);
        foreach (ToolStripMenuItem item in menuStrip.Items)
        {
            item.Padding = new Padding(2, 0, 2, 0);
            item.Margin = Padding.Empty;
        }
    }

    public Color GetLabelColor(string labelType) => labelType switch
    {
        "NoResponse" => IsDarkMode ? Color.FromArgb(244, 122, 112) : Color.FromArgb(176, 47, 47),
        "NoPlayers" => IsDarkMode ? UIConstants.DarkTheme.MutedForeground : UIConstants.LightTheme.MutedForeground,
        "WaitingForResponse" => IsDarkMode ? Color.FromArgb(134, 190, 160) : UIConstants.LightTheme.AccentColor,
        "DownloadState" => IsDarkMode ? UIConstants.DarkTheme.MutedForeground : UIConstants.LightTheme.MutedForeground,
        _ => IsDarkMode ? UIConstants.DarkTheme.Foreground : UIConstants.LightTheme.Foreground
    };

    public Color GetPanelBackColor() => IsDarkMode ? UIConstants.DarkTheme.Surface : UIConstants.LightTheme.Surface;
    public Color GetPanelForeColor() => IsDarkMode ? UIConstants.DarkTheme.Foreground : UIConstants.LightTheme.Foreground;

    private void SetMenuColors(ToolStripItemCollection items, Color color)
    {
        foreach (ToolStripItem item in items)
        {
            item.ForeColor = color;
            if (item is ToolStripMenuItem dropDownItem)
                SetMenuColors(dropDownItem.DropDownItems, color);
        }
    }

    private static void ApplyHeaderStyles(DataGridView grid, Font headerFont, Font starHeaderFont)
    {
        foreach (DataGridViewColumn column in grid.Columns)
        {
            column.HeaderCell.Style ??= new DataGridViewCellStyle();
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.Font = column.Name == "FavColumn"
                ? starHeaderFont
                : headerFont;
            column.HeaderCell.Style.Padding = column.Name == "FavColumn"
                ? Padding.Empty
                : new Padding(6, 0, 6, 0);
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

public sealed class LightMenuColorTable : ProfessionalColorTable
{
    public override Color ToolStripDropDownBackground => UIConstants.LightTheme.Surface;
    public override Color MenuItemSelected => UIConstants.LightTheme.MenuSelected;
    public override Color MenuItemSelectedGradientBegin => UIConstants.LightTheme.MenuSelected;
    public override Color MenuItemSelectedGradientEnd => UIConstants.LightTheme.MenuSelected;
    public override Color MenuItemPressedGradientBegin => UIConstants.LightTheme.MenuPressed;
    public override Color MenuItemPressedGradientEnd => UIConstants.LightTheme.MenuPressed;
    public override Color ImageMarginGradientBegin => UIConstants.LightTheme.Surface;
    public override Color ImageMarginGradientMiddle => UIConstants.LightTheme.Surface;
    public override Color ImageMarginGradientEnd => UIConstants.LightTheme.Surface;
    public override Color MenuBorder => UIConstants.LightTheme.GridColor;
    public override Color MenuItemBorder => Color.Transparent;
    public override Color ToolStripBorder => UIConstants.LightTheme.GridColor;
}
