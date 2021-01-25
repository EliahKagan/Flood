<Query Kind="Statements">
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

var button = new Button {
    Width = 100,
    Height = 30,
    Text = "Click Me",
};

var buttonHost = new TippingHost(button, SizeTracking.ChildTracksHost) {
    EnabledToolTip = "Click here for glory!",
    DisabledToolTip = "This button is mean.",
};

var enabled = new CheckBox {
    Text = "Enabled",
    Checked = true,
};
enabled.CheckedChanged += delegate {
    button.Enabled = enabled.Checked;
};

var ui = new TableLayoutPanel();
ui.Controls.Add(buttonHost);
ui.Controls.Add(enabled);
ui.Dump();

/// <summary>
/// Hosts and provides enabled and disabled tooltips to a single control.
/// </summary>
internal sealed class TippingHost : Control {
    internal TippingHost(Control child, SizeTracking sizeTracking)
    {
        (Child, SizeTracking) = (child, sizeTracking);
        _toolTip = new(_components) { ShowAlways = true };

        Controls.Add(Child);
        Controls.Add(_cover);
        _cover.BringToFront();

        Child.EnabledChanged += delegate { UpdateCover(); };
        UpdateCover();
        ResizeHostToChild();
        RegisterSizeTracker(nameof(sizeTracking));

        Child.Location = Point.Empty;
    }

    internal Control Child { get; }

    internal SizeTracking SizeTracking { get; }

    internal string EnabledToolTip
    {
        get => _toolTip.GetToolTip(Child);
        set => _toolTip.SetToolTip(Child, value);
    }

    internal string DisabledToolTip
    {
        get => _toolTip.GetToolTip(_cover);
        set => _toolTip.SetToolTip(_cover, value);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _components.Dispose();
        base.Dispose(disposing);
    }

    private void RegisterSizeTracker(string paramName)
    {
        switch (SizeTracking) {
        case SizeTracking.ChildTracksHost:
            SizeChanged += delegate { ResizeChildToHost(); };
            break;

        case SizeTracking.HostTracksChild:
            Child.SizeChanged += delegate { ResizeHostToChild(); };
            break;

        default:
            throw new ArgumentException(
                    paramName: paramName,
                    message: $"Unrecognized {nameof(SizeTracking)} constant.");
        }
    }

    private void UpdateCover() => _cover.Visible = !Child.Enabled;

    private void ResizeChildToHost() =>  _cover.Size = Child.Size = Size;

    private void ResizeHostToChild() => Size = _cover.Size = Child.Size;

    private readonly IContainer _components = new Container();

    private readonly ToolTip _toolTip;

    private readonly ClearCover _cover = new();
}

internal enum SizeTracking {
    ChildTracksHost,
    HostTracksChild,
}

/// <summary>
/// A transparent control, intended to be inert except for a tooltip.
/// </summary>
/// <remarks>
/// Used by <see cref="TippingHost"/>. <c>ClearCover</c> (but not the broader
/// design of <c>TippingHost</c>) is directly inspired by
/// <c>TransparentSheet</c> in
/// <a href="https://www.codeproject.com/Articles/32083/Displaying-a-ToolTip-when-the-Mouse-Hovers-Over-a">Displaying a ToolTip when the Mouse Hovers Over a Disabled Control</a>.
/// </remarks>
internal sealed class ClearCover : Control {
    internal ClearCover()
    {
        Location = Point.Empty;
        TabStop = false;
        SetStyle(ControlStyles.Opaque, true); // Unintuitive, but intentional.
        UpdateStyles();
    }

    protected override CreateParams CreateParams
    {
        get {
            var value = base.CreateParams;
            value.ExStyle |= (int)WS_EX.TRANSPARENT;
            return value;
        }
    }
}

internal enum WS_EX : uint {
    TRANSPARENT = 0x00000020,
}
