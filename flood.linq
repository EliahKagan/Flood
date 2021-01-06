<Query Kind="Statements">
  <NuGetReference>Microsoft.Web.WebView2</NuGetReference>
  <NuGetReference>Nito.Collections.Deque</NuGetReference>
  <Namespace>Key = System.Windows.Input.Key</Namespace>
  <Namespace>Keyboard = System.Windows.Input.Keyboard</Namespace>
  <Namespace>LC = LINQPad.Controls</Namespace>
  <Namespace>Microsoft.Web.WebView2.Core</Namespace>
  <Namespace>Microsoft.Web.WebView2.WinForms</Namespace>
  <Namespace>Nito.Collections</Namespace>
  <Namespace>static LINQPad.Controls.ControlExtensions</Namespace>
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <RuntimeVersion>5.0</RuntimeVersion>
</Query>

// flood.linq - Interactive flood-fill visualizer.

#nullable enable

if (Control.ModifierKeys.HasFlag(Keys.Shift)) {
    // Use the launcher ("developer mode").
    var launcher = new Launcher(SuggestCanvasSize());
    launcher.Launch += launcher_Launch;
    launcher.Display();
} else {
    // Proceed immediately with the automatically suggested size.
    new MainPanel(SuggestCanvasSize(), GetBestHelpViewerAsync).Display();
}

static Size SuggestCanvasSize()
{
    // TODO: Maybe try to check which screen the LINQPad window is on.
    var (width, height) = Screen.PrimaryScreen.Bounds.Size;
    var sideLength = Math.Min(width, height) * 5 / 9;
    return new(width: sideLength, height: sideLength);
}

static Task<HelpViewer> GetOldHelpViewerAsync()
    => Task.FromResult<HelpViewer>(WebBrowserHelpViewer.Create());

static async Task<HelpViewer> GetBestHelpViewerAsync()
{
    try {
        return await WebView2HelpViewer.CreateAsync();
    } catch (EdgeNotFoundException) {
        return WebBrowserHelpViewer.Create();
    }
}

static void launcher_Launch(Launcher sender, LauncherEventArgs e)
{
    HelpViewerSupplier supplier =
        (e.UseOldWebBrowser ? GetOldHelpViewerAsync : GetBestHelpViewerAsync);

    new MainPanel(e.Size, supplier) {
        DelayInMilliseconds = sender.DelayInMilliseconds,
        ShowParentInTaskbar = sender.ShowPluginFormInTaskbar,
    }.Display();
}

/// <summary>
/// Provides a canvas size for the <see cref="Launcher.Launch"/> event.
/// </summary>
internal sealed class LauncherEventArgs : EventArgs {
    internal LauncherEventArgs(Size size, bool useOldWebBrowser)
        => (Size, UseOldWebBrowser) = (size, useOldWebBrowser);

    internal Size Size { get; }

    internal bool UseOldWebBrowser { get; }
};

/// <summary>
/// Represents a method that will handle the <see cref="Launcher.Launch"/>
/// event.
/// </summary>
internal delegate void LauncherEventHandler(Launcher sender,
                                            LauncherEventArgs e);

/// <summary>
/// "Developer mode" launcher allowing the user to specify a canvas size.
/// </summary>
internal sealed class Launcher {
    internal Launcher(Size defaultSize)
    {
        _width = defaultSize.Width;
        _height = defaultSize.Height;

        _widthBox = CreateNumberBox(_width);
        _heightBox = CreateNumberBox(_height);
        _delayBox = CreateNumberBox(_delay);

        _panel = new(horizontal: false,
            new LC.FieldSet("Custom Canvas Size", CreateSizeTable()),
            new LC.FieldSet("Asynchronous Delay Behavior", CreateDelayPanel()),
            new LC.FieldSet("Screen Capture Hack", _showPluginFormInTaskbar),
            new LC.FieldSet("Help Browser", _useOldWebBrowser),
            _launch);

        SubscribePrivateHandlers();
    }

    internal event LauncherEventHandler? Launch = null;

    internal int DelayInMilliseconds
        => _delay is int delay
            ? delay
            : throw new NotSupportedException(
                "Bug: Launch button enabled without delay set.");

    internal bool ShowPluginFormInTaskbar => _showPluginFormInTaskbar.Checked;

    internal void Display() => _panel.Dump("Developer Mode Launcher");

    private const string NumberBoxWidth = "5em";

    private static LC.TextBox CreateNumberBox(int? initialValue)
        => new(initialValue.ToString()) { Width = NumberBoxWidth };

    private static void Disable(params LC.Control[] controls)
    {
        foreach (var control in controls) control.Enabled = false;
    }

    private LC.Table CreateSizeTable()
    {
        var table = new LC.Table(noBorders: true,
                                 cellPaddingStyle: ".3em .3em",
                                 cellVerticalAlign: "middle");

        table.Rows.Add(new LC.Label("Width"), _widthBox);
        table.Rows.Add(new LC.Label("Height"), _heightBox);

        return table;
    }

    private LC.StackPanel CreateDelayPanel()
    {
        var table = new LC.Table(noBorders: true,
                                 cellPaddingStyle: ".3em .3em",
                                 cellVerticalAlign: "middle");

        table.Rows.Add(new LC.Label("Delay (ms)"), _delayBox);

        var label = new LC.Label("This is the minimum delay between frames.");

        return new(horizontal: false, table, label);
    }

    private void SubscribePrivateHandlers()
    {
        _widthBox.TextInput += widthBox_TextInput;
        _heightBox.TextInput += heightBox_TextInput;
        _delayBox.TextInput += delayBox_TextInput;
        _launch.Click += launch_Click;
    }

    private void widthBox_TextInput(object? sender, EventArgs e)
        => HandleNumberInput(_widthBox, ref _width);

    private void heightBox_TextInput(object? sender, EventArgs e)
        => HandleNumberInput(_heightBox, ref _height);

    private void delayBox_TextInput(object? sender, EventArgs e)
        => HandleNumberInput(_delayBox, ref _delay);

