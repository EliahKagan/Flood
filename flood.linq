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

// flood.linq - Entry point.
// This file is part of Flood, an interactive flood-fill visualizer.
//
// Copyright 2021 Eliah Kagan <degeneracypressure@gmail.com>
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION
// OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN
// CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

#nullable enable

const float defaultScreenFractionForCanvas = 5.0f / 9.0f;

var devmode = GotKey.Shift;

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
    var (screenWidth, screenHeight) = GetBestScreen().Bounds.Size;

    var sideLength = (int)(Math.Min(screenWidth, screenHeight)
                            * defaultScreenFractionForCanvas);

    return new(width: sideLength, height: sideLength);
}

static Screen GetBestScreen()
{
    // We want the screen that (most of) the LINQPad window is on.
    var screen = Screen.FromHandle(Util.HostWindowHandle);

    // But fall back to the primary screen if we couldn't find that.
    return screen.WorkingArea.IsEmpty ? Screen.PrimaryScreen : screen;
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

    private const int MetaTimerInterval = 150; // See _metatimer.

    private static string FormatTimerResolution(uint ticks)
        => $"{ticks / NtDll.HundredNanosecondsPerMillisecond}{Ch.Nbsp}ms";

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
        var result =
            NtDll.NtQueryTimerResolution(out var worst, out _, out var actual);

        if (result >= 0) {
            _oldWorstTimerResolution = worst;
        } else if (_oldWorstTimerResolution is uint oldWorst) {
            worst = oldWorst;
        } else {
            return "I couldn't determine your system timer resolution.";
        }

        var worstStr = FormatTimerResolution(worst);

        if (result >= 0) {
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
/// Represents a helper for creating and switching output panels.
/// </summary>
/// <remarks>
/// See <see cref="PanelSwitcher"/>. Although it would make sense to have
/// different implementations for different policies, the main purpose of this
/// interface is to distinguish non-owning <see cref="PanelSwitcher"/>
/// references.
/// </remarks>
internal interface IPanelSwitcher {
    /// <summary>Switches to a LINQPad panel, if it is open.</summary>
    /// <param name="panel">
    /// The <see cref="LINQPad.OutputPanel"/> to switch to, or <c>null</c> to
    /// switch to the "Results" panel.
    /// </param>
    /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
    bool TrySwitch(OutputPanel? panel);

    /// <summary>
    /// Switches to a LINQPad if it is open. Throws an exception otherwise.
    /// </summary>
    /// <param name="panel">
    /// The <see cref="LINQPad.OutputPanel"/> to switch to, or <c>null</c> to
    /// switch to the "Results" panel.
    /// </param>
    /// <remarks>See <see cref="TrySwitch"/>.</remarks>
    void Switch(OutputPanel? panel);

    /// <summary>
    /// Opens an output panel for a control and switches to it.
    /// </summary>
    /// <param name="control">The control to display in the panel.</param>
    /// <param name="panelTitle">The panel's title in the toolstrip.</param>
    /// <returns>The panel that was created.</returns>
    OutputPanel DisplayForeground(Control control, string panelTitle);

    /// <summary>
    /// Opens an output panel for a control. Tries not to switch to it.
    /// </summary?
    /// <param name="control">The control to display in the panel.</param>
    /// <param name="panelTitle">The panel's title in the toolstrip.</param>
    /// <returns>The panel that was created.</returns>
    OutputPanel DisplayBackground(Control control, string panelTitle);
}

/// <summary>Default implementation of <see cref="IPanelSwitcher"/>.</summary>
internal sealed class PanelSwitcher : Component, IPanelSwitcher {
    internal PanelSwitcher(IContainer components) : this()
        => components.Add(this);

    internal PanelSwitcher() => _timer.Tick += timer_Tick;

    // FIXME: Since I'm using this for important UI features--switching to the
    // open help panel when Help is clicked again, and to a chart when its
    // notification is clicked--it's very bad I'm violating encapsulation.
    // OutputPanel.Activate has the "internal" acccess modifier; queries aren't
    // expected to use it and it may be removed (or worse, change) at any time.
    // Unfortunately, there doesn't seem to be another way to do this.
    //
    // PanelManager.GetOutputPanels() returns an array of output panels, and
    // writing to Util.SelectedOutputPanelIndex switches panels. When output
    // panels are created in such a way as to be listed from left to right in
    // the order of creation--such as when they are created sequentally by
    // interacting with LINQPad controls in the Results panel--they are indexed
    // in the same order and it is sufficient to add and subtract 1 [since
    // Util.SelectedOutputPanelIndex is 0 for the Results panel, which is not
    // actually an OutputPanel object and thus doesn't appear in
    // PanelManager.GetOutputPanels()]. Otherwise, the orders needn't agree, I
    // believe because new panels are not necessarily added to the very end of
    // the strip, but are instead usually added just to the right of the panel
    // from which they're displayed.
    //
    // I don't think it's reasonable to attempt to maintain a correspondence
    // between the two orders. Besides writing to Util.SelectOutputPanelIndex,
    // it is also possible to read from it, but the indices are not stable as
    // panels open and close; caching an index to get back to it does not seems
    // to work either, aside from 0 for getting back to the Results panel or
    // checking if we are there. What I need to do is investigate a bit futher;
    // produce simple, reproducible examples; and inquire on the LINQPad forums
    // and/or request a feature.
    /// <inheritdoc/>
    public bool TrySwitch(OutputPanel? panel)
    {
        ThrowIfDisposed();

        if (panel is null) {
            Util.SelectedOutputPanelIndex = 0;
        } else if (PanelManager.GetOutputPanels().Contains(panel)) {
            panel.Uncapsulate().Activate();
        } else {
            return false;
        }

        _sticky = panel;
        return true;
    }

    /// <inheritdoc/>
    public void Switch(OutputPanel? panel)
    {
        if (!TrySwitch(panel)) {
            throw new InvalidOperationException(
                    "Bug: The panel is closed or otherwise unavailable.");
        }
    }

    /// <inheritdoc/>
    public OutputPanel DisplayForeground(Control control, string panelTitle)
    {
        ThrowIfDisposed();

        var panel = PanelManager.DisplayControl(control, panelTitle);
        _sticky = panel;
        return panel;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// LINQPad doesn't support opening a new <see cref="OutputPanel"/> without
    /// activating it. So this opens the panel and then, on a best-effort
    /// basis, tries to switch back to the panel that is would next be active.
    /// </remarks>
    public OutputPanel DisplayBackground(Control control, string panelTitle)
    {
        var foreground = ForegroundPanel;
        var background = PanelManager.DisplayControl(control, panelTitle);
        TrySwitch(foreground);
        return background;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed) {
            _disposed = true;
            _timer.Dispose();
        }

        base.Dispose(disposing);
    }

    private const int ForegroundSnapshotInterval = 180;

    private static OutputPanel? CurrentVisiblePanel
        => PanelManager.GetOutputPanels()
                       .SingleOrDefault(panel => panel.IsVisible);

    private OutputPanel? ForegroundPanel
        => (_sticky is null || (_oldest == _older && _older == _old))
            ? _old
            : _sticky;

    private void ThrowIfDisposed()
    {
        if (_disposed) {
            throw new ObjectDisposedException(
                    objectName: nameof(PanelSwitcher),
                    message: "Can't switch panels with disposed switcher.");
        }
    }

    private void timer_Tick(object? sender, EventArgs e)
    {
        var last = (Util.SelectedOutputPanelIndex == 0
                        ? null
                        : CurrentVisiblePanel ?? _old);

        (_oldest, _older, _old) = (_older, _old, last);
    }

    private readonly Timer _timer = new Timer {
        Interval = ForegroundSnapshotInterval,
        Enabled = true,
    };

    private OutputPanel? _sticky = null;

    private OutputPanel? _oldest = null;
    private OutputPanel? _older = null;
    private OutputPanel? _old = null;

    private bool _disposed = false;
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
        _switcher = new PanelSwitcher(_components);

        _rect = new(Point.Empty, canvasSize);
        _bmp = new(width: _rect.Width, height: _rect.Height);
        _graphics = CreateCanvasGraphics();
        _canvas = CreateCanvas();

        _alert = CreateAlertBar();
        _help = new HelpButton(supplier, _switcher);
        _helpButtons = CreateHelpButtons();
        _magnify = new MagnifyButton(_showHideTips.Height, _alert);
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

    internal void Display()
        => _switcher.DisplayForeground(this, "Flood Fill Visualization");

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

    protected override void WndProc(ref Message m)
    {
        // If the user pressed a Windows key (Super key) as a modifier for a
        // canvas command (successful or not), avoid activating the Start Menu.
        if ((User32.WM)m.Msg is User32.WM.KEYUP
                && (User32.VK)m.WParam is User32.VK.LWIN or User32.VK.RWIN
                && _suppressStartMenu) {
            _suppressStartMenu = false;

            if (Focused) {
                // Simulate concurrent input to prevent the Windows key from
                // opening the Start Menu. Don't use anything this program
                // treats specially. "Win+," is a global shortcut but it's only
                // going into this program's message queue. (If it did somehow
                // seep through, it would peek the desktop, which is innocuous
                // even in combination with other keystrokes.) Global shortcuts
                // may even be best, as users are less likely to customize them
                // with a macro program like AutoHotKey.
                // TODO: Decide if this is really less bad than a manual hook.
                SendKeys.Send(",");
            }
        }

        base.WndProc(ref m);
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

    private static int DecideSpeed()
        => (ModifierKeys & (Keys.Shift | Keys.Control)) switch {
            Keys.Shift                =>  1,
            Keys.Control              => 20,
            Keys.Shift | Keys.Control => 10,
            _                         =>  5
        };

    private Graphics CreateCanvasGraphics()
    {
        var graphics = Graphics.FromImage(_bmp);
        graphics.FillRectangle(Brushes.White, _rect);
        return graphics;
    }

    private PictureBox CreateCanvas() => new() {
        Image = _bmp,
        SizeMode = PictureBoxSizeMode.AutoSize,
        Margin = CanvasMargin,
    };

    private AlertBar CreateAlertBar() => new() {
        Width = _rect.Width,
        Margin = CanvasMargin,
    };

    private TableLayoutPanel CreateHelpButtons()
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
        toggles.Controls.Add(_help);

        return toggles;
    }

    private Button CreateStop()
        => new BitmapButton(enabledBitmapFilename: "stop.bmp",
                            disabledBitmapFilename: "stop-faded.bmp",
                            _showHideTips.Height) { Visible = false };

    private AnimatedBitmapCheckBox CreateCharting()
        => new(from i in Enumerable.Range(1, 6)
               select new CheckBoxBitmapFilenamePair($"chart{i}.bmp",
                                                     $"chart{i}-gray.bmp"),
               _showHideTips.Height,
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
        infoBar.Controls.Add(_helpButtons, column: 3, row: 0);

        // Must be after adding _helpButtons.
        infoBar.Height = _helpButtons.Height;

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
    }

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

        // TODO: These don't conceptually belong here, but they need to trigger
        // under the same conditions that update the speed. Refactor so the
        // code makes more sense, or avoid carrying over this confusion when
        // splitting up and rewriting UpdateStatus--as will have to be done
        // the status bar is converted from a label to a toolstrip or other
        // collection of multiple controls.
        _magnify.UpdateToolTip();
        _help.UpdateToolTip();

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

    private void SubscribePrivateHandlers()
    {
        Util.Cleanup += delegate { Dispose(); };
        _nonessentialTimer.Tick += delegate { UpdateStatus(); };

        _canvas.MouseMove += canvas_MouseMove;
        _canvas.MouseDown += canvas_MouseDown;
        _canvas.MouseClick += canvas_MouseClick;
        _canvas.MouseWheel += canvas_MouseWheel;

        _stop.Click += stop_Click;
        _charting.CheckedChanged += delegate { UpdateCharting(); };
        _showHideTips.Click += showHideTips_Click;
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

    private void canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        Focus();
        if (GotKey.Super) _suppressStartMenu = true;
    }

    private async void canvas_MouseClick(object? sender, MouseEventArgs e)
    {
        if (!_rect.Contains(e.Location)) return;

        switch (e.Button) {
        case MouseButtons.Left when GotKey.Super:
            InstantFill(e.Location, Color.Black);
            break;

        case MouseButtons.Left:
            _bmp.SetPixel(e.Location.X, e.Location.Y, Color.Black);
            _canvas.Invalidate(e.Location);
            break;

        case MouseButtons.Right when GotKey.Super:
            await RecursiveFloodFillAsync(e.Location, Color.Orange);
            break;

        case MouseButtons.Right when GotKey.Alt:
            await FloodFillAsync(new RandomFringe<Point>(_generator),
                                 e.Location,
                                 Color.Yellow);
            break;

        case MouseButtons.Right:
            await FloodFillAsync(new StackFringe<Point>(),
                                 e.Location,
                                 Color.Red);
            break;

        case MouseButtons.Middle when GotKey.Alt:
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

        if (!GotKey.Shift) {
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
        } else if (GotKey.Ctrl) {
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

    private void tips_DocumentCompleted(object sender,
                                        WebBrowserDocumentCompletedEventArgs e)
        => _tips.Size = _tips.Document.Body.ScrollRectangle.Size;

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
        var name = $"Job {_jobsEver} ({label} fill)";
        var charter = Charter.StartNew(name, _alert, _switcher);

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
            _canvas.Invalidate();

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
            _canvas.Invalidate();

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

    private readonly IPanelSwitcher _switcher;

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

    private readonly TableLayoutPanel _helpButtons;

    private readonly DualUseButton _magnify;

    private readonly Button _stop;

    private readonly AnimatedBitmapCheckBox _charting;

    private readonly Button _showHideTips = new() {
        // Placeholder text for height computation. Without this, some other
        // parts of the infobar don't scale correctly at >100% display scaling.
        Text = "??? Tips",

        AutoSize = true,
        Margin = new(left: 0, top: 0, right: Pad, bottom: 0),
    };

    private readonly DualUseButton _help;

    private readonly MyWebBrowser _tips = new() {
        Visible = false,
        AutoSize = true,
        ScrollBarsEnabled = false,
        Url = Files.GetDocUrl("tips.html"),
    };

    private readonly Func<int, int> _generator =
        Permutations.CreateRandomGenerator();

    private readonly Carousel<NeighborEnumerationStrategy>
    _neighborEnumerationStrategies;

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

    private bool _suppressStartMenu = false;
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

    private sealed record Style(Font Font,
                                Color Color,
                                Cursor Cursor,
                                bool TabStop) {
        internal void ApplyTo(Control control)
        {
            control.Font = Font;
            control.ForeColor = Color;
            control.Cursor = Cursor;
            control.TabStop = TabStop;
        }
    }

    private const int FontSize = 10;

    private static Font RegularFont { get; } =
        new("Segoe UI Semibold", FontSize, FontStyle.Regular);

    private static Font UnderlinedFont { get; } =
        new(RegularFont, FontStyle.Underline);

    private static Style StaticStyle { get; } =
        new(Font: RegularFont,
            Color: Color.Black,
            Cursor: Cursors.Arrow,
            TabStop: false);

    private static Style LinkStyle { get; } = StaticStyle with {
        Color = Color.FromArgb(0, 0, 238),
        Cursor = Cursors.Hand,
        TabStop = true,
    };

    private static Style LinkHoverStyle { get; } = LinkStyle with {
        Color = Color.FromArgb(0, 80, 238),
    };

    private static Style FocusedLinkStyle { get; } = LinkStyle with {
        Font = UnderlinedFont,
    };

    private static Style FocusedLinkHoverStyle { get; } = LinkHoverStyle with {
        Font = UnderlinedFont,
    };

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
        var style = (action: _onClick,
                     hover: _content.HasMousePointer(),
                     cue: ShouldSimulateFocusCue) switch {
            (action: null, hover: _,     cue: _)     => StaticStyle,
            (action: _,    hover: false, cue: false) => LinkStyle,
            (action: _,    hover: false, cue: true)  => FocusedLinkStyle,
            (action: _,    hover: true,  cue: false) => LinkHoverStyle,
            (action: _,    hover: true,  cue: true)  => FocusedLinkHoverStyle,
        };

        style.ApplyTo(_content);

        HideContentCaret();
    }

    private bool ShouldSimulateFocusCue
        => _content.Focused && _content.SelectionLength == 0;

    private void HideContentCaret()
    {
        if (_content.Focused && !User32.HideCaret(_content.Handle))
            Warn("Couldn't hide alert caret.");
    }

    private void RemoveUnderline()
    {
        _content.Font = RegularFont;
        HideContentCaret();
    }

    private void SubscribePrivateHandlers()
    {
        _content.Click += content_Click;
        _content.DoubleClick += content_DoubleClick;
        _content.GotFocus += content_GotFocus;
        _content.LostFocus += content_LostFocus;
        _content.MouseEnter += delegate { UpdateStyle(); };
        _content.MouseLeave += delegate { UpdateStyle(); };
        _content.MouseDown += delegate { RemoveUnderline(); };
        _content.MouseUp += delegate { UpdateStyle(); };
        _content.KeyDown += content_KeyDown;

        _dismiss.Click += dismiss_Click;
    }

    private void content_Click(object? sender, EventArgs e)
    {
        if (_content.SelectionLength == 0) RunClickAction();
    }

    private void content_DoubleClick(object? sender, EventArgs e)
    {
        _content.DeselectAll();
        Clipboard.SetText(_content.Text);
    }

    private void content_GotFocus(object? sender, EventArgs e)
    {
        UpdateStyle();
        _content.SelectionStart =_content.SelectionLength = 0;
    }

    private void content_LostFocus(object? sender, EventArgs e)
    {
        _content.DeselectAll();
        UpdateStyle();
    }

    private void content_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode) {
        case Keys.Space or Keys.Enter:
            RunClickAction();
            break;

        case Keys.Left or Keys.Right or Keys.Home or Keys.End:
            RemoveUnderline();
            break;

        default:
            break;
        }
    }

    private void dismiss_Click(object? sender, EventArgs e)
    {
        Hide();
        _onDismiss?.Invoke();
    }

    private void RunClickAction()
    {
        if (_onClick is Action action) {
            action();
            _dismiss.Focus();
        }
    }

    private readonly TextBox _content = new() {
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = Padding.Empty,
        BorderStyle = BorderStyle.None,
        ReadOnly = true,
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
/// A square button to launch the system Magnifier or configure it in Settings.
/// </summary>
internal sealed class MagnifyButton : ApplicationButton {
    internal MagnifyButton(int sideLength, AlertBar alert)
            : base(executablePath: Files.GetSystem32ExePath("magnify"),
                   sideLength: sideLength,
                   fallbackDescription: "Magnifier")
        => _alert = alert;

    private protected override string ModifiedToolTip => "Magnifier Settings";

    private protected override void OnMainClick(EventArgs e)
    {
        base.OnMainClick(e);

        if (_checkMagnifierSettings && HaveMagnifierSmoothing) {
            _alert.Show($"{Ch.Gear} You may want to turn off {Ch.Ldquo}"
                        + $"Smooth edges of images and text{Ch.Rdquo}.",
                        onClick: OpenMagnifierSettings,
                        onDismiss: () => _checkMagnifierSettings = false);
        }
    }

    private protected override void OnModifiedClick(EventArgs e)
        => OpenMagnifierSettings();

    private static bool HaveMagnifierSmoothing
    {
        get {
            const string key =
                @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\ScreenMagnifier";

            try {
                // Check if smoothing is on. If the Magnifier has never been
                // configured, the key exists not the value. Then the default
                // behavior is to smooth, as with a truthy (nonzero) value.
                return Registry.GetValue(keyName: key,
                                         valueName: "UseBitmapSmoothing",
                                         defaultValue: 1)
                        is int and not 0;
            } catch (SystemException ex) when (ex is SecurityException
                                                  or IOException) {
                Warn("Couldn't check for magnifier smoothing.");
                return false;
            }
        }
    }

    private static void OpenMagnifierSettings()
        => Shell.Execute("ms-settings:easeofaccess-magnifier");

    private static void Warn(string message)
        => message.Dump($"Warning ({nameof(MagnifyButton)})");

    private readonly AlertBar _alert;

    private bool _checkMagnifierSettings = true;
}

/// <summary>
/// A square button to launch an application, showing an icon and (optionally)
/// toolip obtained from the executable's metadata.
/// </summary>
internal abstract class ApplicationButton : DualUseButton {
    internal ApplicationButton(string executablePath,
                               int sideLength,
                               string? fallbackDescription = null)
    {
        Width = Height = sideLength;
        BackgroundImageLayout = ImageLayout.Stretch;

        _path = executablePath;

        MainToolTip = FileVersionInfo.GetVersionInfo(_path).FileDescription
                        ?? fallbackDescription
                        ?? string.Empty;

        _bitmap = CreateBitmap(_path);
        try {
            BackgroundImage = _bitmap;
        } catch {
            _bitmap.Dispose();
            throw;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _bitmap.Dispose();
        base.Dispose(disposing);
    }

    private protected override string MainToolTip { get; }

    private protected override void OnMainClick(EventArgs e)
        => Shell.Execute(_path);

    private static Bitmap CreateBitmap(string path)
    {
        // TODO: Degrade gracefully and use some generic icon on failure.
        var hIcon =
            Shell32.ExtractIconOrThrow(Process.GetCurrentProcess().Handle,
                                       path,
                                       0);

        try {
            return Icon.FromHandle(hIcon).ToBitmap();
        } finally {
            User32.DestroyIconOrThrow(hIcon);
        }
    }

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
        if ((User32.WM)msg.Msg is User32.WM.KEYUP or User32.WM.SYSKEYUP)
            PreviewKeyUp?.Invoke(this, new((Keys)msg.WParam | ModifierKeys));

        return base.PreProcessMessage(ref msg);
    }

    internal event PreviewKeyDownEventHandler? PreviewKeyUp = null;
}

/// <summary>Help launcher button.</summary>
/// <remarks>
/// This is a bridge between <see cref="MainPanel"/> and
/// <see cref="HelpViewer"/>.
/// </remarks>
internal sealed class HelpButton : DualUseButton {
    internal HelpButton(HelpViewerSupplier supplier, IPanelSwitcher switcher)
    {
        (_supplier, _switcher) = (supplier, switcher);

        Text = "Help";
        AutoSize = true;
    }

    private protected override string MainToolTip
        => _helpPanel is null ? "View the full help in a new panel"
                              : "Go to the panel with the full help";

    private protected override string ModifiedToolTip
        => "Open the full help in your web browser";

    private protected override async void OnMainClick(EventArgs e)
    {
        if (_helpPanel is null)
            await OpenHelp();
        else
            _switcher.Switch(_helpPanel);
    }

    private protected override void OnModifiedClick(EventArgs e)
    {
        var uri = Files.GetDocUrl(FileName);

        if (!uri.SchemeIs(Uri.UriSchemeFile)) {
            throw new InvalidOperationException(
                    "Bug: Help URI isn't a file:/// URL");
        }

        Shell.Execute(uri.AbsoluteUri);
    }

    private const string Title = "Flood Fill Visualization - Help";

    private const string FileName = "help.html";

    private static void help_Navigating(object sender,
                                        HelpViewerNavigatingEventArgs e)
    {
        // Open file:// URLs normally, inside the help browser.
        if (e.Uri.SchemeIs(Uri.UriSchemeFile)) return;

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

    private void helpPanel_PanelClosed(object? sender, EventArgs e)
    {
        _helpPanel = null;
        UpdateToolTip();
    }

    private async Task OpenHelp()
    {
        Enabled = false;

        var help = await _supplier();
        help.Source = Files.GetDocUrl(FileName);
        help.Navigating += help_Navigating;
        _helpPanel = _switcher.DisplayForeground(help.WrappedControl, Title);
        _helpPanel.PanelClosed += helpPanel_PanelClosed;
        UpdateToolTip();

        Enabled = true;
    }

    private readonly HelpViewerSupplier _supplier;

    private readonly IPanelSwitcher _switcher;

    private OutputPanel? _helpPanel = null;
}

/// <summary>
/// A button with a primary (non-Shift) action and secondary (Shift) action,
/// and a tooltip that changes accordingly.
/// </summary>
/// <remarks>
/// Modifier keys are checked when the button is clicked, but the state can
/// also be refreshed, so the tooltip text can change immediately, even when
/// the control is not focused and the tooltip is currently visible.
/// </remarks>
internal abstract class DualUseButton : Button {
    internal DualUseButton()
    {
        Margin = Padding.Empty;
        _toolTip = new(_components) { ShowAlways = true };
    }

    internal void UpdateToolTip() => _toolTip.SetToolTip(this, CurrentToolTip);

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdateToolTip();
    }

    protected override void OnClick(EventArgs e)
    {
        var doModified = GotKey.Shift;

        base.OnClick(e);

        if (doModified)
            OnModifiedClick(e);
        else
            OnMainClick(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _components.Dispose();
        base.Dispose(disposing);
    }

    private protected abstract string MainToolTip { get; }

    private protected abstract string ModifiedToolTip { get; }

    private protected abstract void OnMainClick(EventArgs e);

    private protected abstract void OnModifiedClick(EventArgs e);

    private string CurrentToolTip
        => GotKey.Shift ? ModifiedToolTip : MainToolTip;

    private readonly IContainer _components = new Container();

    private readonly ToolTip _toolTip;
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
        => new(Path.Combine(QueryDirectory, "doc", filename));

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
    internal static Charter
    StartNew(string name, AlertBar alert, IPanelSwitcher switcher)
        => new(name, alert, switcher);

    internal void Finish()
    {
        _timer.Stop();

        var chart = MakeChart();
        CustomizeSeries(chart);
        CustomizeArea(chart);
        TryCustomizeToolTip(chart);

        DisplayChart(chart);
    }

    internal void Update() => _times.Add(_timer.Elapsed);

    private const float LabelFontSize = 10;

    private const int ScrollBarSize = 17;

    private const int ToolTipDelay = 5;

    private Charter(string name, AlertBar alert, IPanelSwitcher switcher)
        => (_name, _alert, _switcher) = (name, alert, switcher);

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

    private void DisplayChart(Chart chart)
    {
        var panel = _switcher.DisplayBackground(chart, _name);

        _alert.Show($"{_name} has charted.", onClick: () => {
            if (_switcher.TrySwitch(panel)) {
                _alert.Hide();
            } else {
                _alert.Show($"{_name}{Ch.Rsquo}s chart was closed and"
                            + $" can{Ch.Rsquo}t be shown.");
            }
        });
    }

    private static void Warn(string message)
        => message.Dump($"Warning ({nameof(Charter)})");

    private readonly string _name;

    private readonly AlertBar _alert;

    private readonly IPanelSwitcher _switcher;

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

/// <summary>Convienice properties for polling modifier keys.</summary>
internal static class GotKey {
    internal static bool Shift => Control.ModifierKeys.HasFlag(Keys.Shift);

    internal static bool Ctrl => Control.ModifierKeys.HasFlag(Keys.Control);

    internal static bool Alt => Control.ModifierKeys.HasFlag(Keys.Alt);

    internal static bool Super
        => Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);
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

/// <summary>Access to the winbase.h/kernel32.dll Windows API.</summary>
internal static class Kernel32 {
    internal static void ThrowLastError()
        => throw new Win32Exception(Marshal.GetLastWin32Error());
}

/// <summary>Access to the ntdll.dll Windows API.</summary>
internal static class NtDll {
    /// <remarks>
    /// <see cref="NtQueryTimerResolution"/> returns times in units of 100 ns.
    /// </remarks>
    internal const double HundredNanosecondsPerMillisecond = 10_000.0;

    [DllImport("ntdll")]
    internal static extern int
    NtQueryTimerResolution(out uint MinimumResolution,
                           out uint MaximumResolution,
                           out uint CurrentResolution);
}

/// <summary>Access to the shellapi.h/shell32.dll Windows API.</summary>
internal static class Shell32 {
    // TODO: Use a SafeHandle-based way instead. (See User32.DestroyIcon.)
    internal static IntPtr ExtractIconOrThrow(IntPtr hInst,
                                              string pszExeFileName,
                                              uint nIconIndex)
    {
        var hIcon = ExtractIcon(hInst, pszExeFileName, nIconIndex);
        if (hIcon == IntPtr.Zero) Kernel32.ThrowLastError();
        return hIcon;
    }

    [DllImport("shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr ExtractIcon(IntPtr hInst,
                                             string pszExeFileName,
                                             uint nIconIndex);
}

/// <summary>Access to the winuser.h/user32.dll Windows API.</summary>
internal static class User32 {
    // These are 16-bit values, but using a 64-bit type ensures that
    // mistakenly casting from a Message.WParam that does not represent a
    // virtual key code will not inadventently match something.
    internal enum VK : ulong {
        LWIN = 0x5B,
        RWIN = 0x5C,
    }

    internal enum WM : uint {
        KEYUP    = 0x0101,
        SYSKEYUP = 0x0105,
    }

    [DllImport("user32")]
    internal static extern bool HideCaret(IntPtr hWnd);

    // TODO: Use a SafeHandle-based way instead. (See Shell32.ExtractIcon.)
    internal static void DestroyIconOrThrow(IntPtr hIcon)
    {
        if (!DestroyIcon(hIcon)) Kernel32.ThrowLastError();
    }

    [DllImport("user32", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
