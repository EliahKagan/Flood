<Query Kind="Statements">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
</Query>

var count = 0;
var label = new Label();

label.Rendering += async delegate {
    for (; ; ) {
        label.Text = count++.ToString();
        await Task.Delay(1000);
    }
};

var button = new Button("Reset", delegate { count = 0; });

label.Dump();
button.Dump();