    private void launch_Click(object? sender, EventArgs e)
    {
        if (_width is not int width || _height is not int height) {
            throw new NotSupportedException(
                "Bug: Launch button enabled without width and height set.");
        }

        DisableInteractiveControls();

        var size = new Size(width: width, height: height);
        var eLauncher = new LauncherEventArgs(size, _useOldWebBrowser.Checked);
        Launch?.Invoke(this, eLauncher);
    }

    private void HandleNumberInput(LC.TextBox sender, ref int? sink)
    {
        sink = int.TryParse(sender.Text, out var value) && value > 0
                ? value
                : null;

        UpdateLaunchButton();
    }

    private void UpdateLaunchButton()
        => _launch.Enabled = _width is int && _height is int && _delay is int;

    private void DisableInteractiveControls()
        => Disable(_widthBox,
                   _heightBox,
                   _delayBox,
                   _launch,
                   _showPluginFormInTaskbar,
                   _useOldWebBrowser);

    private readonly LC.TextBox _widthBox;

    private readonly LC.TextBox _heightBox;

    private readonly LC.TextBox _delayBox;

    private readonly LC.CheckBox _showPluginFormInTaskbar =
        new LC.CheckBox("Show PluginForm in Taskbar");

    private readonly LC.CheckBox _useOldWebBrowser = new LC.CheckBox(
            "Use old WebBrowser control even if WebView2 is available");

    private readonly LC.Button _launch = new LC.Button("Launch!");

    private readonly LC.StackPanel _panel;

    private int? _width;

    private int? _height;

    private int? _delay = MainPanel.DefaultDelayInMilliseconds;
}

/// <summary>
/// The main user interface, containing an interactive canvas, an info bar, and
/// expandable/collapsable tips.
/// </summary>
internal sealed class MainPanel : TableLayoutPanel {
    internal static int DefaultDelayInMilliseconds { get; } = 10;

    internal MainPanel(Size canvasSize, HelpViewerSupplier supplier)
    {
        _nonessentialTimer = new(_components) { Interval = 110 };
        _toolTip = new(_components) { ShowAlways = true };
        _helpViewerSupplier = supplier;

        _rect = new Rectangle(Point.Empty, canvasSize);
        _bmp = new Bitmap(width: _rect.Width, height: _rect.Height);
        _graphics = Graphics.FromImage(_bmp);
        _graphics.FillRectangle(Brushes.White, _rect);

        _canvas = new PictureBox {
            Image = _bmp,
            SizeMode = PictureBoxSizeMode.AutoSize,
            Margin = CanvasMargin,
        };

        _alertBar = CreateAlertBar();
        _toggles = CreateToggles();
        _magnify = CreateMagnify();
        _infoBar = CreateInfoBar();
        _tips = CreateTips();

        _neighborEnumerationStrategies = CreateNeighborEnumerationStrategies();

        InitializeMainPanel();

        UpdateStatus();
        UpdateShowHideTips();
        UpdateOpenCloseHelp();

        SubscribeEventHandlers();
    }

    internal int DelayInMilliseconds { get; init; } =
        DefaultDelayInMilliseconds;

    internal bool ShowParentInTaskbar { get; init; } = false;

    internal void Display() => this.Dump("Flood Fill Visualization");

    protected override void Dispose(bool disposing)
    {
        if (disposing) _components.Dispose();
        base.Dispose(disposing);
    }

    private static bool AltIsPressed => Control.ModifierKeys.HasFlag(Keys.Alt);

    private static bool SuperIsPressed
        => Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

    private static int DecideSpeed()
        => (Control.ModifierKeys & (Keys.Shift | Keys.Control)) switch {
            Keys.Shift                =>  1,
            Keys.Control              => 20,
            Keys.Shift | Keys.Control => 10,
            _                         =>  5
        };

    private static void Warn(string message)
        => message.Dump($"Warning ({nameof(MainPanel)})");

    [DllImport("user32.dll")]
    private static extern bool HideCaret(IntPtr hWnd);

    private TableLayoutPanel CreateAlertBar()
    {
        const int padLeft = 3;
        const int padRight = 0;

        var alertBar = new TableLayoutPanel {
            RowCount = 1,
            ColumnCount = 2,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Width = _rect.Width,
            Margin = CanvasMargin,
            Padding = new(left: padLeft, top: 0, right: padRight, bottom: 0),
            BackColor = AlertBackgroundColor,
            Visible = false,
        };

        alertBar.Controls.Add(_alert);
        alertBar.Controls.Add(_dismiss);
        alertBar.Height = _dismiss.Height; // Must be after adding _dismiss.

        // TODO: Someday, figure out why every attempt to do this in a
        // reasonable way failed (and why some raised NullReferenceException).
        _alert.Width = alertBar.Width - (_dismiss.Width + padLeft + padRight);

        return alertBar;
    }

    private TableLayoutPanel CreateToggles()
    {
        var toggles = new TableLayoutPanel {
            RowCount = 1,
            ColumnCount = 2,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Margin = Padding.Empty,
        };

        toggles.Controls.Add(_showHideTips);
        toggles.Controls.Add(_openCloseHelp);

        return toggles;
    }

    private Button CreateMagnify()
        => new ApplicationButton(Files.GetSystem32ExePath("magnify"),
                                 _showHideTips.Height,
                                 _toolTip);

    private TableLayoutPanel CreateInfoBar()
    {
        var infoBar = new TableLayoutPanel {
            RowCount = 1,
            ColumnCount = 3,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Width = _rect.Width,
        };

        infoBar.Controls.Add(_status, column: 1, row: 0);
        infoBar.Controls.Add(_toggles, column: 2, row: 0);
        infoBar.Height = _toggles.Height; // Must be after adding _toggles.
        infoBar.Controls.Add(_magnify, column: 0, row: 0);

        return infoBar;
    }

    private WebBrowser CreateTips() => new() {
        Visible = false,
        Size = new(width: _rect.Width, height: 200),
        AutoSize = true,
        ScrollBarsEnabled = false,
        Url = Files.GetDocUrl("tips.html"),
    };

