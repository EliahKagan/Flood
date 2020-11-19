<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

const string html = @"<!DOCTYPE html>
<html>
  <head>
    <title>Help</title>
  </head>
  <body>
    <h1>A heading</h1>
    <p>
      A paragraph. Yeah.
      Yes.
    </p>
    <p>
      A second paragraph. This is very exciting because:
    </p>
    <ol>
      <li>First reason.</li>
      <li>Second reason.</li>
      <li><em>Italicized third reason.</em></li>
    </ol>
    <p>
      Well, that's about it.
    </p>
  </body>
</html>
";

var showHideHelp = new Button {
    Text = "Show Help",
};

var wb = new WebBrowser {
    Visible = false,
    Size = new Size(width: 600, height: 300),
    Location = new Point(x: 30, y: 30),
    DocumentText = html,
    //Url = new Uri("https://en.m.wiktionary.org/wiki/booyah#Interjection"),
};

showHideHelp.Click += delegate {
    var shown = wb.Visible = !wb.Visible;
    showHideHelp.Text = (shown ? "Hide Help" : "Show Help");
};

var panel = new TableLayoutPanel {
    RowCount = 2,
    ColumnCount = 1,
    GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
    AutoSize = true,
};

panel.Controls.Add(showHideHelp);
panel.Controls.Add(wb);

panel.Dump();

//var form = new Form {
//    Size = Size.Empty,
//    AutoSize = true,
//};
//
//form.Controls.Add(panel);
//form.Show();
