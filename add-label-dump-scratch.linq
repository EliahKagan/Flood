<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

// This approach doesn't work because the label doesn't redraw when the control
// is reshown.

var label = new Label {
    Text = "Dynamically Added Control",
    Location = new Point(x: 20, y: 20),
    Size = new Size(width: 250, height: 100),
};

//label.LostFocus += delegate { label.BringToFront(); };

var panel = new Panel {
    BackColor = Color.CornflowerBlue,
};

panel.Dump();

panel.Click += delegate {
    var parent = (Form)panel.Parent;
    //parent.Controls.Dump(depth: 1);
    //(label.Parent == panel.Parent).Dump();
    parent.SuspendLayout();
    parent.Controls.Remove(label);
    parent.Controls.Add(label);
    parent.ResumeLayout();
    label.BringToFront();
};

panel.HandleCreated += delegate {
    var parent = (Form)panel.Parent;
    parent.GetType().Dump();//
    parent.Controls.Add(label);
    //parent.BeginInvoke((MethodInvoker)async delegate {
    //    await Task.Delay(1000);
    //    label.BringToFront();
    //});
    label.BringToFront();
    //parent.Layout += delegate { label.BringToFront(); };
};