    private Carousel<NeighborEnumerationStrategy>
    CreateNeighborEnumerationStrategies()
        => new(new UniformStrategy(),
               new RandomPerFillStrategy(_generator),
               new RandomEachTimeStrategy(_generator),
               new RandomPerPixelStrategy(_rect.Size, _generator));

    private void InitializeMainPanel()
    {
        RowCount = 4;
        ColumnCount = 1;
        GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        AutoScroll = true;

        Controls.Add(_alertBar);
        Controls.Add(_canvas);
        Controls.Add(_infoBar);
        Controls.Add(_tips);
    }

    private void UpdateStatus()
    {
        var strategy = _neighborEnumerationStrategies.Current.ToString();
        var speed = DecideSpeed();

        if ((strategy, speed, _jobs) == (_oldStrategy, _oldSpeed, _oldJobs))
            return;

        _oldStrategy = strategy;
        _oldSpeed = speed;
        _oldJobs = _jobs;

        _status.Text =
            $"Neighbors: {strategy}   Speed: {speed}   Jobs: {_jobs}";
    }

    private void UpdateShowHideTips()
    {
        if (_tips.Visible) {
            _showHideTips.Text = "Hide Tips";
            _toolTip.SetToolTip(_showHideTips, "Collapse brief help below");
        } else {
            _showHideTips.Text = "Show Tips";
            _toolTip.SetToolTip(_showHideTips, "Expand brief help below");
        }
    }

    private void UpdateOpenCloseHelp()
    {
        if (_helpPanel is null) {
            _openCloseHelp.Text = "Open Help";
            _toolTip.SetToolTip(_openCloseHelp, "View full help in new panel");
        } else {
            _openCloseHelp.Text = "Close Help";
            _toolTip.SetToolTip(_openCloseHelp, "Close panel with full help");
        }

        _openCloseHelp.Enabled = true;
    }

    private void SubscribeEventHandlers()
    {
        HandleCreated += MainPanel_HandleCreated;
        VisibleChanged += MainPanel_VisibleChanged;
        _nonessentialTimer.Tick += delegate { UpdateStatus(); };

        _alert.GotFocus += alert_GotFocus;
        _dismiss.Click += delegate { _alertBar.Hide(); };

        _canvas.MouseMove += canvas_MouseMove;
        _canvas.MouseClick += canvas_MouseClick;
        _canvas.MouseWheel += canvas_MouseWheel;

        _showHideTips.Click += showHideTips_Click;
        _openCloseHelp.Click += openCloseHelp_Click;
        _tips.DocumentCompleted += tips_DocumentCompleted;
    }

    private void MainPanel_HandleCreated(object? sender, EventArgs e)
    {
        var pluginForm = (Form)Parent;

        if (ShowParentInTaskbar) pluginForm.ShowInTaskbar = true;

        // Update "Speed" in status from modifier keys, crisply when
        // reasonable. Unlike with an ordinary form, users can't readily see if
        // a PluginForm is active (and it starts inactive) so update it, albeit
        // slower, even when not.
        pluginForm.KeyPreview = true;
        pluginForm.KeyDown += delegate { UpdateStatus(); };
        pluginForm.KeyUp += delegate { UpdateStatus(); };
        pluginForm.Activated += delegate { _nonessentialTimer.Stop(); };
        pluginForm.Deactivate += delegate { _nonessentialTimer.Start(); };
        _nonessentialTimer.Start();
    }

    private void MainPanel_VisibleChanged(object? sender, EventArgs e)
    {
        if (_shownBefore || !Visible) return;

        _shownBefore = true;

        if (VScroll) {
            ShowAlert("Low vertical space."
                    + " Rearranging panels (Ctrl+F8) may help.");
        }
    }

    private void alert_GotFocus(object? sender, EventArgs e)
    {
        if (!HideCaret(_alert.Handle)) Warn("Failure hiding alert caret");
    }

    private void canvas_MouseMove(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) {
            _graphics.DrawLine(_pen, _oldLocation, e.Location);
            _canvas.Invalidate();
        }

