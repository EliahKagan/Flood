<Query Kind="Statements">
  <NuGetReference>Microsoft.Toolkit.Forms.UI.Controls.WebView</NuGetReference>
</Query>

using System.Windows.Forms;
using Microsoft.Toolkit.Forms.UI.Controls;

var wv = new WebView {
    Source = new Uri("https://www.youtube.com/watch?v=25haxRuZQUk"),
};

var panel = new TableLayoutPanel();
panel.Controls.Add(wv);
panel.Dump();

var form = new Form();
form.Controls.Add(panel);
form.Show();
wv.Show();
