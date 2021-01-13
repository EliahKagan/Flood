<Query Kind="Statements" />

using System.Threading.Tasks;
using LINQPad.Controls;

var count = 0;
var label = new Label();

label.Rendering += async delegate {
    "Label rendering.".Dump();
    
    for (; ; ) {
        label.Text = count++.ToString();
        await Task.Delay(1000);
    }
};

label.Dump();