        _oldLocation = e.Location;
    }

    private async void canvas_MouseClick(object? sender, MouseEventArgs e)
    {
        if (!_rect.Contains(e.Location)) return;

        switch (e.Button) {
        case MouseButtons.Left when SuperIsPressed:
            InstantFill(e.Location, Color.Black);
            break;

        case MouseButtons.Left:
            _bmp.SetPixel(e.Location.X, e.Location.Y, Color.Black);
            _canvas.Invalidate();
            break;

        case MouseButtons.Right when SuperIsPressed:
            await RecursiveFloodFillAsync(e.Location, Color.Orange);
            break;

        case MouseButtons.Right when AltIsPressed:
            await FloodFillAsync(new RandomFringe<Point>(_generator),
                                 e.Location,
                                 Color.Yellow);
            break;

        case MouseButtons.Right:
            await FloodFillAsync(new StackFringe<Point>(),
                                 e.Location,
                                 Color.Red);
            break;

        case MouseButtons.Middle when AltIsPressed:
            await FloodFillAsync(new DequeFringe<Point>(_generator),
                                 e.Location,
                                 Color.Purple);
            break;

        case MouseButtons.Middle:
            await FloodFillAsync(new QueueFringe<Point>(),
                                 e.Location,
                                 Color.Blue);
            break;

        default:
            break; // Other buttons do nothing.
        }
    }

    private void canvas_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (!_rect.Contains(e.Location)) return;

        var scrollingDown = e.Delta < 0;
        if (e.Delta == 0) return; // I'm not sure if this is possible.
        ((HandledMouseEventArgs)e).Handled = true;

        if (!Control.ModifierKeys.HasFlag(Keys.Shift)) {
            // Scrolling without Shift cycles neighbor enumeration strategies.
            if (scrollingDown)
                _neighborEnumerationStrategies.CycleNext();
            else
                _neighborEnumerationStrategies.CyclePrev();
        } else if (_neighborEnumerationStrategies.Current
                    is ConfigurableNeighborEnumerationStrategy strategy) {
            // Scrolling with Shift cycles substrategies instead.
            if (scrollingDown)
                strategy.CycleNextSubStrategy();
            else
                strategy.CyclePrevSubStrategy();
        } // TODO: Maybe show a message on a nonconfigurable current strategy.

        UpdateStatus();
    }

    private void showHideTips_Click(object? sender, EventArgs e)
    {
        _tips.Visible = !_tips.Visible;
        UpdateShowHideTips();
    }

    private async void openCloseHelp_Click(object? sender, EventArgs e)
    {
        if (_helpPanel is null)
            await OpenHelp();
        else
            _helpPanel.Close();
    }

    private void tips_DocumentCompleted(object sender,
                                        WebBrowserDocumentCompletedEventArgs e)
        => _tips.Size = _tips.Document.Body.ScrollRectangle.Size;

    private void helpPanel_PanelClosed(object? sender, EventArgs e)
    {
        _helpPanel = null;
        UpdateOpenCloseHelp();
    }

    private static void help_Navigating(object sender,
                                        HelpViewerNavigatingEventArgs e)
    {
        // Make sure this link would actually be opened in a web browser, i.e.,
        // a program (or COM object) registered as an appropriate protocol
        // handler. We can't guarantee the user won't manage to navigate
        // somewhere that offers up a hyperlink that starts with something
        // that ShellExecute will take to be a Windows executable. Thus even
        // if a better way to ensure we're only specially handling navigation
        // to external sites is used, this protocol check is still essential
        // for security.
        if (e.Uri.IsHttpsOrHttp()) {
            e.Cancel = true;
            Shell.Execute(e.Uri.AbsoluteUri);
        }
    }

    private void ShowAlert(string message)
    {
        _alert.Text = message;
        _alertBar.Show();
    }

    private async Task OpenHelp()
    {
        const string title = "Flood Fill Visualization - Help";

        _openCloseHelp.Enabled = false;

        var help = await _helpViewerSupplier();
        help.Source = Files.GetDocUrl("help.html");
        help.Navigating += help_Navigating;
        _helpPanel = PanelManager.DisplayControl(help.WrappedControl, title);
        _helpPanel.PanelClosed += helpPanel_PanelClosed;

        UpdateOpenCloseHelp();
    }

    private async Task FloodFillAsync(IFringe<Point> fringe,
                                      Point start,
                                      Color toColor)
    {
        var fromArgb = _bmp.GetPixel(start.X, start.Y).ToArgb();
        if (fromArgb == toColor.ToArgb()) return;

        var speed = DecideSpeed();
        var supplier = _neighborEnumerationStrategies.Current.GetSupplier();
        ++_jobs;
        UpdateStatus();
        var area = 0;

        for (fringe.Insert(start); fringe.Count != 0; ) {
            var src = fringe.Extract();

            if (!_rect.Contains(src)
                    || _bmp.GetPixel(src.X, src.Y).ToArgb() != fromArgb)
                continue;

            if (area++ % speed == 0) await Task.Delay(DelayInMilliseconds);

            _bmp.SetPixel(src.X, src.Y, toColor);
            _canvas.Invalidate();

            foreach (var dest in supplier(src)) fringe.Insert(dest);
        }

        --_jobs;
        UpdateStatus();
    }

    // TODO: Refactor to eliminate (or at least decrease) code duplication
    // between FloodFillAsync and RecursiveFloodFillAsync.
    private async Task RecursiveFloodFillAsync(Point start, Color toColor)
    {
        var fromArgb = _bmp.GetPixel(start.X, start.Y).ToArgb();
        if (fromArgb == toColor.ToArgb()) return;

        var speed = DecideSpeed();
        var supplier = _neighborEnumerationStrategies.Current.GetSupplier();
        var area = 0;

        async Task FillFromAsync(Point src)
        {
            if (!_rect.Contains(src)
                    || _bmp.GetPixel(src.X, src.Y).ToArgb() != fromArgb)
                return;

            if (area++ % speed == 0) await Task.Delay(DelayInMilliseconds);

            _bmp.SetPixel(src.X, src.Y, toColor);
            _canvas.Invalidate();

            foreach (var dest in supplier(src)) await FillFromAsync(dest);
        }

        ++_jobs;
        UpdateStatus();
        await FillFromAsync(start);
        --_jobs;
        UpdateStatus();
    }

    private void InstantFill(Point start, Color toColor)
    {
        var toArgb = toColor.ToArgb();

        using (var lb = new LockedBits(_bmp, _rect)) {
            var fromArgb = lb[start.X, start.Y];
            if (fromArgb == toArgb) return;

            var fringe = new Stack<(int x, int y)>();

            for (fringe.Push((start.X, start.Y)); fringe.Count != 0; ) {
                var (x, y) = fringe.Pop();
                if (!lb.Has(x, y) || lb[x, y] != fromArgb) continue;

                lb[x, y] = toArgb;

                fringe.Push((x - 1, y));
                fringe.Push((x + 1, y));
                fringe.Push((x, y - 1));
                fringe.Push((x, y + 1));
            }
        }

        _canvas.Invalidate();
    }

    private static Padding CanvasMargin { get; } =
        new(left: 2, top: 2, right: 0, bottom: 0);

    private static Color AlertBackgroundColor { get; } = Color.NavajoWhite;

    private readonly IContainer _components = new Container();

    private readonly System.Windows.Forms.Timer _nonessentialTimer;

    private readonly ToolTip _toolTip;

    private readonly HelpViewerSupplier _helpViewerSupplier;

    private readonly Rectangle _rect;

    private readonly Bitmap _bmp;

    private readonly Graphics _graphics;

    private readonly PictureBox _canvas;

    private readonly Pen _pen = new(Color.Black);

    private Point _oldLocation = Point.Empty;

    private readonly TableLayoutPanel _alertBar;

    private readonly TextBox _alert = new() {
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = Padding.Empty,
        BorderStyle = BorderStyle.None,
        Font = new("Segoe UI Semibold", 10),
        BackColor = AlertBackgroundColor,
        ForeColor = Color.Black,
        ReadOnly = true,
        Cursor = Cursors.Arrow,
        TabStop = false,
    };

    private readonly Button _dismiss = new() {
        Text = "Dismiss",
        BackColor = Button.DefaultBackColor,
        AutoSize = true,
        Anchor = AnchorStyles.Right,
        Margin = Padding.Empty,
    };

    private readonly TableLayoutPanel _infoBar;

    private readonly Label _status = new() {
        AutoSize = true,
        Font = new(Label.DefaultFont.FontFamily, 10),
    };

    private readonly TableLayoutPanel _toggles;

    private readonly Button _magnify;

    private readonly Button _showHideTips = new() {
        Text = "??? Tips", // Placeholder text for height computation.
        AutoSize = true,
        Margin = new(left: 0, top: 0, right: 2, bottom: 0),
    };

    private readonly Button _openCloseHelp = new() {
        Text = "??? Help", // Placeholder text for height computation.
        AutoSize = true,
        Margin = new(left: 2, top: 0, right: 0, bottom: 0),
    };

    private readonly WebBrowser _tips;

    private OutputPanel? _helpPanel = null;

    private readonly Func<int, int> _generator =
        Permutations.CreateRandomGenerator();

    private readonly Carousel<NeighborEnumerationStrategy>
    _neighborEnumerationStrategies;

    private string _oldStrategy = string.Empty;

    private int _oldSpeed = -1;

    private int _oldJobs = -1;

    private int _jobs = 0;

    private bool _shownBefore = false;
};

