<Query Kind="Statements">
  <NuGetReference>Microsoft.Web.WebView2</NuGetReference>
  <Namespace>Microsoft.Web.WebView2.Core</Namespace>
  <Namespace>Microsoft.Web.WebView2.WinForms</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

var webBrowser = new WebBrowser {
    Url = new("https://en.wikipedia.org"),
};
webBrowser.PreviewKeyDown += delegate {
    $"Got {nameof(webBrowser.PreviewKeyDown)}.".Dump(nameof(WebBrowser));
};
webBrowser.Dump(nameof(WebBrowser));

var webView2 = new WebView2 {
    Source = new("https://en.wikipedia.org"),
};
webView2.PreviewKeyDown += delegate {
    $"Got {nameof(webView2.PreviewKeyDown)}.".Dump(nameof(WebView2));
};
webView2.Dump(nameof(WebView2));
