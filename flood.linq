<Query Kind="Statements">
  <NuGetReference>Microsoft.Web.WebView2</NuGetReference>
  <NuGetReference>morelinq</NuGetReference>
  <NuGetReference>Nito.Collections.Deque</NuGetReference>
  <Namespace>Cursor = System.Windows.Forms.Cursor</Namespace>
  <Namespace>Key = System.Windows.Input.Key</Namespace>
  <Namespace>Keyboard = System.Windows.Input.Keyboard</Namespace>
  <Namespace>LC = LINQPad.Controls</Namespace>
  <Namespace>Microsoft.Web.WebView2.Core</Namespace>
  <Namespace>Microsoft.Web.WebView2.WinForms</Namespace>
  <Namespace>Microsoft.Win32</Namespace>
  <Namespace>Nito.Collections</Namespace>
  <Namespace>static LINQPad.Controls.ControlExtensions</Namespace>
  <Namespace>static MoreLinq.Extensions.PairwiseExtension</Namespace>
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
  <Namespace>System.Security</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <Namespace>System.Windows.Forms.DataVisualization.Charting</Namespace>
  <Namespace>Timer = System.Windows.Forms.Timer</Namespace>
  <RuntimeVersion>5.0</RuntimeVersion>
</Query>

// flood.linq - Interactive flood-fill visualizer.

#nullable enable

const float defaultScreenFractionForCanvas = 5.0f / 9.0f;

var devmode = Control.ModifierKeys.HasFlag(Keys.Shift);

// Make dump headings bigger. (See Launcher.Display for further customization.)
Util.RawHtml("<style>h1.headingpresenter { font-size: 1rem }</style>").Dump();

if (devmode) {
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
    var (screenWidth, screenHeight) = Screen.PrimaryScreen.Bounds.Size;

    var sideLength = (int)(Math.Min(screenWidth, screenHeight)
                            * defaultScreenFractionForCanvas);

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

    var ui = new MainPanel(e.Size, supplier) {
        DelayInMilliseconds = sender.DelayInMilliseconds,
        ShowParentInTaskbar = sender.ShowPluginFormInTaskbar,
        MagnifierButtonVisible = sender.ShowMagnifierButton,
        StopButtonVisible = sender.ShowStopButton,
        ChartingButtonVisible = sender.ShowChartButton,
    };

    ui.Activated += delegate { sender.PauseUpdates(); };
    ui.Deactivate += delegate { sender.ResumeUpdates(); };

    ui.Display();
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
/// "Developer mode" launcher allowing the user to specify a canvas size and
/// other advanced configuration.
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
            new LC.FieldSet("Features", CreateFeaturesPanel()),
            new LC.FieldSet("Features (Experimental)",
                            CreateExperimentalFeaturesPanel()),
            new LC.WrapPanel(_launch, _postLaunch));

        SubscribePrivateHandlers();
    }

    internal event LauncherEventHandler? Launch = null;

    internal int DelayInMilliseconds
        => _delay is int delay
            ? delay
            : throw new NotSupportedException(
                "Bug: Launch button enabled without delay set.");

    internal bool ShowPluginFormInTaskbar => _showPluginFormInTaskbar.Checked;

    internal bool ShowMagnifierButton => _magnifier.Checked;

    internal bool ShowStopButton => _stopButton.Checked;

    internal bool ShowChartButton => _charting.Checked;

    internal void Display()
    {
        // Make launcher text 12.5% bigger than with LINQPad's default CSS.
        // Do it here instead of globally so debugging dumps have normal size.
        Util.WithStyle(_panel, "font-size: .9rem")
            .Dump("Developer Mode Launcher");

        ResumeUpdates();
    }

    internal void PauseUpdates() => _metatimer.Stop();

    internal void ResumeUpdates() => _metatimer.Start();

    private const string NumberBoxWidth = "5em";

    // NtQueryTimerResolution returns times in units of 100 ns.
    private const double HundredNanosecondsPerMillisecond = 10_000.0;

    private const int MetaTimerInterval = 150; // See _metatimer.

    [DllImport("ntdll")]
    private static extern int
    NtQueryTimerResolution(out uint MinimumResolution,
                           out uint MaximumResolution,
                           out uint CurrentResolution);

    private static string FormatTimerResolution(uint ticks)
        => $"{ticks / HundredNanosecondsPerMillisecond}{Ch.Nbsp}ms";

    private static LC.TextBox CreateNumberBox(int? initialValue)
        => new(initialValue.ToString()) { Width = NumberBoxWidth };

    private static void Disable(params LC.Control[] controls)
    {
        foreach (var control in controls) control.Enabled = false;
    }

    private LC.Table CreateSizeTable()
    {
        var table = MakeEmptyTable();
        table.Rows.Add(new LC.Label("Width"), _widthBox);
        table.Rows.Add(new LC.Label("Height"), _heightBox);
        return table;
    }

    private LC.StackPanel CreateDelayPanel()
    {
        var table = MakeEmptyTable();

        table.Rows.Add(new LC.Label("Delay (ms)"), _delayBox);

        var description = new LC.Label(
            "This is the requested minimum delay between frames.");

        UpdateTimingNote();

        return new(horizontal: false, table, description, _timingNote);
    }

    private LC.StackPanel CreateFeaturesPanel()
        => new(horizontal: false, _magnifier, _charting);

    private LC.StackPanel CreateExperimentalFeaturesPanel()
        => new(horizontal: false, _stopButton);

    private LC.Table MakeEmptyTable()
        => new LC.Table(noBorders: true,
                        cellPaddingStyle: ".3em .3em",
                        cellVerticalAlign: "middle");

    private void SubscribePrivateHandlers()
    {
        Util.Cleanup += delegate { _metatimer.Dispose(); };
        _metatimer.Tick += delegate { UpdateTimingNote(); };
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

        _postLaunch.Text = "(Launched. You can re-run the LINQPad query to"
                            + " re-enable the launcher.)";

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

    private void UpdateTimingNote()
        => _timingNote.Text =
            $"Note that the system timer{Ch.Rsquo}s resolution affects"
            + $" accuracy. {Environment.NewLine}({GetTimingNoteDetail()})";

    private string GetTimingNoteDetail()
    {
        var success =
            NtQueryTimerResolution(out var worst, out _, out var actual) >= 0;

        if (success) {
            _oldWorstTimerResolution = worst;
        } else if (_oldWorstTimerResolution is uint oldWorst) {
            worst = oldWorst;
        } else {
            return "I couldn't determine your system timer resolution.";
        }

        var worstStr = FormatTimerResolution(worst);

        if (success) {
            var actualStr = FormatTimerResolution(actual);

            return $"Your system timer resolution is {worstStr} at worst,"
                    + $" {actualStr} now.";
        }

        return $"Your system timer resolution is {worstStr} at worst.";
    }

    private void DisableInteractiveControls()
        => Disable(_widthBox,
                   _heightBox,
                   _delayBox,
                   _showPluginFormInTaskbar,
                   _useOldWebBrowser,
                   _magnifier,
                   _charting,
                   _stopButton,
                   _launch);

    // Timer for polling the system timer's timings. Not the system timer.
    private readonly Timer _metatimer = new() { Interval = MetaTimerInterval };

    private readonly LC.TextBox _widthBox;

    private readonly LC.TextBox _heightBox;

    private readonly LC.TextBox _delayBox;

    private readonly LC.Label _timingNote = new();

    private readonly LC.CheckBox _showPluginFormInTaskbar =
        new("Show PluginForm in Taskbar");

    private readonly LC.CheckBox _useOldWebBrowser =
        new("Use old WebBrowser control even if WebView2 is available");

    private readonly LC.CheckBox _magnifier =
        new("Magnifier", isChecked: true);

    private readonly LC.CheckBox _charting = new("Charting", isChecked: true);

    private readonly LC.CheckBox _stopButton = new("Stop button");

    private readonly LC.Button _launch = new("Launch!");

    private readonly LC.Label _postLaunch = new();

    private readonly LC.StackPanel _panel;

    private int? _width;

    private int? _height;

    private int? _delay = MainPanel.DefaultDelayInMilliseconds;

    private uint? _oldWorstTimerResolution = null;
}

