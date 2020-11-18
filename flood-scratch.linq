<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

var generator = CreateRandomGenerator();

var canvas = new PictureBox { Size = new Size(width: 600, height: 600) };
var bmp = new Bitmap(width: canvas.Width, height: canvas.Height);
canvas.Image = bmp;

var graphics = Graphics.FromImage(bmp);
var rectangle = new Rectangle(new Point(0, 0), canvas.Size);
graphics.FillRectangle(Brushes.White, rectangle);

var pen = new Pen(Color.Black);
var oldLocation = Point.Empty;

var neighborEnumerationStrategies = new Carousel<NeighborEnumerationStrategy>(
    new UniformStrategy(),
    new RandomEachTimeStrategy(generator),
    new RandomPerPixelStrategy(canvas.Size, generator));

var status = new Label { Width = canvas.Width, Height = 20 };
UpdateStatus();

void UpdateStatus()
    => status.Text = $"Neighbor enumeration strategy:"
                   + $" {neighborEnumerationStrategies.Current}";

canvas.MouseMove += (sender, args) => {
    if (args.Button == MouseButtons.Left) {
        graphics.DrawLine(pen, oldLocation, args.Location);
        canvas.Invalidate();
    }
    
    oldLocation = args.Location;
};

canvas.MouseClick += async (sender, e) => {
    if (!rectangle.Contains(e.Location)) return;
    
    switch (e.Button) {
    case MouseButtons.Left:
        bmp.SetPixel(e.Location.X, e.Location.Y, Color.Black);
        canvas.Invalidate();
        break;
    
    case MouseButtons.Right when (Control.ModifierKeys & Keys.Alt) != 0:
        await FloodFillAsync(new RandomFringe<Point>(generator),
                             e.Location,
                             Color.Yellow);
        break;
    
    case MouseButtons.Right:
        await FloodFillAsync(new StackFringe<Point>(),
                             e.Location,
                             Color.Red);
        break;
    
    case MouseButtons.Middle:
        await FloodFillAsync(new QueueFringe<Point>(),
                             e.Location,
                             Color.Blue);
        break;
    
    default:
        break; // Other buttons do nothing.
    }
};

canvas.MouseWheel += (sender, e) => {
    if (!rectangle.Contains(e.Location)) return;
    
    if (e.Delta < 0) // Got upward scroll from wheel.
        neighborEnumerationStrategies.CycleNext();
    else if (e.Delta > 0) // Got downward scroll from wheel.
        neighborEnumerationStrategies.CyclePrev();
    else return; // I'm not sure if this is possible.
    
    UpdateStatus();
};

async Task FloodFillAsync(IFringe<Point> fringe,
                          Point start,
                          Color toColor)
{
    var fromArgb = bmp.GetPixel(start.X, start.Y).ToArgb();
    if (fromArgb == toColor.ToArgb()) return;
    
    var speed = DecideSpeed();
    var supplier = neighborEnumerationStrategies.Current.GetSupplier();
    var area = 0;

    for (fringe.Insert(start); fringe.Count != 0; ) {
        var src = fringe.Extract();

        if (!rectangle.Contains(src)
                || bmp.GetPixel(src.X, src.Y).ToArgb() != fromArgb)
            continue;
        
        if (area++ % speed == 0) await Task.Delay(1);
        
        bmp.SetPixel(src.X, src.Y, toColor);
        canvas.Invalidate();
        //bmp.GetPixel(src.X, src.Y).Dump("filled color");
        //Debug.Assert(bmp.GetPixel(src.X, src.Y).ToArgb() == toColor.ToArgb());

        foreach (var dest in supplier(src)) fringe.Insert(dest);
    }
    
    //canvas.Invalidate();
}

static int DecideSpeed()
    => (Control.ModifierKeys & (Keys.Shift | Keys.Control)) switch {
        Keys.Shift => 1,
        Keys.Control => 20,
        Keys.Shift | Keys.Control => 10,
        _ => 5
    };

var ui = new TableLayoutPanel {
    RowCount = 2,
    ColumnCount = 1,
    GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
    AutoSize = true,
};
ui.Controls.Add(canvas);
ui.Controls.Add(status);
ui.Dump("Watching Paint Dry");

static Func<int, int> CreateRandomGenerator()
    => new Random(RandomNumberGenerator.GetInt32(int.MaxValue)).Next;

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

internal sealed class RandomFringe<T> : IFringe<T> {
    internal RandomFringe(Func<int, int> generator) => _generator = generator;

    public int Count => _items.Count;
    
    public void Insert(T vertex) => _items.Add(vertex);
    
    public T Extract()
    {
        var index = _generator(Count);
        var vertex = _items[index];
        _items[index] = _items[Count - 1];
        _items.RemoveAt(Count - 1);
        return vertex;
    }

