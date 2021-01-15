<Query Kind="Statements" />

using System.Windows.Forms;

//new WebBrowser().KeyDown += delegate { }; // Throws NotSupportedException.

Control wb = new WebBrowser {
    Url = new("file:///d:/source/repos/Flood/tips.html"),
};

wb.PreviewKeyDown += delegate { nameof(wb.PreviewKeyDown).Dump(); };
wb.KeyDown += delegate { nameof(wb.KeyDown).Dump(); };

wb.Dump();
