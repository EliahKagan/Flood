<Query Kind="Statements">
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

var widget = new Widget {
    Width = 100,
    Height = 30,
};

var enabled = new CheckBox {
    Text = "Enabled",
    Checked = true,
};
enabled.CheckedChanged += delegate {
    widget.ButtonEnabled = enabled.Checked;
};

var ui = new TableLayoutPanel();
ui.Controls.Add(widget);
ui.Controls.Add(enabled);
ui.Dump();

internal sealed class Widget : Control {
    internal Widget()
    {
        _toolTip = new(_components) { ShowAlways = true };
        _toolTip.SetToolTip(_button, "Click here for glory!");
        _toolTip.SetToolTip(_viewfoil, "This button is mean.");

        Controls.Add(_button);
        Controls.Add(_viewfoil);

        _button.EnabledChanged += delegate { SetZOrder(); };

        Size = _viewfoil.Size = _button.Size;
    }

    internal bool ButtonEnabled
    {
        get => _button.Enabled;
        set => _button.Enabled = value;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        _viewfoil.Size = _button.Size = Size;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _components.Dispose();
        base.Dispose(disposing);
    }

    private void SetZOrder()
    {
        if (_button.Enabled)
            _button.BringToFront();
        else
            _viewfoil.BringToFront();
    }

    private readonly IContainer _components = new Container();

    private readonly ToolTip _toolTip;

    private readonly Button _button = new() {
        Text = "Click Me",
        Location = Point.Empty,
    };

    private readonly Viewfoil _viewfoil = new();
}

internal sealed class Viewfoil : Control {
    internal Viewfoil()
    {
        Location = Point.Empty;
        TabStop = false;
        SetStyle(ControlStyles.Opaque, true);
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
