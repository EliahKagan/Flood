<Query Kind="Statements">
  <NuGetReference>Microsoft.Web.WebView2</NuGetReference>
  <Namespace>Microsoft.Web.WebView2.Core</Namespace>
  <Namespace>Microsoft.Web.WebView2.WinForms</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

var webBrowser = new MyWebBrowser {
    Url = new("https://en.wikipedia.org"),
};
webBrowser.Dump(nameof(MyWebBrowser));

var webView2 = new MyWebView2 {
    Source = new("https://en.wikipedia.org"),
};
webView2.Dump(nameof(MyWebView2));

internal sealed class MyWebBrowser : WebBrowser {
    public override bool PreProcessMessage(ref Message msg)
    {
        $"{nameof(MyWebBrowser)}: {_timer.Elapsed.TotalSeconds}: {msg}".Dump();
        return base.PreProcessMessage(ref msg);
    }

    private readonly Stopwatch _timer = Stopwatch.StartNew();
}

internal sealed class MyWebView2 : WebView2 {
    public override bool PreProcessMessage(ref Message msg)
    {
        $"{nameof(MyWebView2)}: {_timer.Elapsed.TotalSeconds}: {msg}".Dump();
        return base.PreProcessMessage(ref msg);
    }

    private readonly Stopwatch _timer = Stopwatch.StartNew();
}
