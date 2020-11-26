<Query Kind="Statements">
  <NuGetReference>Nito.Collections.Deque</NuGetReference>
  <Namespace>Key = System.Windows.Input.Key</Namespace>
  <Namespace>Keyboard = System.Windows.Input.Keyboard</Namespace>
  <Namespace>LC = LINQPad.Controls</Namespace>
  <Namespace>Nito.Collections</Namespace>
  <Namespace>static LINQPad.Controls.ControlExtensions</Namespace>
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>System.Drawing</Namespace>
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
    new MainPanel(SuggestCanvasSize()).Display();
}

static Size SuggestCanvasSize()
{
    // TODO: Maybe try to check which screen the LINQPad window is on.
    var (width, height) = Screen.PrimaryScreen.Bounds.Size;
    var sideLength = Math.Min(width, height) * 5 / 9;
    return new(width: sideLength, height: sideLength);
}

static void launcher_Launch(Launcher sender, LauncherEventArgs e)
    => new MainPanel(e.Size) {
        ShowParentInTaskbar = sender.ShowPluginFormInTaskbar,
    }.Display();

/// <summary>
/// Provides a canvas size for the <see cref="Launcher.Launch"/> event.
/// </summary>
internal sealed class LauncherEventArgs : EventArgs {
    internal LauncherEventArgs(int width, int height)
        : this(new(width: width, height: height)) { }

    internal LauncherEventArgs(Size size) => Size = size;

    internal Size Size { get; }
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
        (_width, _height) = defaultSize;

        const string textBoxWidth = "5em";
        _widthBox = new(_width.ToString()) { Width = textBoxWidth };
        _heightBox = new(_height.ToString()) { Width = textBoxWidth };

        _panel = new(horizontal: false,
            new LC.FieldSet("Custom Canvas Size", CreateSizeTable()),
            new LC.FieldSet("Screen Capture Hack", _showPluginFormInTaskbar),
            _launch);

        SubscribePrivateHandlers();
    }

    internal event LauncherEventHandler? Launch = null;

    internal bool ShowPluginFormInTaskbar => _showPluginFormInTaskbar.Checked;

    internal void Display() => _panel.Dump("Developer Mode Launcher");

    private LC.Table CreateSizeTable()
    {
        var table = new LC.Table(noBorders: true,
                                 cellPaddingStyle: ".3em .3em",
                                 cellVerticalAlign: "middle");

        table.Rows.Add(new LC.Label("Width"), _widthBox);
        table.Rows.Add(new LC.Label("Height"), _heightBox);

        return table;
    }

    private void SubscribePrivateHandlers()
    {
        _widthBox.TextInput += widthBox_TextInput;
        _heightBox.TextInput += heightBox_TextInput;
        _launch.Click += launch_Click;
    }

    private void widthBox_TextInput(object? sender, EventArgs e)
        => HandleInput(_widthBox, ref _width);

    private void heightBox_TextInput(object? sender, EventArgs e)
        => HandleInput(_heightBox, ref _height);

    private void launch_Click(object? sender, EventArgs e)
    {
        _widthBox.Enabled = _heightBox.Enabled = _launch.Enabled = false;
        Launch?.Invoke(this, new(width: _width, height: _height));
    }

    private void HandleInput(LC.TextBox sender, ref int sink)
    {
        const int invalid = -1;

        sink = int.TryParse(sender.Text, out var value) && value > 0
                ? value
                : invalid;

        _launch.Enabled = _width != invalid && _height != invalid;
    }

    private readonly LC.TextBox _widthBox;

    private readonly LC.TextBox _heightBox;

    private readonly LC.CheckBox _showPluginFormInTaskbar =
        new LC.CheckBox("Show PluginForm in Taskbar");

    private readonly LC.Button _launch = new LC.Button("Launch!");

    private readonly LC.StackPanel _panel;

    private int _width;

    private int _height;
}

/// <summary>
/// The main user interface, containing an interactive canvas, an info bar, and
/// expandable/collapsable tips.
/// </summary>
internal sealed class MainPanel : TableLayoutPanel {
    internal MainPanel(Size canvasSize)
    {
        _nonessentialTimer = new(_components) { Interval = 110 };
        _toolTip = new(_components) { ShowAlways = true };

        _rect = new Rectangle(Point.Empty, canvasSize);
        _bmp = new Bitmap(width: _rect.Width, height: _rect.Height);
        _graphics = Graphics.FromImage(_bmp);
        _graphics.FillRectangle(Brushes.White, _rect);

        _canvas = new PictureBox {
            Image = _bmp,
            SizeMode = PictureBoxSizeMode.AutoSize,
        };

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
        infoBar.Height = _toggles.Height; // Must be after adding toggles.
        infoBar.Controls.Add(_magnify, column: 0, row: 0);

        return infoBar;
    }

