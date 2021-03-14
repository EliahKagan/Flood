<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

// This approach doesn't seem workable. See add-label-dump-scratch.linq for
// why.

var form = new Form {
    Text = "Initially Blank Form",
    Size = new Size(width: 400, height: 300),
};

var panel = new Panel {
    Location = Point.Empty,
    Size = form.ClientSize,
};

form.Controls.Add(panel);

var label = new Label {
    Text = "Dynamically Added Control",
    Location = new Point(x: 20, y: 20),
    Size = new Size(width: 250, height: 100),
};

panel.HandleCreated += delegate {
    form.Controls.Add(label);
    label.BringToFront();
};

Application.Run(form);