/// <summary>
/// A button to launch an application, showing an icon and (optionally) toolip
/// obtained from the executable's metadata.
/// </summary>
internal sealed class ApplicationButton : Button {
    internal ApplicationButton(string path,
                               int sideLength,
                               ToolTip? toolTip = null)
    {
        _path = path;

        BackgroundImage = CreateBitmap(_path);
        BackgroundImageLayout = ImageLayout.Stretch;
        Size = new(width: sideLength, height: sideLength);
        Margin = Padding.Empty;

        toolTip?.SetToolTip(
            this,
            FileVersionInfo.GetVersionInfo(_path).FileDescription);

        Click += ApplicationButton_Click;
    }

    private static Bitmap CreateBitmap(string path)
    {
        var hIcon = ExtractIcon(Process.GetCurrentProcess().Handle, path, 0);
        try {
            return Icon.FromHandle(hIcon).ToBitmap();
        } finally {
            DestroyIcon(hIcon);
        }
    }

    // TODO: Degrade gracefully and use some generic icon on failure instead.
    private static IntPtr ExtractIconOrThrow(IntPtr hInst,
                                             string pszExeFileName,
                                             uint nIconIndex)
    {
        var hIcon = ExtractIcon(hInst, pszExeFileName, nIconIndex);
        if (hIcon == IntPtr.Zero) Throw();
        return hIcon;
    }

    private static void DestroyIconOrThrow(IntPtr hIcon)
    {
        if (!DestroyIcon(hIcon)) Throw();
    }

    private static void Throw()
        => throw new Win32Exception(Marshal.GetLastWin32Error());

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr ExtractIcon(IntPtr hInst,
                                             string pszExeFileName,
                                             uint nIconIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private void ApplicationButton_Click(object? sender, EventArgs e)
        => Shell.Execute(_path);

    private readonly string _path;
}

/// <summary>
/// Provides data for the <see cref="HelpViewer.Navigating"/> event.
/// </summary>
internal sealed class HelpViewerNavigatingEventArgs : EventArgs {
    internal HelpViewerNavigatingEventArgs(Uri uri) => Uri = uri;

    internal Uri Uri { get; }

    internal bool Cancel { get; set; } = false;
}

/// <summary>
/// Represents a method that will handle the
/// <see cref="HelpViewer.Navigating"/> event.
/// </summary>
internal delegate void
HelpViewerNavigatingEventHandler(HelpViewer sender,
                                 HelpViewerNavigatingEventArgs e);

/// <summary>
/// Wrapper allowing a choice of web-browsing controls as a help browser.
/// </summary>
internal abstract class HelpViewer {
    /// <summary>The URL of the current top-level document.</summary>
    internal abstract Uri Source { get; set; }

    /// <summary>Occurs before navigation to a new document.</summary>
    internal event HelpViewerNavigatingEventHandler? Navigating = null;

    /// <summary>The wrapped control. The user interacts with this.</summary>
    internal abstract Control WrappedControl { get; }

    /// <summary>Invokes the <see cref="Navigating"/> event.</summary>
    private protected void OnNavigating(HelpViewerNavigatingEventArgs e)
        => Navigating?.Invoke(this, e);
}

/// <summary>
/// <see cref="System.Windows.Forms.WebBrowser"/>-based help viewer.
/// </summary>
internal sealed class WebBrowserHelpViewer : HelpViewer {
    internal static HelpViewer Create() => new WebBrowserHelpViewer();

    /// <inheritdoc/>
    internal override Uri Source {
        get => _webBrowser.Url;
        set => _webBrowser.Url = value;
    }

    /// <inheritdoc/>
    internal override Control WrappedControl => _webBrowser;

    private WebBrowserHelpViewer()
        => _webBrowser.Navigating += webBrowser_Navigating;

    private void webBrowser_Navigating(object sender,
                                       WebBrowserNavigatingEventArgs e)
    {
        var eHelpViewer = new HelpViewerNavigatingEventArgs(e.Url);
        OnNavigating(eHelpViewer);
        e.Cancel = eHelpViewer.Cancel;
    }