    private WebBrowser CreateTips() => new() {
        Visible = false,
        Size = new(width: _rect.Width, height: 200),
        AutoSize = true,
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
        RowCount = 3;
        ColumnCount = 1;
        GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        AutoScroll = true;

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
    }

    private void SubscribeEventHandlers()
    {
        HandleCreated += MainPanel_HandleCreated;
        _nonessentialTimer.Tick += delegate { UpdateStatus(); };

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
            ImmediateFill(e.Location, Color.Black);
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

    private void openCloseHelp_Click(object? sender, EventArgs e)
    {
        const string title = "Flood Fill Visualization - Help";

        if (_helpPanel is not null) {
            _helpPanel.Close();
            return;
        }

        var help = new WebBrowser { Url = Files.GetDocUrl("help.html") };
        _helpPanel = PanelManager.DisplayControl(help, title);

        _helpPanel.PanelClosed += delegate {
            _helpPanel = null;
            UpdateOpenCloseHelp();
        };

        UpdateOpenCloseHelp();
    }

    private void tips_DocumentCompleted(object sender,
                                        WebBrowserDocumentCompletedEventArgs e)
    {
        var (width, height) = _tips.Document.Body.ScrollRectangle.Size;
        var newSize = new SizeF(width: width * 1.05f, height: height * 1.22f);
        _tips.Size = Size.Round(newSize);
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

            if (area++ % speed == 0) await Task.Delay(10);

            _bmp.SetPixel(src.X, src.Y, toColor);
            _canvas.Invalidate();

            foreach (var dest in supplier(src)) fringe.Insert(dest);
        }

        --_jobs;
        UpdateStatus();
    }

    // FIXME: If this is kept, refactor eliminate (or at least decrease) the
    // code duplication between FloodFillAsync and RecursiveFloodFillAsync.
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

            if (area++ % speed == 0) await Task.Delay(10);

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

    // FIXME: Right now this is implemented analogously to
    // RecursiveFloodFullAsync, to test whether stack overflow is averted there
    // only due to the use of async/await (since the call stack at runtime is
    // built up separately across continuations). At minimum, this should use
    // LockBits. Better would be to use an API function; if Bitmap doesn't
    // provide one, then ExtFloodFill (in the Windows API) could be used.
    private void ImmediateFill(Point start, Color toColor)
    {
        var fromArgb = _bmp.GetPixel(start.X, start.Y).ToArgb();
        if (fromArgb == toColor.ToArgb()) return;

        void FillFrom(Point src)
        {
            if (!_rect.Contains(src)
                    || _bmp.GetPixel(src.X, src.Y).ToArgb() != fromArgb)
                return;

            _bmp.SetPixel(src.X, src.Y, toColor);

            FillFrom(src.Go(Direction.Left));
            FillFrom(src.Go(Direction.Right));
            FillFrom(src.Go(Direction.Up));
            FillFrom(src.Go(Direction.Down));
        }

        FillFrom(start);
        _canvas.Invalidate();
    }

    private readonly IContainer _components = new Container();

    private readonly System.Windows.Forms.Timer _nonessentialTimer;

    private readonly ToolTip _toolTip;

    private readonly Rectangle _rect;

    private readonly Bitmap _bmp;

    private readonly Graphics _graphics;

    private readonly PictureBox _canvas;

    private readonly Pen _pen = new(Color.Black);

    private Point _oldLocation = Point.Empty;

    private readonly TableLayoutPanel _infoBar;

    private readonly Label _status = new() {
        AutoSize = true,
        Font = new(TextBox.DefaultFont.FontFamily, 10),
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
    {
        var process = new Process();
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = _path;
        process.Start();
    }

    private readonly string _path;
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
            ?? throw new NotSupportedException("Can't find query directory");

    private static string WindowsDirectory
        => Environment.GetEnvironmentVariable("windir")
            ?? throw new InvalidOperationException(
                    "Can't find Windows directory");
}

/// <summary>
/// Represents a collection of vertices that have been encountered during
/// graph traversal and must still be visited.
/// </summary>
/// <typeparam name="T">The vertex type.</typeparam>
internal interface IFringe<T> {
    int Count { get; }

    void Insert(T vertex);

    T Extract();
}

/// <summary>
/// A last-in first-out (LIFO, i.e., stack) collection of vertices for graph
/// traversal.
/// </summary>
/// <typeparam name="T">The vertex type.</typeparam>
internal sealed class StackFringe<T> : IFringe<T> {
    /// <summary>The number of vertices currently stored.</summary>
    public int Count => _stack.Count;

    /// <summary>Puts a vertex into this fringe.</summary>
    /// <param name="vertex">The vertex to be inserted.</param>
    public void Insert(T vertex) => _stack.Push(vertex);

    /// <summary>Takes a vertex out of this fringe.</summary>
    /// <returns>The extracted vertex.</returns>
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
