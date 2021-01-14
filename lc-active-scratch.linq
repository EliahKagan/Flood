<Query Kind="Statements">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
</Query>

var count = 0;
var label = new Label();

void Update()
{
    var elapsed = (count == 1 ? $"{count} second" : $"{count} seconds");

    var thread = $"thread {Thread.CurrentThread.ManagedThreadId}"
                    + $" ({Thread.CurrentThread.GetApartmentState()})";

    var updates = $"{State.Count} updates";

    label.Text = string.Join(Environment.NewLine, elapsed, thread, updates);
}

//async void Loop(object? unused)
//{
//    for (; ; ) {
//        Update();
//        ++count;
//        ++State.Count;
//        await Task.Delay(1000);
//    }
//}

var reset = new Button("Reset", delegate {
    count = 0;
    Update();
});

var collect = new Button("Collect", delegate { GC.Collect(); });

new StackPanel(horizontal: false,
               label,
               new WrapPanel(reset, collect)).Dump();

if (State.Count % 2 == 0) new System.Windows.Forms.Form().Dump();
Util.CreateSynchronizationContext(true);
//SynchronizationContext.Current?.Post(Loop, null);

SynchronizationContext.Current.Dump()?.Post(async delegate {
    for (; ; ) {
        Update();
        ++count;
        ++State.Count;
        await Task.Delay(1000);
    }
}, null);

internal static class State {
    internal static int Count { get; set; } = 0;
}
