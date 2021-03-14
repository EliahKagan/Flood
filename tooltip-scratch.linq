<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

// This approach doesn't work well because the tooltip lags behind the control
// when the control is moved (such as when the LINQPad window is dragged).

var toolTip = new ToolTip {
    ShowAlways = true,
};

var panel = new Panel {
    BackColor = Color.CornflowerBlue,
};

var panelShown = false;

panel.VisibleChanged += async delegate {
    if (panelShown || !panel.Visible) return;

    panelShown = true;

    for (; ; ) {
        for (var count = 1; count <= 4; ++count) {
            toolTip.Show("Loading" + new string('.', count),
                         panel,
                         Point.Empty);

            await Task.Delay(350);
        }
    }
};

panel.Dump();

for (; ; ) {
    await Task.Delay(100);
    if (panel.Visible) panel.Focus();
}
