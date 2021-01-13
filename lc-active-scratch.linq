<Query Kind="Statements" />

using System.Threading.Tasks;
using LINQPad.Controls;

var rendered = false;
var count = 0;
var label = new Label();

label.Rendering += async delegate {
    if (rendered) return;
    
    rendered = true;
    
    for (; ; ) {
        label.Text = count++.ToString();
        await Task.Delay(1000);
    }
};

label.Dump();
