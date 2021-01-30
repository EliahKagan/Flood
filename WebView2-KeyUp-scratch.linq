<Query Kind="Statements">
  <NuGetReference Version="1.0.705.50">Microsoft.Web.WebView2</NuGetReference>
  <Namespace>Microsoft.Web.WebView2.Core</Namespace>
  <Namespace>Microsoft.Web.WebView2.WinForms</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

// In Windows Forms, like with the WebBrowser control, the WebView2 control's
// keyboard input is never previewed by the owning form, and like WebBrowser,
// keystrokes don't generate WM_KEYUP, WM_KEYDOWN, WM_SYSKEYUP, or
// WM_SYSKEYDOWN messages in WndProc. But unlike WebBrowser, keystrokes given
// to WebView2 also don't generate those window messages in PreProcessMessage.
// Also, WebView2 inherits directly from Control and does not override
// PreProcessMessage. This is unlike WebBrowser, which inherits from
// WebBrowserBase, which overrides PreProcessMessage.

var webBrowser = new MyWebBrowser {
    Url = new("https://en.wikipedia.org"),
};
webBrowser.Dump(nameof(MyWebBrowser));

var webView2 = new MyWebView2 {
    Source = new("https://en.wikipedia.org"),
};
webView2.Dump(nameof(MyWebView2));
//var panel = PanelManager.DisplayControl(webView2, nameof(MyWebView2));
//panel.Dump();

var pluginForm = (Form)webView2.Parent;
pluginForm.KeyPreview = true;
pluginForm.KeyDown += (_, e) => e.Dump();

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

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        $"{nameof(MyWebView2)}: {_timer.Elapsed.TotalSeconds}: {msg}".Dump();
        return base.ProcessCmdKey(ref msg, keyData);
    }

    // Uncomment to notice the absence of messages on key-up and key-down.
    //protected override void WndProc(ref Message msg)
    //{
    //    $"{nameof(MyWebView2)}: {_timer.Elapsed.TotalSeconds}: {msg}".Dump();
    //    base.WndProc(ref msg);
    //}

    private readonly Stopwatch _timer = Stopwatch.StartNew();
}
