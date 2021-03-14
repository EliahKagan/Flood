<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

// I'm not sure if this can work. As written, it doesn't render relative to the
// correct form.

const int padding = 10;

var overlay = new Form {
    Width = 300,
    Height = 200,
    FormBorderStyle = FormBorderStyle.None,
};

var panel = new Panel {
    BackColor = Color.CornflowerBlue,
};

panel.Click += delegate {
    var parent = (Form)panel.Parent;

    overlay.Location = new Point(x: parent.Location.X + padding,
                                 y: parent.Location.Y + padding);

    overlay.ShowDialog(panel);
};

//panel.VisibleChanged += delegate {
//    if (panel.Parent is null || !panel.Visible) return;
//
//    overlay.Location = new Point(x: panel.Location.X + padding,
//                                 y: panel.Location.Y + padding);
//
//    overlay.ShowDialog(panel);
//};

panel.Dump();
//Task.Delay(250);


//var parent = (Form)panel.Parent;
//overlay.ShowDialog(panel);
