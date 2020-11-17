<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

var canvas = new PictureBox();
canvas.Size = new Size(width: 600, height: 600);
var bmp = new Bitmap(width: canvas.Width, height: canvas.Height);
canvas.Image = bmp;// = new Bitmap(width: canvas.Width, height: canvas.Height);

var graphics = Graphics.FromImage(bmp);
var rectangle = new Rectangle(new Point(0, 0), canvas.Size);
graphics.FillRectangle(Brushes.White, rectangle);

var pen = new Pen(Color.Black);
var oldLocation = Point.Empty;

canvas.MouseMove += (sender, args) => {
    if (args.Button == MouseButtons.Left) {
        graphics.DrawLine(pen, oldLocation, args.Location);
        canvas.Invalidate();
    }
    
    oldLocation = args.Location;
};

canvas.MouseClick += async (sender, args) => {
    if (!rectangle.Contains(args.Location)) return;

    switch (args.Button) {
    case MouseButtons.Left:
        bmp.SetPixel(args.Location.X, args.Location.Y, Color.Black);
        canvas.Invalidate();
        break;
    
    case MouseButtons.Right:
        await FloodFillAsync(new StackFringe<Point>(),
                             args.Location,
                             Color.Red);
        break;
    
    case MouseButtons.Middle:
        await FloodFillAsync(new QueueFringe<Point>(),
                             args.Location,
                             Color.Blue);
        break;
    }
};

async Task FloodFillAsync(IFringe<Point> fringe, Point start, Color toColor)
{
    var fromArgb = bmp.GetPixel(start.X, start.Y).ToArgb();
    if (fromArgb == toColor.ToArgb()) return;
    
    await Task.Yield();

    const int speedup = 15;
    var count = 0;

    for (fringe.Insert(start); fringe.Count != 0; ) {
        var src = fringe.Extract();

        if (!rectangle.Contains(src)
                || bmp.GetPixel(src.X, src.Y).ToArgb() != fromArgb)
            continue;
        
        if (count++ % speedup == 0) await Task.Delay(1);
        
        bmp.SetPixel(src.X, src.Y, toColor);
        canvas.Invalidate();
        bmp.GetPixel(src.X, src.Y).Dump("filled color");
        //Debug.Assert(bmp.GetPixel(src.X, src.Y).ToArgb() == toColor.ToArgb());

        fringe.Insert(new(src.X - 1, src.Y));
        fringe.Insert(new(src.X + 1, src.Y));
        fringe.Insert(new(src.X, src.Y - 1));
        fringe.Insert(new(src.X, src.Y + 1));
    }
    
    //canvas.Invalidate();
}

canvas.Dump();

internal interface IFringe<T> {
    int Count { get; }

    void Insert(T vertex);
    
    T Extract();
}

internal sealed class StackFringe<T> : IFringe<T> {
    public int Count => _stack.Count;

    public void Insert(T vertex) => _stack.Push(vertex);
    
    public T Extract() => _stack.Pop();

    private readonly Stack<T> _stack = new();
}

internal sealed class QueueFringe<T> : IFringe<T> {
    public int Count => _queue.Count;
    
    public void Insert(T vertex) => _queue.Enqueue(vertex);
    
    public T Extract() => _queue.Dequeue();

    private readonly Queue<T> _queue = new();
}