    private readonly WebBrowser _webBrowser = new();
}

/// <summary>
/// <see cref="Microsoft.Web.WebView2.WinForms.WebView2"/>-based help viewer.
/// </summary>
internal sealed class WebView2HelpViewer : HelpViewer {
    internal async static Task<HelpViewer> CreateAsync()
    {
        var webView2 = new MyWebView2();
        await webView2.EnsureCoreWebView2Async();

        var settings = webView2.CoreWebView2.Settings;
        settings.AreHostObjectsAllowed = false;
        settings.IsWebMessageEnabled = false;
        settings.IsScriptEnabled = false;
        settings.AreDefaultScriptDialogsEnabled = false;

        return new WebView2HelpViewer(webView2);
    }

    /// <inheritdoc/>
    internal override Uri Source {
        get => _webView2.Source;
        set => _webView2.Source = value;
    }

    /// <inheritdoc/>
    internal override Control WrappedControl => _webView2;

    private WebView2HelpViewer(WebView2 webView2)
    {
        _webView2 = webView2;
        _webView2.NavigationStarting += webView2_NavigationStarting;
    }

    private void
    webView2_NavigationStarting(object? sender,
                                CoreWebView2NavigationStartingEventArgs e)
    {
        var eHelpViewer = new HelpViewerNavigatingEventArgs(new Uri(e.Uri));
        OnNavigating(eHelpViewer);
        e.Cancel = eHelpViewer.Cancel;
    }

    private readonly WebView2 _webView2;
}

/// <summary>
/// Encapsulates a method that supplies a <see cref="HelpViewer"/>.
/// </summary>
internal delegate Task<HelpViewer> HelpViewerSupplier();

/// <summary>
/// Hack to work around <a href="https://github.com/MicrosoftEdge/WebView2Feedback/issues/442">System.NullReferenceException upon WebView2.Dispose</a>.
/// </summary>
/// <remarks>
/// Remove and replace all uses with <c>WebView2</c> when the bug is fixed.
/// </remarks>
internal sealed class MyWebView2 : WebView2 {
    protected override void OnVisibleChanged(EventArgs e)
    {
        if (CoreWebView2 is not null) base.OnVisibleChanged(e);
    }
}

/// <summary>
/// Convenience methods for getting information about files and directories
/// this program uses and expects to be present.
/// </summary>
internal static class Files {
    internal static Uri GetDocUrl(string filename)
        => new(Path.Combine(QueryDirectory, filename));

    internal static string GetSystem32ExePath(string basename)
        => Path.Combine(WindowsDirectory, "system32", $"{basename}.exe");

    private static string QueryDirectory
        => Path.GetDirectoryName(Util.CurrentQueryPath)
            ?? throw new NotSupportedException("Can't find query directory.");

    private static string WindowsDirectory
        => Environment.GetEnvironmentVariable("windir")
            ?? throw new InvalidOperationException(
                    "Can't find Windows directory.");
}

/// <summary>
/// Convenience methods for interacting with the Windows shell.
/// </summary>
internal static class Shell {
    internal static void Execute(string path)
        => Process.Start(new ProcessStartInfo() {
            FileName = path,
            UseShellExecute = true,
        });
}

/// <summary>
/// Represents a collection of vertices that have been encountered during
/// graph traversal and must still be visited.
/// </summary>
/// <typeparam name="T">The vertex type.</typeparam>
internal interface IFringe<T> {
    /// <summary>The number of vertices currently stored.</summary>
    int Count { get; }

    /// <summary>Puts a vertex into this fringe.</summary>
    /// <param name="vertex">The vertex to be inserted.</param>
    void Insert(T vertex);

    /// <summary>Takes a vertex out of this fringe.</summary>
    /// <returns>The extracted vertex.</returns>
    T Extract();
}

/// <summary>
/// A last-in first-out (LIFO, i.e., stack) collection of vertices for graph
/// traversal.
/// </summary>
/// <typeparam name="T">The vertex type.</typeparam>
internal sealed class StackFringe<T> : IFringe<T> {
    /// <inheritdoc/>
    public int Count => _stack.Count;

    /// <inheritdoc/>
    public void Insert(T vertex) => _stack.Push(vertex);

    /// <inheritdoc/>
    public T Extract() => _stack.Pop();

    private readonly Stack<T> _stack = new();
}

/// <summary>
/// A first-in first-out (FIFO, i.e., "queue") collection of vertices for
/// graph traversal.
/// </summary>
/// <typeparam name="T">The vertex type.</typeparam>
internal sealed class QueueFringe<T> : IFringe<T> {
    /// <inheritdoc/>
    public int Count => _queue.Count;

    /// <inheritdoc/>
    public void Insert(T vertex) => _queue.Enqueue(vertex);

    /// <inheritdoc/>
    public T Extract() => _queue.Dequeue();

    private readonly Queue<T> _queue = new();
}

/// <summary>
/// A collection of vertices for graph traversal that selects vertices for
/// extraction by uniform random sampling.
/// </summary>
/// <typeparam name="T">The vertex type.</typeparam>
internal sealed class RandomFringe<T> : IFringe<T> {
    internal RandomFringe(Func<int, int> generator) => _generator = generator;

    /// <inheritdoc/>
    public int Count => _items.Count;

    /// <inheritdoc/>
    public void Insert(T vertex) => _items.Add(vertex);

    /// <inheritdoc/>
    public T Extract()
    {
        var index = _generator(Count);
        var vertex = _items[index];
        _items[index] = _items[Count - 1];
        _items.RemoveAt(Count - 1);
        return vertex;
    }

    private readonly List<T> _items = new();

    private readonly Func<int, int> _generator;
}

/// <summary>
/// A deque-based collection of vertices for graph traversal that selects the
/// most or least recently inserted vertex with equal probability.
/// </summary>
/// <typeparam name="T">The vertex type.</typeparam>
internal sealed class DequeFringe<T> : IFringe<T> {
    internal DequeFringe(Func<int, int> generator) => _generator = generator;

    /// <inheritdoc/>
    public int Count => _deque.Count;

    /// <inheritdoc/>
    public void Insert(T vertex) => _deque.AddToBack(vertex);

    /// <inheritdoc/>
    public T Extract() => _generator(2) switch {
        0 => _deque.RemoveFromFront(),
        1 => _deque.RemoveFromBack(),
        var other
            => throw new IndexOutOfRangeException($"Need 0 or 1, got {other}.")
    };