/// <summary>
/// The main user interface, containing an interactive canvas, an info bar, and
/// expandable/collapsable tips.
/// </summary>
internal sealed class MainPanel : TableLayoutPanel {
    internal static int DefaultDelayInMilliseconds { get; } = 10;

    internal MainPanel(Size canvasSize, HelpViewerSupplier supplier)
    {
        _nonessentialTimer = new(_components) { Interval = NonessentialDelay };
        _toolTip = new(_components) { ShowAlways = true };
        _helpViewerSupplier = supplier;

        _rect = new(Point.Empty, canvasSize);
        _bmp = new(width: _rect.Width, height: _rect.Height);
        _graphics = Graphics.FromImage(_bmp);
        _graphics.FillRectangle(Brushes.White, _rect);

        _canvas = new() {
            Image = _bmp,
            SizeMode = PictureBoxSizeMode.AutoSize,
            Margin = CanvasMargin,
        };

        _alert = CreateAlertBar();
        _toggles = CreateToggles();
        _magnify = CreateMagnify();
        _stop = CreateStop();
        _charting = CreateCharting();
        _infoBar = CreateInfoBar();

        _neighborEnumerationStrategies = CreateNeighborEnumerationStrategies();

        InitializeMainPanel();
        PerformInitialUpdates();
        SubscribePrivateHandlers();
    }

    internal int DelayInMilliseconds { get; init; } =
        DefaultDelayInMilliseconds;

    internal bool ShowParentInTaskbar { get; init; } = false;

    internal bool MagnifierButtonVisible
    {
        get => _magnify.Visible;
        set => _magnify.Visible = value;
    }

    internal bool StopButtonVisible
    {
        get => _stop.Visible;
        set => _stop.Visible = value;
    }

    internal bool ChartingButtonVisible
    {
        get => _charting.Visible;
        set => _charting.Visible = value;
    }

    internal event EventHandler? Activated;

    internal event EventHandler? Deactivate;

    internal void Display() => this.Dump("Flood Fill Visualization");

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        var pluginForm = (Form)Parent;

        if (ShowParentInTaskbar) pluginForm.ShowInTaskbar = true;

