<Query Kind="Statements" />

using System.Threading.Tasks;
using System.Windows.Forms;

var button = new Button { Text = "A button" };
button.EnabledChanged += (_, _) => nameof(button.EnabledChanged).Dump();
button.LostFocus += (_, _)
    => $"{nameof(button.LostFocus)} - {nameof(button.Enabled)}={button.Enabled}".Dump();

var ui = new TableLayoutPanel();
ui.Controls.Add(button);
ui.Dump();

await Task.Delay(1000);
button.Enabled = false;
"Disabled the button.".Dump();
