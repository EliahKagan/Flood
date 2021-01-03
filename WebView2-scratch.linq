<Query Kind="Statements">
  <NuGetReference>Microsoft.Web.WebView2</NuGetReference>
  <Namespace>Microsoft.Web.WebView2.WinForms</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

static string GetQueryDirectory()
    => Path.GetDirectoryName(Util.CurrentQueryPath)
        ?? throw new NotSupportedException("Can't find query directory");

static string GetDocUrl(string filename)
    => Path.Combine(GetQueryDirectory(), filename);

foreach (var name in new[] { "Tips", "Help" }) {
    var wv = new WebView2();
    await wv.EnsureCoreWebView2Async();
    var s = wv.CoreWebView2.Settings;
    s.AreHostObjectsAllowed = false;
    s.IsWebMessageEnabled = false;
    s.IsScriptEnabled = false;
    s.AreDefaultScriptDialogsEnabled = false;
    //s.AreDevToolsEnabled = false;
    wv.Source = new Uri(GetDocUrl($"{name.ToLower()}.html"));
    wv.Dump(name);
}

//var wv = new WebView2 {
//    Source = new Uri(GetDocUrl("help.html")),
//    Size = new Size(width: 600, height: 200),
//    Location = new Point(x: 30, y: 30),
//};
//
//wv.Dump("Help");

//var panel = new TableLayoutPanel();
//panel.Controls.Add(wv);
//panel.Dump();
//
//var form = new Form();
//form.Controls.Add(panel);
//form.Show();
//wv.Show();
