<Query Kind="Statements">
  <NuGetReference Prerelease="true">Microsoft.Web.WebView2</NuGetReference>
  <Namespace>Microsoft.Web.WebView2.WinForms</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

var wv = new WebView2 {
    Source = new Uri("https://www.youtube.com/watch?v=25haxRuZQUk"),
    Size = new Size(width: 600, height: 200),
    Location = new Point(x: 30, y: 30),
};

var panel = new TableLayoutPanel();
panel.Controls.Add(wv);
panel.Dump();

var form = new Form();
form.Controls.Add(panel);
form.Show();
wv.Show();