    private readonly List<T> _items = new();
    
    private readonly Func<int, int> _generator;
}

//// If this prototype is rewritten with OOP, maybe make this an interface.
//// The issue is that it uses other configuration information.
//internal enum NeighborEnumerationStrategy {
//    Uniform,
//    RandomEachTime,
//    RandomPerFill,
//    //RandomPerPixel,
//    //Whirlpool,
//}

internal enum Direction {
    Left,
    Right,
    Up,
    Down,
}

internal static class PointExtensions {
    internal static Point Go(this Point src, Direction direction)
        => direction switch {
            Direction.Left  => new(src.X - 1, src.Y),
            Direction.Right => new(src.X + 1, src.Y),
            Direction.Up    => new(src.X, src.Y - 1),
            Direction.Down  => new(src.X, src.Y + 1),
            _ => throw new NotSupportedException("Bug: unrecognized direction")
        };
}

internal abstract class NeighborEnumerationStrategy {
    internal NeighborEnumerationStrategy(string name) => Name = name;

    public sealed override string ToString()
    {
        var detail = Detail;
        return detail is null ? Name : $"{Name} - {detail}";
    }

    internal string Name { get; }
    
    internal virtual string? Detail => null;
    
    internal abstract Func<Point, Point[]> GetSupplier();
}

internal sealed class UniformStrategy : NeighborEnumerationStrategy {
    internal UniformStrategy() : this(Direction.Left,
                                      Direction.Right,
                                      Direction.Up,
                                      Direction.Down)
    {
    }

    internal UniformStrategy(params Direction[] uniformOrder) : base("Uniform")
        => _uniformOrder = (Direction[])uniformOrder.Clone();
    
    internal override string Detail
        => new string(Array.ConvertAll(_uniformOrder,
                                       direction => direction.ToString()[0]));
    
    internal override Func<Point, Point[]> GetSupplier()
    {
        var uniformOrder = (Direction[])_uniformOrder.Clone();
        
        return src => Array.ConvertAll(uniformOrder,
                                       direction => src.Go(direction));
    }
    
    // FIXME: Implement methods to cycle the _uniformOrder permutation.
    // (That's why GetSupplier() captures a copy of _uniformOrder.)
    
    private readonly Direction[] _uniformOrder;
}

internal sealed class RandomEachTimeStrategy : NeighborEnumerationStrategy {
    internal RandomEachTimeStrategy(Func<int, int> generator)
            : base("Random each time")
        => _supply = src => {
            var neighbors = new[] {
                src.Go(Direction.Left),
                src.Go(Direction.Right),
                src.Go(Direction.Up),
                src.Go(Direction.Down),
            };
            neighbors.Shuffle(generator);
            return neighbors;
        };
    
    internal override Func<Point, Point[]> GetSupplier() => _supply;
    
    private readonly Func<Point, Point[]> _supply;
}

internal sealed class RandomPerPixelStrategy : NeighborEnumerationStrategy {
    internal RandomPerPixelStrategy(Size size, Func<int, int> generator)
            : base("Random per pixel")
    {
        var perPixelOrders = GeneratePerPixelOrders(size, generator);
        
        _supplier = src => Array.ConvertAll(perPixelOrders[src.X, src.Y],
                                            direction => src.Go(direction));
    }
    
    internal override Func<Point, Point[]> GetSupplier() => _supplier;
    
    private static Direction[,][]
    GeneratePerPixelOrders(Size size, Func<int, int> generator)
    {
        var perPixelOrders = new Direction[size.Width, size.Height][];
        
        for (var x = 0; x < size.Width; ++x) {
            for (var y = 0; y < size.Height; ++y) {
                var directions = new[] {
                    Direction.Left,
                    Direction.Right,
                    Direction.Up,
                    Direction.Down,
                };
                directions.Shuffle(generator);
                perPixelOrders[x, y] = directions;
            }
        }
        
        return perPixelOrders;
    }
    
    private readonly Func<Point, Point[]> _supplier;
}

internal static class Permutations {
    internal static void Shuffle<T>(this T[] items, Func<int, int> generator)
    {
        for (var i = items.Length; i > 0; --i) items.Swap(generator(i), i - 1);
    }
    
    private static void Swap<T>(this T[] items, int i, int j)
        => (items[i], items[j]) = (items[j], items[i]);
}

internal sealed class Carousel<T> {
    internal Carousel(params T[] items) => _items = items[..];
    
    internal T Current => _items[_pos];
    
    internal void CycleNext() => Change(+1);
    
    internal void CyclePrev() => Change(-1);
    
    private void Change(int delta)
        => _pos = (_pos + delta + _items.Length) % _items.Length;
    
    private readonly T[] _items;
    
    private int _pos = 0;
}