    private readonly Deque<T> _deque = new();

    private readonly Func<int, int> _generator;
}

/// <summary>
/// A direction to look for a neighbor in a bitmap whose pixels are regarded as
/// vertices with four-way adjacency.
/// </summary>
internal enum Direction {
    Left,
    Right,
    Up,
    Down,
}

/// <summary>
/// Provides an extension method for finding an adjacent point (in an image)
/// in a specified direction.
/// </summary>
internal static class PointExtensions {
    internal static Point Go(this Point src, Direction direction)
        => direction switch {
            Direction.Left  => new(src.X - 1, src.Y),
            Direction.Right => new(src.X + 1, src.Y),
            Direction.Up    => new(src.X, src.Y - 1),
            Direction.Down  => new(src.X, src.Y + 1),
            _ => throw new NotSupportedException("Bug: unrecognized direction")
        };
}

/// <summary>
/// Provies an extension method for deconstructing the size (of an image or
/// control) into its constituent dimensions.
/// </summary>
internal static class SizeExtensions {
    internal static void Deconstruct(this Size size,
                                     out int width,
                                     out int height)
        => (width, height) = (size.Width, size.Height);
}

/// <summary>
/// Provides an extension method for checking a URI's scheme case-insensitively
/// against one or more other schemes.
/// </summary>
internal static class UriExtensions {
    internal static bool IsHttpsOrHttp(this Uri uri)
        => uri.IsScheme(Uri.UriSchemeHttp) || uri.IsScheme(Uri.UriSchemeHttps);

    private static bool IsScheme(this Uri uri, string scheme)
        => uri.Scheme.Equals(scheme, StringComparison.Ordinal);
}

/// <summary>
/// This abstract base class represents an order in which to enumerate
/// neighbors while traversing the implicit graph of a bitmap image with
/// four-way adjacency, or an algorithm for determining such an order.
/// </summary>
internal abstract class NeighborEnumerationStrategy {
    private protected NeighborEnumerationStrategy(string name) => Name = name;

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>The name of this strategy, to appear in the UI.</summary>
    /// <remarks>Not affected by substrategy, if any.</remarks>
    private protected string Name { get; }

    /// <summary>
    /// Creates a delegate that accepts a point and returns and array of its
    /// four neighbors, in an order determined by the concrete implementation.
    /// </summary>
    internal abstract Func<Point, Point[]> GetSupplier();
}

/// <summary>
/// Represents an algorithm for determining an order in which to enumerate
/// neighbors while traversing the implicit graph of a bitmap image with
/// four-way adjacency, which is configurable by selecting a substrategy.
/// </summary>
internal abstract class ConfigurableNeighborEnumerationStrategy
        : NeighborEnumerationStrategy {
    private protected ConfigurableNeighborEnumerationStrategy(string name)
        : base(name) { }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} - {Detail}";

    /// <summary>
    /// Switches to the next substrategy, or from the last to the first.
    /// </summary>
    internal abstract void CycleNextSubStrategy();

    /// <summary>
    /// Switches to the previous substrategy, or from the first to the last.
    /// </summary>
    internal abstract void CyclePrevSubStrategy();

    /// <summary>
    /// Inforamtion about the current substrategy, to appear in the UI.
    /// </summary>
    private protected abstract string Detail { get; }
}

/// <summary>
/// Enumerates neighbors in the same order, within and across fills.
/// </summary>
/// <remarks>The substrategy determines the specific order used.</remarks>
internal sealed class UniformStrategy
        : ConfigurableNeighborEnumerationStrategy {
    internal UniformStrategy() : this(FastEnumInfo<Direction>.GetValues()) { }

    internal UniformStrategy(params Direction[] uniformOrder) : base("Uniform")
        => _uniformOrder = uniformOrder[..];

    /// <inheritdoc/>
    private protected override string Detail
        => new string(Array.ConvertAll(_uniformOrder,
                                       direction => direction.ToString()[0]));

    /// <inheritdoc/>
    internal override Func<Point, Point[]> GetSupplier()
    {
        var uniformOrder = _uniformOrder[..];

        return src => Array.ConvertAll(uniformOrder,
                                       direction => src.Go(direction));
    }

    /// <inheritdoc/>
    internal override void CycleNextSubStrategy()
        => _uniformOrder.CycleNextPermutation();

    /// <inheritdoc/>
    internal override void CyclePrevSubStrategy()
        => _uniformOrder.CyclePrevPermutation();

    private readonly Direction[] _uniformOrder;
}

/// <summary>
/// Enumerates neighbors in an order that differs randomly across fills but is
/// the same within any one fill.
/// </summary>
internal sealed class RandomPerFillStrategy : NeighborEnumerationStrategy {
    internal RandomPerFillStrategy(Func<int, int> generator)
            : base("Random per fill")
        => _generator = generator;

    /// <inheritdoc/>
    internal override Func<Point, Point[]> GetSupplier()
    {
        var order = FastEnumInfo<Direction>.GetValues();
        order.Shuffle(_generator);
        return src => Array.ConvertAll(order, direction => src.Go(direction));
    }

    private readonly Func<int, int> _generator;
}

/// <summary>
/// Enumerates neighbors in an order that differs randomly each each time, even
/// within the same fill, and even (as can happen in the case of interference
/// from a concurrent fill) for multiple traversals from the same source pixel
/// in the same fill.
/// </summary>
internal sealed class RandomEachTimeStrategy : NeighborEnumerationStrategy {
    internal RandomEachTimeStrategy(Func<int, int> generator)
            : base("Random always")
        => _supply = src => {
            var neighbors = FastEnumInfo<Direction>.ConvertValues(
                                direction => src.Go(direction));
            neighbors.Shuffle(generator);
            return neighbors;
        };

    /// <inheritdoc/>
    internal override Func<Point, Point[]> GetSupplier() => _supply;

    private readonly Func<Point, Point[]> _supply;
}

