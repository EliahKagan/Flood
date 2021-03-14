<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

var toolTip = new ToolTip();

toolTip.Popup += (sender, e) => {
    e.ToolTipSize *= 2; // Increases the tooltip size, but not the text size.
};

toolTip.Draw += (sender, e) => {
    e.Dump(); // Doesn't run, because OwnerDraw is not set.
};

var panel = new Panel {
    BackColor = Color.CornflowerBlue,
};

panel.Click += delegate {
    toolTip.Show("Hello, world!", panel, Point.Empty);
};

panel.Dump();