        // Update "Speed" in status from modifier keys, crisply when
        // reasonable. Unlike with an ordinary form, users can't readily see if
        // a PluginForm is active (and it starts inactive) so update it, albeit
        // slower, even when not. [These are two of the three cases. The other
        // is when _tips is focused. See MainPanel.SubscribePrivateHandlers.]
        pluginForm.KeyPreview = true;
        pluginForm.KeyDown += delegate { UpdateStatus(); };
        pluginForm.KeyUp += delegate { UpdateStatus(); };
        pluginForm.Activated += Parent_Activated;
        pluginForm.Deactivate += Parent_Deactivate;
        _nonessentialTimer.Start();
    }

    protected override async void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        const string message =
            "Low vertical space. Rearranging panels (Ctrl+F8) may help.";

        if (_shownBefore || !Visible) return;

        _shownBefore = true;

        // When the launcher is used, even without changing anything, VScroll
        // is always true here, which caused "Low vertical space" to always
        // appear. Processing enqueued window messages first works around it. I
        // don't know why. I suspect this is too early and should be a handler
        // for another event (maybe on the PluginForm). I haven't encountered
        // cases without the launcher but I suspect there may be others, and
        // since the user can't interact much with the panel in that time, this
        // seems harmless.
        //
        // TODO: Figure out what's going on and if there's a better fix.
        await Task.Yield();

        if (VScroll) {
            _alert.Show(message);
            VerticalScroll.Value = 0; // Also needed when the launcher is used.
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) {
            StopAllFills();

            _components.Dispose();

            _bmp.Dispose();
            _graphics.Dispose();
            _pen.Dispose();
        }

        base.Dispose(disposing);
    }

    private const int UnknownCount = -1;

    private const int NonessentialDelay = 90; // See _nonessentialTimer.

    private const int StatusFontSize = 10;

    private const int Pad = 2;

    private static Padding CanvasMargin { get; } =
        new(left: Pad, top: Pad, right: 0, bottom: 0);

    private static bool ShiftIsPressed => ModifierKeys.HasFlag(Keys.Shift);

    private static bool CtrlIsPressed => ModifierKeys.HasFlag(Keys.Control);

    private static bool AltIsPressed => ModifierKeys.HasFlag(Keys.Alt);

    private static bool SuperIsPressed
        => Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

    private static int DecideSpeed()
        => (ModifierKeys & (Keys.Shift | Keys.Control)) switch {
            Keys.Shift                =>  1,
            Keys.Control              => 20,
            Keys.Shift | Keys.Control => 10,
            _                         =>  5
        };

    private int SmallButtonSize => _showHideTips.Height;

    private static void Warn(string message)
        => message.Dump($"Warning ({nameof(MainPanel)})");

    private static bool HaveMagnifierSmoothing
    {
        get {
            try {
                var result = Registry.GetValue(
                    @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\ScreenMagnifier",
                    "UseBitmapSmoothing",
                    null);

                return result is int value && value != 0;
            } catch (SystemException ex) when (ex is SecurityException
                                                  or IOException) {
                Warn("Couldn't check for magnifier smoothing.");
                return false;
            }
        }
    }

    private static void OpenMagnifierSettings()
        => Shell.Execute("ms-settings:easeofaccess-magnifier");

    private AlertBar CreateAlertBar() => new() {
        Width = _rect.Width,
        Margin = CanvasMargin,
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

    private ApplicationButton CreateMagnify()
        => new(executablePath: Files.GetSystem32ExePath("magnify"),
               SmallButtonSize,
               fallbackDescription: "Magnifier");

    private Button CreateStop()
        => new BitmapButton(enabledBitmapFilename: "stop.bmp",
                            disabledBitmapFilename: "stop-faded.bmp",
                            SmallButtonSize) { Visible = false };

    private AnimatedBitmapCheckBox CreateCharting()
        => new(from i in Enumerable.Range(1, 6)
               select new CheckBoxBitmapFilenamePair($"chart{i}.bmp",
                                                     $"chart{i}-gray.bmp"),
               SmallButtonSize,
               FrameSequence.Oscillating);

    private TableLayoutPanel CreateInfoBar()
    {
        var infoBar = new TableLayoutPanel {
            RowCount = 1,
            ColumnCount = 5,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Width = _rect.Width,
        };

        infoBar.Controls.Add(_status, column: 2, row: 0);
        infoBar.Controls.Add(_toggles, column: 3, row: 0);
        infoBar.Height = _toggles.Height; // Must be after adding _toggles.
        infoBar.Controls.Add(_magnify, column: 0, row: 0);
        infoBar.Controls.Add(_stop, column: 1, row: 0);
        infoBar.Controls.Add(_charting, column: 2, row: 0);

        return infoBar;
    }

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

        Controls.Add(_alert);
        Controls.Add(_canvas);
        Controls.Add(_infoBar);
        Controls.Add(_tips);
    }

    private void PerformInitialUpdates()
    {
        UpdateStopButton();
        UpdateCharting();
        UpdateStatus();
        UpdateShowHideTips();
        UpdateOpenCloseHelp();
    }

    private void UpdateMagnifyToolTip()
        => _magnify.SetToolTip(ShiftIsPressed ? "Magnifier Settings" : null);

    private void UpdateStopButton()
    {
        if (_jobs == 0) {
            _stop.Enabled = false;
            _toolTip.SetToolTip(_stop, "No running fills to stop");
        } else {
            _stop.Enabled = true;
            _toolTip.SetToolTip(_stop, "Stop running fills");
        }
    }

    private void UpdateCharting()
    {
        _charting.Animated = _jobsCharting != 0;

        var (lede, comment) =
            _charting.Checked
                ? ("Click to NOT chart newly started fills.",
                   "As of now, newly started fills will chart.")
                : ("Click to chart newly started fills.",
                   "As of now, newly started fills will not chart.");

        var detail = (_jobsCharting, _charting.Checked) switch {
            (0, false) =>
                "Also, no currenty running fills are charting.",
            (0, true) =>
                "But no currently running fills are charting.",
            (1, false) =>
                $"But {_jobsCharting} currently running fill is charting.",
            (1, true) =>
                $"Also, {_jobsCharting} currently running fill is charting.",
            (_, false) =>
                $"But {_jobsCharting} currently running fills are charting.",
            (_, true) =>
                $"Also, {_jobsCharting} currently running fills are charting.",
        };

        var report = string.Join(Environment.NewLine, lede, comment, detail);
        _toolTip.SetToolTip(_charting, report);
    }

    private void UpdateStatus()
    {
        var strategy = _neighborEnumerationStrategies.Current.ToString();
        var speed = DecideSpeed();

        if (strategy.Equals(_oldStrategy, StringComparison.Ordinal)
                && speed == _oldSpeed && _jobs == _oldJobs) return;

        UpdateStatusText(strategy, speed, _jobs);
        UpdateStatusToolTip(strategy, speed, _jobs);

        // TODO: This doesn't conceptually belong here, but it needs to trigger
        // under the same conditions that update the speed. Refactor so the
        // code makes more sense, or avoid carrying over this confusion when
        // splitting up and rewriting UpdateStatus--as will have to be done
        // the status bar is converted from a label to a toolstrip or other
        // collection of multiple controls.
        UpdateMagnifyToolTip();

        (_oldStrategy, _oldSpeed, _oldJobs) = (strategy, speed, _jobs);
    }

    private void UpdateStatusText(string strategy, int speed, int jobs)
    {
        const string spacer = "      ";

        var speedSummary = $"{speed}{Ch.Times}";

        var jobsSummary = (jobs == 1 ? $"{jobs} job" : $"{jobs} jobs");

        _status.Text =
            string.Join(spacer, strategy, speedSummary, jobsSummary);
    }

    private void UpdateStatusToolTip(string strategy, int speed, int jobs)
    {
        var strategyDetail = $"New fills{Ch.Rsquo} neighbor enumeration"
                            + $" strategy is {Ch.Ldquo}{strategy}.{Ch.Rdquo}";

        var speedQuantity = (speed == 1 ? $"{speed} pixel per frame"
                                        : $"{speed} pixels per frame");

        var speedDetail =
            $"New fills{Ch.Rsquo} drawing speed is {speedQuantity}.";

        var jobsDetail = jobs switch {
            0 => "No fills are currently running.",
            1 => $"{jobs} fill is currently running.",
            _ => $"{jobs} fills are currently running.",
        };

        var details = string.Join(Environment.NewLine,
                                  strategyDetail, speedDetail, jobsDetail);

        _toolTip.SetToolTip(_status, details);
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

    private void SubscribePrivateHandlers()
    {
        Util.Cleanup += delegate { Dispose(); };
        _nonessentialTimer.Tick += delegate { UpdateStatus(); };

        _canvas.MouseMove += canvas_MouseMove;
        _canvas.MouseClick += canvas_MouseClick;
        _canvas.MouseWheel += canvas_MouseWheel;

        _magnify.StartingApplication += magnify_StartingApplication;
        _stop.Click += stop_Click;
        _charting.CheckedChanged += delegate { UpdateCharting(); };
        _showHideTips.Click += showHideTips_Click;
        _openCloseHelp.Click += openCloseHelp_Click;
        _tips.DocumentCompleted += tips_DocumentCompleted;

        // Update the status bar from modifier keys pressed or released while
        // _tips has focus. This must be covered separately because keypresses
        // sent to a WebBrowser control are not previewed by the containing
        // form, notwithstanding KeyPreview. [This is one of three cases; see
        // MainPanel.OnHandleCreated for the other two.]
        _tips.PreviewKeyDown += delegate { UpdateStatus(); };
        _tips.PreviewKeyUp += delegate { UpdateStatus(); };
    }

    private void Parent_Activated(object? sender, EventArgs e)
    {
        // If the parent is detached, don't respond to its activation.
        if (Parent is null) return;

        _nonessentialTimer.Stop();
        Activated?.Invoke(sender, e);
    }

    private void Parent_Deactivate(object? sender, EventArgs e)
    {
        _nonessentialTimer.Start();
        Deactivate?.Invoke(sender, e);
    }

    private void canvas_MouseMove(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) {
            _graphics.DrawLine(_pen, _oldLocation, e.Location);

            var x1 = Math.Min(_oldLocation.X, e.Location.X);
            var y1 = Math.Min(_oldLocation.Y, e.Location.Y);
            var x2 = Math.Max(_oldLocation.X, e.Location.X);
            var y2 = Math.Max(_oldLocation.Y, e.Location.Y);

            var corner = new Point(x: x1, y: y1);
            var size = new Size(width: x2 - x1 + 1, height: y2 - y1 + 1);

            _canvas.Invalidate(new Rectangle(corner, size));
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
            _canvas.Invalidate(e.Location);
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

        if (!ShiftIsPressed) {
            // Scrolling without Shift cycles neighbor enumeration strategies.
            if (scrollingDown)
                _neighborEnumerationStrategies.CycleNext();
            else
                _neighborEnumerationStrategies.CyclePrev();
        } else if (_neighborEnumerationStrategies.Current
                    is not ConfigurableNeighborEnumerationStrategy strategy) {
            // Scrolling with Shift cycles substrategies, but there are none.
            _alert.Show(
                $"{Ch.Ldquo}{_neighborEnumerationStrategies.Current}{Ch.Rdquo}"
                + " strategy has no sub-strategies to scroll.");
        } else if (CtrlIsPressed) {
            // Scrolling with Ctrl+Shift cycles substrategies many at a time.
            if (scrollingDown)
                strategy.CycleFastAheadSubStrategy();
            else
                strategy.CycleFastBehindSubStrategy();
        } else {
            // Scrolling with just Shift cycles substrategies one by one.
            if (scrollingDown)
                strategy.CycleNextSubStrategy();
            else
                strategy.CyclePrevSubStrategy();
        }

        UpdateStatus();
    }

    private void magnify_StartingApplication(ApplicationButton sender,
                                             StartingApplicationEventArgs e)
    {
        if (ShiftIsPressed) {
            e.Cancel = true;
            OpenMagnifierSettings();
        } else if (_checkMagnifierSettings && HaveMagnifierSmoothing) {
            _alert.Show(
                $"{Ch.Gear} You may want to turn off {Ch.Ldquo}"
                + $"Smooth edges of images and text{Ch.Rdquo}.",
                onClick: OpenMagnifierSettings,
                onDismiss: () => _checkMagnifierSettings = false);
        }
    }

    private void stop_Click(object? sender, EventArgs e)
    {
        _stop.Enabled = false;
        _toolTip.SetToolTip(_stop, "Stopping fills...");
        StopAllFills();
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
        // Open file:// URLs normally, inside the help browser.
        if  (e.Uri.SchemeIs(Uri.UriSchemeFile)) return;

        // No other URLs shall be opened in the help browser panel.
        e.Cancel = true;

        // Open web links externally, in the default browser. [This check is
        // also important for security, to make sure we are actually opening
        // them in a web browser, i.e., a program (or COM object) registered as
        // an appropriate protocol handler. We can't guarantee the user won't
        // manage to navigate somewhere that offers up a hyperlink starting
        // with something ShellExecuteExW would take as a Windows executable.]
        if (e.Uri.SchemeIsAny(Uri.UriSchemeHttps, Uri.UriSchemeHttp))
            Shell.Execute(e.Uri.AbsoluteUri);
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

    private void StopAllFills()
    {
        if (_jobs != 0) ++_generation; // Make running fills cancel themselves.
    }

    private void AddChartingJob()
    {
        if (_jobsCharting >= _jobs) {
            throw new InvalidOperationException(
                    "Bug: more charting jobs than total jobs?!");
        }

        ++_jobsCharting;
        UpdateCharting();
    }

    private void RemoveChartingJob()
    {
        if (_jobsCharting <= 0) {
            throw new InvalidOperationException(
                    "Bug: negatvely many jobs are charting?!");
        }

        --_jobsCharting;
        UpdateCharting();
    }

    private sealed record Job(int FromArgb,
                              int Speed,
                              Func<Point, Point[]> Supplier,
                              Func<Task> Delayer,
                              int Generation,
                              Action? PostAction);

    private Func<Task> GetSimpleDelayer()
        => () => Task.Delay(DelayInMilliseconds);

    private Func<Task> GetChartingDelayer(Charter charter)
        => async () => {
            await Task.Delay(DelayInMilliseconds);
            charter.Update();
        };

    private Action GetChartingPostAction(Charter charter)
        => () => {
            RemoveChartingJob();
            charter.Finish();
        };

    private Job? BeginFill(Point start, Color toColor, string label)
    {
        var speed = DecideSpeed(); // Don't miss hastily released key combos.

        var fromArgb = _bmp.GetPixel(start.X, start.Y).ToArgb();
        if (fromArgb == toColor.ToArgb()) return null;

        var supplier = _neighborEnumerationStrategies.Current.GetSupplier();

        ++_jobs;
        ++_jobsEver;

        UpdateStopButton();
        UpdateStatus();

        if (!_charting.Checked) {
            return new(fromArgb,
                       speed,
                       supplier,
                       GetSimpleDelayer(),
                       _generation,
                       null);
        }

        AddChartingJob();

        var charter = Charter.StartNew($"Job {_jobsEver} ({label} fill)");

        return new(fromArgb,
                   speed,
                   supplier,
                   GetChartingDelayer(charter),
                   _generation,
                   GetChartingPostAction(charter));
    }

    private void EndFill(Job job)
    {
        if (IsDisposed) return;

        job.PostAction?.Invoke();
        if (--_jobs == 0) UpdateStopButton();
        UpdateStatus();
    }

    private async Task FloodFillAsync(IFringe<Point> fringe,
                                      Point start,
                                      Color toColor)
    {
        if (BeginFill(start, toColor, fringe.Label) is not Job job) return;

        var area = 0;

        for (fringe.Insert(start); fringe.Count != 0; ) {
            var src = fringe.Extract();

            if (!_rect.Contains(src)
                    || _bmp.GetPixel(src.X, src.Y).ToArgb() != job.FromArgb)
                continue;

            if (area++ % job.Speed == 0) {
                await job.Delayer();
                if (job.Generation != _generation) break;
            }

            _bmp.SetPixel(src.X, src.Y, toColor);
            _canvas.Invalidate(src);

            foreach (var dest in job.Supplier(src)) fringe.Insert(dest);
        }

        EndFill(job);
    }

    private async Task RecursiveFloodFillAsync(Point start, Color toColor)
    {
        if (BeginFill(start, toColor, "recursive") is not Job job) return;

        var area = 0;

        async ValueTask FillFromAsync(Point src)
        {
            if (!_rect.Contains(src)
                    || _bmp.GetPixel(src.X, src.Y).ToArgb() != job.FromArgb)
                return;

            if (area++ % job.Speed == 0) {
                await job.Delayer();
                if (job.Generation != _generation) return;
            }

            _bmp.SetPixel(src.X, src.Y, toColor);
            _canvas.Invalidate(src);

            foreach (var dest in job.Supplier(src)) {
                await FillFromAsync(dest);
                if (job.Generation != _generation) return;
            }
        }

        await FillFromAsync(start);
        EndFill(job);
    }

    private void InstantFill(Point start, Color toColor)
    {
        var bounds = new RectangleBuilder(start);
        var toArgb = toColor.ToArgb();

        using (var lb = new LockedBits(_bmp, _rect)) {
            var fromArgb = lb[start.X, start.Y];
            if (fromArgb == toArgb) return;

            var fringe = new Stack<Point>();

            for (fringe.Push(start); fringe.Count != 0; ) {
                var src = fringe.Pop();

                if (!lb.Has(src.X, src.Y) || lb[src.X, src.Y] != fromArgb)
                    continue;

                lb[src.X, src.Y] = toArgb;
                bounds.Add(src);

                foreach (var direction in FastEnumInfo<Direction>.Values)
                    fringe.Push(src.Go(direction));
            }
        }

        _canvas.Invalidate(bounds);
    }

    private readonly IContainer _components = new Container();

    private readonly Timer _nonessentialTimer;

    private readonly ToolTip _toolTip;

    private readonly HelpViewerSupplier _helpViewerSupplier;

    private readonly Rectangle _rect;

    private readonly Bitmap _bmp;

    private readonly Graphics _graphics;

    private readonly PictureBox _canvas;

    private readonly Pen _pen = new(Color.Black);

    private Point _oldLocation = Point.Empty;

    private readonly AlertBar _alert = new();

    private readonly TableLayoutPanel _infoBar;

    private readonly Label _status = new() {
        AutoSize = true,
        Font = new(Label.DefaultFont.FontFamily, StatusFontSize),
    };

    private readonly TableLayoutPanel _toggles;

    private readonly ApplicationButton _magnify;

    private readonly Button _stop;

    private readonly AnimatedBitmapCheckBox _charting;

    private readonly Button _showHideTips = new() {
        Text = "??? Tips", // Placeholder text for height computation.
        AutoSize = true,
        Margin = new(left: 0, top: 0, right: Pad, bottom: 0),
    };

    private readonly Button _openCloseHelp = new() {
        Text = "??? Help", // Placeholder text for height computation.
        AutoSize = true,
        Margin = new(left: Pad, top: 0, right: 0, bottom: 0),
    };

    private readonly MyWebBrowser _tips = new() {
        Visible = false,
        AutoSize = true,
        ScrollBarsEnabled = false,
        Url = Files.GetDocUrl("tips.html"),
    };

    private OutputPanel? _helpPanel = null;

    private readonly Func<int, int> _generator =
        Permutations.CreateRandomGenerator();

    private readonly Carousel<NeighborEnumerationStrategy>
    _neighborEnumerationStrategies;

    private bool _checkMagnifierSettings = true;

    // Store and compare to the old strategy's string representation, because
    // configurable strategies mutate to change sub-strategy, so comparing a
    // strategy across a sub-strategy change would be a self-comparison.
    private string _oldStrategy = string.Empty;

    private int _oldSpeed = UnknownCount;

    private int _oldJobs = UnknownCount;

    private int _jobs = 0;

    private int _jobsEver = 0;

    private int _jobsCharting = 0;

    private int _generation = 0;

    private bool _shownBefore = false;
};

/// <summary>A horizontal bar to show dismissable text-based alerts.</summary>
internal sealed class AlertBar : TableLayoutPanel {
    internal AlertBar()
    {
        InitializeAlertBar();
        SubscribePrivateHandlers();
    }

    internal void Show(string message,
                       Action? onClick = null,
                       Action? onDismiss = null)
    {
        (_content.Text, _onClick, _onDismiss) = (message, onClick, onDismiss);
        UpdateStyle();
        Show();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        _content.Width =
            Width - (_dismiss.Width + Padding.Left + Padding.Right);
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        _content.BackColor = BackColor;
    }

    private static Style StaticStyle { get; } =
        new(Font: new("Segoe UI Semibold", 10, FontStyle.Regular),
            Color: Color.Black,
            Cursor: Cursors.Arrow);

    private static Style LinkStyle { get; } = StaticStyle with {
        Color = Color.FromArgb(0, 102, 204),
        Cursor = Cursors.Hand,
    };

    private static Style LinkHoverStyle { get; } = LinkStyle with {
        Color = Color.FromArgb(0, 80, 197),
    };

    [DllImport("user32")]
    private static extern bool HideCaret(IntPtr hWnd);

    private static void Warn(string message)
        => message.Dump($"Warning ({nameof(AlertBar)})");

    private void InitializeAlertBar()
    {
        // FIXME: Override DefaultPadding instead?
        const int padLeft = 3;
        const int padRight = 0;

        RowCount = 1;
        ColumnCount = 2;
        GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        Padding = new(left: padLeft, top: 0, right: padRight, bottom: 0);
        BackColor = Color.NavajoWhite;
        Visible = false;

        Controls.Add(_content);
        Controls.Add(_dismiss);
        Height = _dismiss.Height; // Must be after adding _dismiss.
    }

    private void UpdateStyle()
    {
        var style = _onClick switch {
            null                              => StaticStyle,
            _ when _content.HasMousePointer() => LinkHoverStyle,
            _                                 => LinkStyle,
        };

        style.ApplyTo(_content);
    }

    private void SubscribePrivateHandlers()
    {
        _content.Click += delegate { _onClick?.Invoke(); };
        _content.GotFocus += content_GotFocus;
        _content.MouseEnter += delegate { UpdateStyle(); };
        _content.MouseLeave += delegate { UpdateStyle(); };

        _dismiss.Click += dismiss_Click;
    }

    private void content_GotFocus(object? sender, EventArgs e)
    {
        if (!HideCaret(_content.Handle)) Warn("Couldn't hide alert caret.");
    }

    private void dismiss_Click(object? sender, EventArgs e)
    {
        Hide();
        _onDismiss?.Invoke();
    }

    private readonly TextBox _content = new() {
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = Padding.Empty,
        BorderStyle = BorderStyle.None,
        ReadOnly = true,
        TabStop = false,
    };

    private readonly Button _dismiss = new() {
        Text = "Dismiss",
        BackColor = Button.DefaultBackColor,
        AutoSize = true,
        Anchor = AnchorStyles.Right,
        Margin = Padding.Empty,
    };

    Action? _onClick = null;

    Action? _onDismiss = null;
}

/// <summary>
/// A bundle of styling information for text, including mouse effects.
/// </summary>
internal sealed record Style(Font Font, Color Color, Cursor Cursor) {
    internal void ApplyTo(Control control)
    {
        control.Font = Font;
        control.ForeColor = Color;
        control.Cursor = Cursor;
    }
}

/// <summary>
/// Provides the ability to cancel the
/// <see cref="ApplicationButton.StartingApplication"/> event.
/// </summary>
internal sealed class StartingApplicationEventArgs : EventArgs {
    internal bool Cancel { get; set; } = false;
}

/// <summary>
/// Represents a method that will handle the
/// <see cref="ApplicationButton.StartingApplication"/> event.
/// </summary>
internal delegate void
StartingApplicationEventHandler(ApplicationButton sender,
                                StartingApplicationEventArgs e);

/// <summary>
/// A square button to launch an application, showing an icon and (optionally)
/// toolip obtained from the executable's metadata.
/// </summary>
internal sealed class ApplicationButton : Button {
    internal ApplicationButton(string executablePath,
                               int sideLength,
                               string? fallbackDescription = null)
    {
        Width = Height = sideLength;
        Margin = Padding.Empty;
        BackgroundImageLayout = ImageLayout.Stretch;

        _toolTip = new(_components) { ShowAlways = true };

        _path = executablePath;

        _description = FileVersionInfo.GetVersionInfo(_path).FileDescription
                        ?? fallbackDescription
                        ?? string.Empty;

        _bitmap = CreateBitmap(_path);

        try {
            BackgroundImage = _bitmap;
            SetToolTip();
        } catch {
            DisposeState();
            throw;
        }
    }

    internal event StartingApplicationEventHandler? StartingApplication;

    internal void SetToolTip(string? caption = null)
        => _toolTip.SetToolTip(this, caption ?? _description);

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        var eStartingApplication = new StartingApplicationEventArgs();
        StartingApplication?.Invoke(this, eStartingApplication);
        if (!eStartingApplication.Cancel) Shell.Execute(_path);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) DisposeState();
        base.Dispose(disposing);
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

    // TODO: Use a SafeHandle-based approach instead.
    [DllImport("shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr ExtractIcon(IntPtr hInst,
                                             string pszExeFileName,
                                             uint nIconIndex);

    [DllImport("user32", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private void DisposeState()
    {
        _components.Dispose();
        _bitmap.Dispose();
    }

    private readonly IContainer _components = new Container();

    private readonly ToolTip _toolTip;

    private readonly string _description;

    private readonly string _path;

    private readonly Bitmap _bitmap;
}

/// <summary>
/// A square button showing a bitmap that changes when enabled/disabled.
/// </summary>
internal sealed class BitmapButton : Button {
    internal BitmapButton(string enabledBitmapFilename,
                          string disabledBitmapFilename,
                          int sideLength)
    {
        Width = Height = sideLength;
        Margin = Padding.Empty;
        BackgroundImageLayout = ImageLayout.Stretch;

        (_enabledImage, _disabledImage) =
            Files.OpenBitmapPair(enabledBitmapFilename,
                                 disabledBitmapFilename);

        try {
            UpdateImage();
        } catch {
            DisposeImages();
            throw;
        }
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        UpdateImage();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) DisposeImages();
        base.Dispose(disposing);
    }

    private void DisposeImages()
    {
        _enabledImage.Dispose();
        _disabledImage.Dispose();
    }

    private void UpdateImage()
        => BackgroundImage = (Enabled ? _enabledImage : _disabledImage);

    private readonly Image _enabledImage;

    private readonly Image _disabledImage;
}

/// <summary>
/// A square button-style checkbox showing an animation of several bitmaps,
/// which can be paused and resumed, and of which each frame has checked and
/// unchecked versions.
/// </summary>
/// <remarks>
/// Animates only when <see cref="AnimatedCheckBox.Animated"/> is set to
/// <c>true</c>. It is <c>false</c> by default. (Animation represents that an
/// ongoing task, separate from whether the box is checked, is being done.)
/// </remarks>
internal sealed class AnimatedBitmapCheckBox : CheckBox {
    internal AnimatedBitmapCheckBox(
            IEnumerable<CheckBoxBitmapFilenamePair> filenamePairs,
            int sideLength,
            Func<int, IEnumerable<int>> frameSequenceSupplier)
    {
        var pairs = filenamePairs.ToList();

        Width = Height = sideLength;
        Margin = Padding.Empty;
        Appearance = Appearance.Button;
        BackgroundImageLayout = ImageLayout.Stretch;

        _timer = new(_components) { Interval = DefaultInterval };
        _timer.Tick += timer_Tick;
        _frame = frameSequenceSupplier(pairs.Count).GetStartedEnumerator();
        _imagePairs = OpenAllBitmapPairs(pairs);

        try {
            UpdateImage();
        } catch {
            DisposeState();
            throw;
        }
    }

    internal bool Animated
    {
        get => _timer.Enabled;
        set => _timer.Enabled = value;
    }

    internal int Interval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    protected override void OnCheckedChanged(EventArgs e)
    {
        base.OnCheckedChanged(e);
        UpdateImage();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) DisposeState();
        base.Dispose(disposing);
    }

    private const int DefaultInterval = 200;

    private static IList<CheckBoxImagePair>
    OpenAllBitmapPairs(IEnumerable<CheckBoxBitmapFilenamePair> filenamePairs)
    {
        var imagePairs = new List<CheckBoxImagePair>();

        try {
            foreach (var filenames in filenamePairs)
                imagePairs.Add(new(filenames));
        } catch {
            DisposeImagePairs(imagePairs);
            throw;
        }

        return imagePairs;
    }

    private static void DisposeImagePairs(IList<CheckBoxImagePair> imagePairs)
    {
        foreach (var pair in imagePairs) pair.Dispose();
        imagePairs.Clear();
    }

    private void timer_Tick(object? sender, EventArgs e)
    {
        _frame.MoveNextOrThrow();
        UpdateImage();
    }

    private void UpdateImage()
    {
        var pair = _imagePairs[_frame.Current];
        BackgroundImage = (Checked ? pair.CheckedImage : pair.UncheckedImage);
    }

    private void DisposeState()
    {
        _components.Dispose();
        DisposeImagePairs(_imagePairs);
        _frame.Dispose();
    }

    private readonly IContainer _components = new Container();

    private readonly Timer _timer;

    private readonly IList<CheckBoxImagePair> _imagePairs;

    private readonly IEnumerator<int> _frame;
}

/// <summary>
/// Filenames for background bitmaps for the checked and unchecked states of
/// a button-style checkbox.
/// </summary>
/// <remarks>See <see cref="CheckBoxBitmapPair"/>.</remarks>
internal sealed record
CheckBoxBitmapFilenamePair(string CheckedFilename, string UncheckedFilename);

/// <summary>
/// Background images for the checked and uchecked states of a button-style
/// checkbox.
/// </summary>
/// <remarks>See <see cref="CheckBoxBitmapFilenamePair"/>.</remarks>
internal sealed class CheckBoxImagePair : IDisposable {
    internal CheckBoxImagePair(CheckBoxBitmapFilenamePair filenames)
        => (CheckedImage, UncheckedImage) =
            Files.OpenBitmapPair(filenames.CheckedFilename,
                                 filenames.UncheckedFilename);

    public void Dispose()
    {
        CheckedImage.Dispose();
        UncheckedImage.Dispose();
    }

    internal Image CheckedImage { get; }

    internal Image UncheckedImage { get; }
}

/// <summary>
/// Generators of infinite sequences of indices representing frames in an
/// endless animation.
/// </summary>
internal static class FrameSequence {
    internal static IEnumerable<int> Oscillating(int frameCount)
    {
        if (frameCount <= 0) {
            throw new ArgumentException(
                    paramName: nameof(frameCount),
                    message: "Animation Must have at least one frame.");
        }

        return OscillatingImpl(0, frameCount - 1);
    }

    private static IEnumerable<int> OscillatingImpl(int min, int max)
    {
        var next = min;
        var delta = +1;

        for (; ; ) {
            var current = next;
            next = current + delta;

            if (!(min <= next && next <= max)) {
                delta *= -1;
                next = current + delta;
            }

            yield return current;
        }
    }
}

/// <summary>
/// Adds a <c>PreviewKeyUp</c> event to <see cref="Windows.Forms.WebBrowser"/>.
/// </summary>
/// <remarks>
/// <see cref="Windows.Forms.WebBrowser"/> doesn't support <c>KeyDown</c> and
/// <c>KeyUp</c> (see <see cref="Windows.Forms.WebBrowserBase.KeyDown/> and
/// <see cref="Windows.Forms.WebBrowserBase.KeyUp/>). It does support
/// <c>PreviewKeyDown</c>. but no <c>PreviewKeyUp</c>, which this provides.
/// </remarks>
internal sealed class MyWebBrowser : WebBrowser {
    public override bool PreProcessMessage(ref Message msg)
    {
        // Give PreviewKeyUp the same information PreviewKeyDown gets. Compare:
        // https://github.com/dotnet/winforms/blob/v5.0.2/src/System.Windows.Forms/src/System/Windows/Forms/Control.cs#L8977
        if ((WM)msg.Msg is WM.KEYUP or WM.SYSKEYUP)
            PreviewKeyUp?.Invoke(this, new((Keys)msg.WParam | ModifierKeys));

        return base.PreProcessMessage(ref msg);
    }

    internal event PreviewKeyDownEventHandler? PreviewKeyUp = null;

    private enum WM : uint {
        KEYUP    = 0x0101,
        SYSKEYUP = 0x0105,
    }
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

    internal static (Bitmap, Bitmap)
    OpenBitmapPair(string firstFilename, string secondFilename)
    {
        Bitmap? firstBitmap = null;
        Bitmap? secondBitmap = null;

        try {
            firstBitmap = Files.OpenBitmap(firstFilename);
            secondBitmap = Files.OpenBitmap(secondFilename);
            // Further processing that can throw could go here.
        } catch {
            firstBitmap?.Dispose();
            secondBitmap?.Dispose();
            throw;
        }

        return (firstBitmap, secondBitmap);
    }

    private static Bitmap OpenBitmap(string filename)
    {
        var bitmap = new Bitmap(Files.GetImagePath(filename));
        bitmap.MakeTransparent();
        return bitmap;
    }

    private static string GetImagePath(string filename)
        => Path.Combine(QueryDirectory, "images", filename);

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
    /// <summary>Description of this fringe (at least of its type).</summary>
    string Label { get; }

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
    public string Label => "stack";

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
    public string Label => "queue";

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
    public string Label => "random";

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
    public string Label => "deque";

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

/// <summary>LINQ operators not found in Enumerable or MoreLinq.</summary>
internal static class EnumerableExtensions {
    internal static IEnumerable<TimeSpan>
    Deltas(this IEnumerable<TimeSpan> times)
        => times.Pairwise((before, after) => after - before);

    internal static TimeSpan Average(this IEnumerable<TimeSpan> durations)
    {
        var sum = TimeSpan.Zero;
        var count = 0;

        foreach (var duration in durations) {
            sum += duration;
            ++count;
        }

        return sum / count;
    }
}

/// <summary>
/// Provides extension methods for enumerating sequences known to be infinite.
/// </summary>
internal static class EnumeraExtensions {
    internal static IEnumerator<T>
    GetStartedEnumerator<T>(this IEnumerable<T> source)
    {
        var enumerator = source.GetEnumerator();
        try {
            enumerator.MoveNextOrThrow();
            return enumerator;
        } catch {
            enumerator.Dispose();
            throw;
        }
    }

    internal static void MoveNextOrThrow(this IEnumerator enumerator)
    {
        if (!enumerator.MoveNext()) {
            throw new InvalidOperationException(
                    "A sequence that needed to be infinite was finite.");
        }
    }
}

/// <summary>
/// Provides extension methods for region invalidation and mouse polling.
/// </summary>
internal static class ControlExtensions {
    internal static void Invalidate(this Control control, Point point)
        => control.Invalidate(new Rectangle(point, new Size(1, 1)));

    internal static void Invalidate(this Control control,
                                    Point point1,
                                    Point point2)
        => control.Invalidate(RectangleBuilder.BuildFrom(point1, point2));

    internal static void Invalidate(this Control control,
                                    RectangleBuilder bounds)
        => control.Invalidate(bounds.Build());

    internal static bool HasMousePointer(this Control control)
    {
        var clientPoint = control.PointToClient(Control.MousePosition);
        return control.ClientRectangle.Contains(clientPoint);
    }
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
/// Provides an extension method for deconstructing the size (of an image or
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
    public static bool SchemeIs(this Uri uri, string scheme)
        => uri.Scheme.Equals(scheme, StringComparison.Ordinal);

    internal static bool SchemeIsAny(this Uri uri, params string[] schemes)
        => schemes.Any(uri.SchemeIs);
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
    public override string ToString() => $"{Name} {Ch.Ndash} {Detail}";

    /// <summary>
    /// Switches to the next sub-strategy, or from the last to the first.
    /// </summary>
    internal abstract void CycleNextSubStrategy();

    /// <summary>
    /// Switches to the previous sub-strategy, or from the first to the last.
    /// </summary>
    internal abstract void CyclePrevSubStrategy();

    /// <summary>
    /// Switches to a (conceptually) much later substrategy. May wrap.
    /// </summary>
    /// <remarks>

    /// </remarks>
    internal abstract void CycleFastAheadSubStrategy();

    /// <summary>
    /// Switches to a (conceptually) much earlier substrategy. May wrap.
    /// </summary>
    /// <remarks>
    /// Implementation-defined semantics. Not necessarily equivalent to calling
    /// <see cref="CyclePrevSubStrategy"/> a fixed number of times.
    /// </remarks>
    internal abstract void CycleFastBehindSubStrategy();

    /// <summary>
    /// Information about the current sub-strategy, to appear in the UI.
    /// </summary>
    private protected abstract string Detail { get; }
}

/// <summary>
/// Enumerates neighbors in the same order, within and across fills.
/// </summary>
/// <remarks>The substrategy determines the specific order used.</remarks>
internal sealed class UniformStrategy
        : ConfigurableNeighborEnumerationStrategy {
    internal UniformStrategy() : this(FastEnumInfo<Direction>.CopyValues()) { }

    internal UniformStrategy(params Direction[] uniformOrder) : base("Uniform")
        => _uniformOrder = uniformOrder[..];

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

    /// <summary>Reverses the uniform order.</summary>
    /// <remarks>Same as <see cref="CycleFastBehindSubStrategy"/>.</remarks>
    internal override void CycleFastAheadSubStrategy() => Reverse();

    /// <summary>Reverses the uniform order.</summary>
    /// <remarks>Same as <see cref="CycleFastAheadSubStrategy"/>.</remarks>
    internal override void CycleFastBehindSubStrategy() => Reverse();

    /// <inheritdoc/>
    private protected override string Detail
        => new(Array.ConvertAll(_uniformOrder,
                                direction => direction.ToString()[0]));

    private void Reverse() => Array.Reverse(_uniformOrder);

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
        var order = FastEnumInfo<Direction>.CopyValues();
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
            var neighbors = FastEnumInfo<Direction>
                            .ConvertValues(direction => src.Go(direction));
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
                var directions = FastEnumInfo<Direction>.CopyValues();
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
/// <typeparam name="T">The item type offered by the carousel.</typeparam>
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

/// <summary>Adapter for reversing the direction of a comparer.</summary>
/// <typeparam name="T">The element type the comparer accepts.</typeparam>
internal sealed class ReverseComparer<T> : IComparer<T> {
    internal static IComparer<T> Default { get; } =
        new ReverseComparer<T>(Comparer<T>.Default);

    public int Compare(T? lhs, T? rhs) => _comparer.Compare(rhs, lhs);

    internal ReverseComparer(IComparer<T> comparer) => _comparer = comparer;

    private readonly IComparer<T> _comparer;
}

/// <summary>Methods to get information about enums quickly.</summary>
/// <typeparam name="T">The enum to provide information about.</typeparam>
/// <remarks>Works by caching results of slow reflective operations.</remarks>
internal static class FastEnumInfo<T> where T : struct, Enum {
    internal static ReadOnlySpan<T> Values => _values;

    internal static T[] CopyValues() => _values[..];

    internal static TOutput[]
    ConvertValues<TOutput>(Converter<T, TOutput> converter)
        => Array.ConvertAll(_values, converter);

    private static readonly T[] _values = Enum.GetValues<T>();
}

/// <summary>Times each step of a process and provides charting.</summary>
internal sealed class Charter {
    internal static Charter StartNew(string name) => new(name);

    internal void Finish()
    {
        _timer.Stop();

        var chart = MakeChart();
        CustomizeSeries(chart);
        CustomizeArea(chart);
        TryCustomizeToolTip(chart);
        chart.Dump(_name);
    }

    internal void Update() => _times.Add(_timer.Elapsed);

    private const float LabelFontSize = 10;

    private const int ScrollBarSize = 17;

    private const int ToolTipDelay = 5;

    private Charter(string name) => _name = name;

    private static Font ChartTitleFont { get; } =
        new Font("Segoe UI Semibold", LabelFontSize);

    private static Font AxisTitleFont { get; } =
        new Font("Segoe UI", LabelFontSize);

    private Chart MakeChart()
    {
        var deltas = _times.Deltas().ToList();

        var chart = deltas.Select((duration, index) => new {
                                Milliseconds = duration.TotalMilliseconds,
                                Count = index + 1,
                            })
                          .Chart(datum => datum.Count,
                                 datum => datum.Milliseconds,
                                 Util.SeriesType.Column)
                          .ToWindowsChart();

        var topText = string.Join("     ",
            _name,
            $"total {_times[^1].TotalSeconds:F2}{Ch.Nnbsp}s",
            $"mean {deltas.Average().TotalMilliseconds:F1}{Ch.Nnbsp}ms",
            $"min {deltas.Min().TotalMilliseconds:F1}{Ch.Nnbsp}ms",
            $"max {deltas.Max().TotalMilliseconds:F1}{Ch.Nnbsp}ms");

        chart.Titles.Add(MakeChartTitle(topText));

        return chart;
    }

    private static Title MakeChartTitle(string topText)
        => new(topText, Docking.Top, ChartTitleFont, Chart.DefaultForeColor);

    private static void CustomizeSeries(Chart chart)
    {
        var series = chart.Series[0];
        series.ToolTip =
            $"frame #VALX{Environment.NewLine}#VAL{{F1}}{Ch.Nbsp}ms";
        series["PointWidth"] = "1"; // No padding between the bars.
    }

    private static void CustomizeArea(Chart chart)
    {
        var area = chart.ChartAreas[0];
        area.CursorX.IsUserSelectionEnabled = true;
        area.AxisX.ScrollBar.Size = ScrollBarSize;
        area.AxisX.Title = "frame number";
        area.AxisY.Title = "delay (milliseconds)";
        area.AxisX.TitleFont = area.AxisY.TitleFont = AxisTitleFont;
    }

    private static void TryCustomizeToolTip(Chart chart)
    {
        // TODO: Encapsulation exists for a reason. Consider (1) not doing
        // this at all, or maybe (2) using the Windows API to find and modify
        // the tooltip (which might, arguably, be more resilient to changes).
        ToolTip toolTip;
        try {
            toolTip = chart.Uncapsulate().selection._toolTip;
        } catch (SystemException ex) when (ex is MissingMemberException
                                              or InvalidCastException) {
            Warn("Couldn't customize chart tooltip.");
            return;
        }

        toolTip.InitialDelay = toolTip.ReshowDelay = ToolTipDelay;
    }

    private static void Warn(string message)
        => message.Dump($"Warning ({nameof(Charter)})");

    private readonly string _name;

    private readonly Stopwatch _timer = Stopwatch.StartNew();

    private readonly List<TimeSpan> _times = new() { TimeSpan.Zero };
}

/// <summary>Builds minimal rectangles encompassing specified points.</summary>
/// <remarks>The rectangles' height and width are strictly positive.</remarks>
internal sealed class RectangleBuilder {
    internal static Rectangle BuildFrom(Point point1, Point point2)
        => BuildFromCorners(minX: Math.Min(point1.X, point2.X),
                            minY: Math.Min(point1.Y, point2.Y),
                            maxX: Math.Max(point1.X, point2.X),
                            maxY: Math.Max(point1.Y, point2.Y));

    internal RectangleBuilder(Point start)
    {
        _minX = _maxX = start.X;
        _minY = _maxY = start.Y;
    }

    internal void Add(Point point)
    {
        _minX = Math.Min(_minX, point.X);
        _minY = Math.Min(_minY, point.Y);
        _maxX = Math.Max(_maxX, point.X);
        _maxY = Math.Max(_maxY, point.Y);
    }

    internal Rectangle Build()
        => BuildFromCorners(minX: _minX,
                            minY: _minY,
                            maxX: _maxX,
                            maxY: _maxY);

    private static Rectangle
    BuildFromCorners(int minX, int minY, int maxX, int maxY)
    {
        var topLeft = new Point(x: minX, y: minY);
        var size = new Size(width: maxX - minX + 1, height: maxY - minY + 1);
        return new(topLeft, size);
    }

    private int _minX, _minY, _maxX, _maxY;
}

/// <summary>
/// A specialized 2-dimensional span managing lifetime and access to a region
/// in a 32-bit ARGB bitmap image.
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

/// <summary>Named aliases for some Unicode characters.</summary>
internal static class Ch {
    /// <summary>No-break space.</summary>
    internal const char Nbsp = '\u00A0';

    /// <summary>Narrow no-break space.</summary>
    internal const char Nnbsp = '\u202F';

    /// <summary>Right single quotation mark.</summary>
    internal const char Rsquo = '\u2019';

    /// <summary>Left double quotation mark.</summary>
    internal const char Ldquo = '\u201C';

    /// <summary>Right double quotation mark.</summary>
    internal const char Rdquo = '\u201D';

    /// <summary>En dash.</summary>
    internal const char Ndash = '\u2013';

    /// <summary>Multiplication sign.</summary>
    internal const char Times = '\u00D7';

    /// <summary> Gear (emoji).</summary>
    internal const char Gear = '\u2699';
}
