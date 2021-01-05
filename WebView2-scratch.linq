<Query Kind="Statements">
  <NuGetReference>Microsoft.Web.WebView2</NuGetReference>
  <Namespace>Microsoft.Web.WebView2.Core</Namespace>
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
    var wv = new MyWebView2();
    await wv.EnsureCoreWebView2Async();
    var s = wv.CoreWebView2.Settings;
    s.AreHostObjectsAllowed = false;
    s.IsWebMessageEnabled = false;
    s.IsScriptEnabled = false;
    s.AreDefaultScriptDialogsEnabled = false;
    //s.AreDevToolsEnabled = false;
    wv.Source = new Uri(GetDocUrl($"{name.ToLower()}.html"));

    wv.Dump(name);
    var pluginForm = (Form)wv.Parent;

    //var panel = PanelManager.DisplayControl(wv, name);
    //panel.GetType().GetEvents().Dump();
}

internal sealed class MyWebView2 : WebView2 {
    protected override void OnVisibleChanged(EventArgs e)
    {
        // $"{nameof(OnVisibleChanged)} called.".Dump();

        if (CoreWebView2 is not null) base.OnVisibleChanged(e);

        //try {
        //    base.OnVisibleChanged(e);
        //} catch (NullReferenceException ex) {
        //    ex.Dump();
        //    CoreWebView2.Dump();
        //    //throw;
        //}
    }
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