/// <summary>
/// Enumerates neighbors in randomly determined orders that differ between
/// pixels. Each pixel has a random order that is fixed even across fills.
/// </summary>
internal sealed class RandomPerPixelStrategy : NeighborEnumerationStrategy {
    internal RandomPerPixelStrategy(Size size, Func<int, int> generator)
            : base("Random per pixel")
    {
        var perPixelOrders = GeneratePerPixelOrders(size, generator);

        _supplier = src => Array.ConvertAll(perPixelOrders[src.X, src.Y],
                                            direction => src.Go(direction));
    }

    /// <inheritdoc/>
    internal override Func<Point, Point[]> GetSupplier() => _supplier;

    private static Direction[,][]
    GeneratePerPixelOrders(Size size, Func<int, int> generator)
    {
        var perPixelOrders = new Direction[size.Width, size.Height][];

        for (var x = 0; x < size.Width; ++x) {
            for (var y = 0; y < size.Height; ++y) {
                var directions = FastEnumInfo<Direction>.GetValues();
                directions.Shuffle(generator);
                perPixelOrders[x, y] = directions;
            }
        }

        return perPixelOrders;
    }

    private readonly Func<Point, Point[]> _supplier;
}

// TODO: Add WhirlpoolStrategy (which will be (counter)clockwise configurable).

/// <summary>A cyclic sequence of items and a current position in it.</summary>
internal sealed class Carousel<T> {
    internal Carousel(params T[] items) => _items = items[..];

    internal T Current => _items[_pos];

    internal void CycleNext() => Change(+1);

    internal void CyclePrev() => Change(-1);

    private void Change(int delta)
        => _pos = (_pos + delta + _items.Length) % _items.Length;

    private readonly T[] _items;

    private int _pos = 0;
}


/// <summary>
/// Methods for generating random and non-random permutations.
/// </summary>
internal static class Permutations {
    internal static Func<int, int> CreateRandomGenerator()
        => new Random(RandomNumberGenerator.GetInt32(int.MaxValue)).Next;

    internal static void Shuffle<T>(this T[] items, Func<int, int> generator)
    {
        for (var right = items.Length; right > 0; --right) {
            var left = generator(right);
            items.Swap(left, right - 1);
        }
    }

    internal static void CycleNextPermutation<T>(this T[] items)
        => items.CyclePermutation(Comparer<T>.Default);

    internal static void CyclePrevPermutation<T>(this T[] items)
        => items.CyclePermutation(ReverseComparer<T>.Default);

    // This cycles from max to min but is otherwise the algorithm described in:
    // https://en.wikipedia.org/wiki/Permutation#Generation_in_lexicographic_order
    private static void CyclePermutation<T>(this T[] items,
                                            IComparer<T> comparer)
    {
        var right = items.Length - 2;
        while (right >= 0
                && comparer.Compare(items[right], items[right + 1]) >= 0)
            --right;

        if (right < 0) {
            // This is the last permutation (w.r.t. comparer) so cycle around.
            Array.Reverse(items);
        } else {
            // Go to the permutation that comes next (w.r.t. comparer).
            var left = items.Length - 1;
            while (comparer.Compare(items[right], items[left]) >= 0) --left;
            items.Swap(left, right);
            items.ReverseBetween(right + 1, items.Length);
        }
    }

    private static void Swap<T>(this T[] items, int i, int j)
        => (items[i], items[j]) = (items[j], items[i]);

    private static void ReverseBetween<T>(this T[] items,
                                          int startInclusive,
                                          int endExclusive)
        => Array.Reverse(items, startInclusive, endExclusive - startInclusive);
}

/// <summary>Adaptor for reversing the direction of a comparer.</summary>
/// <typeparam name="T">The element type the comparer accepts.</typeparam>
internal sealed class ReverseComparer<T> : IComparer<T> {
    internal static IComparer<T> Default { get; } =
        new ReverseComparer<T>(Comparer<T>.Default);

    public int Compare(T? lhs, T? rhs) => _comparer.Compare(rhs, lhs);

    internal ReverseComparer(IComparer<T> comparer) => _comparer = comparer;

    private readonly IComparer<T> _comparer;
}

/// <summary>
/// Methods to access information about enums quickly by caching the
/// results of reflection.
/// </summary>
/// <typeparam name="T">The enum to provide information about.</typeparam>
internal static class FastEnumInfo<T> where T : struct, Enum {
    internal static T[] GetValues() => _values[..];

    internal static TOutput[]
    ConvertValues<TOutput>(Converter<T, TOutput> converter)
        => Array.ConvertAll(_values, converter);

    private static readonly T[] _values = Enum.GetValues<T>();
}

/// <summary>
/// A specialized 2-dimensional span managing lifetime and access to a
/// region in a 32-bit ARGB bitmap image.
/// </summary>
internal readonly ref struct LockedBits {
    internal LockedBits(Bitmap bmp, Rectangle rect)
    {
        _bmp = bmp;
        _metadata = _bmp.LockBits(rect,
                                  ImageLockMode.ReadWrite,
                                  PixelFormat.Format32bppArgb);
        Width = _metadata.Width;
        Height = _metadata.Height;
        unsafe {
            _argbs = new(_metadata.Scan0.ToPointer(), Width * Height);
        }
    }

    internal void Dispose() => _bmp.UnlockBits(_metadata);

    internal int this[int x, int y]
    {
        get => _argbs[GetIndex(x, y)];
        set => _argbs[GetIndex(x, y)] = value;
    }

    internal int Width { get; }

    internal int Height { get; }

    internal bool Has(int x, int y) => HasX(x) && HasY(y);

    private bool HasX(int x) => 0 <= x && x < Width;

    private bool HasY(int y) => 0 <= y && y < Height;

    private int GetIndex(int x, int y)
        => Has(x, y)
            ? y * Width + x
            : throw new IndexOutOfRangeException("Coordinates out of range.");

    private readonly Bitmap _bmp;

    private readonly BitmapData _metadata;

    private readonly Span<int> _argbs;
}
