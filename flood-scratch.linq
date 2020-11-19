<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

// flood-scratch.linq - Prototype for interactive flood-fill visualizer.

var canvas = new PictureBox { Size = new Size(width: 600, height: 600) };
var bmp = new Bitmap(width: canvas.Width, height: canvas.Height);
canvas.Image = bmp;

var graphics = Graphics.FromImage(bmp);
var rectangle = new Rectangle(new Point(0, 0), canvas.Size);
graphics.FillRectangle(Brushes.White, rectangle);

var pen = new Pen(Color.Black);
var oldLocation = Point.Empty;

var generator = CreateRandomGenerator();

var neighborEnumerationStrategies = new Carousel<NeighborEnumerationStrategy>(
    new UniformStrategy(),
    new RandomPerFillStrategy(generator),
    new RandomEachTimeStrategy(generator),
    new RandomPerPixelStrategy(canvas.Size, generator));

var status = new Label {
    AutoSize = true,
    Font = new Font(TextBox.DefaultFont.FontFamily, 11),
};

void UpdateStatus()
    => status.Text = $"Neighbor enumeration strategy:"
                   + $" {neighborEnumerationStrategies.Current}";

UpdateStatus();

var showHideTips = new Button {
    Text = "Show Tips",
    AutoSize = true,
    Margin = new Padding(left: 0, top: 0, right: 2, bottom: 0),
};

var openCloseHelp = new Button {
    Text = "Open Help",
    AutoSize = true,
    Margin = new Padding(left: 2, top: 0, right: 0, bottom: 0),
};

var buttons = new TableLayoutPanel {
    RowCount = 1,
    ColumnCount = 2,
    GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
    AutoSize = true,
    Anchor = AnchorStyles.Top | AnchorStyles.Right,
    Margin = new Padding(0),
};
buttons.Controls.Add(showHideTips);
buttons.Controls.Add(openCloseHelp);

var infoBar = new TableLayoutPanel {
    RowCount = 1,
    ColumnCount = 2,
    GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
    Size = new Size(width: canvas.Width, height: 30),
};
infoBar.Controls.Add(status);
infoBar.Controls.Add(buttons);

var tips = new WebBrowser {
    Visible = false,
    // FIXME: Make this work with width: canvas.Width (600).
    Size = new Size(width: 700, height: 200),
    Url = GetDocUrl("tips.html"),
};

var ui = new TableLayoutPanel {
    RowCount = 3,
    ColumnCount = 1,
    GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
    AutoSize = true,
};
ui.Controls.Add(canvas);
ui.Controls.Add(infoBar);
ui.Controls.Add(tips);

OutputPanel? helpPanel = null;

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

    var scrollingDown = e.Delta < 0;
    if (e.Delta == 0) return; // I'm not sure if this is possible.

    if ((Control.ModifierKeys & Keys.Shift) == 0) {
        // Scrolling without Shift cycles neighbor enumeration strategies.
        if (scrollingDown)
            neighborEnumerationStrategies.CycleNext();
        else
            neighborEnumerationStrategies.CyclePrev();
    } else if (neighborEnumerationStrategies.Current
                is ConfigurableNeighborEnumerationStrategy strategy) {
        // Scrolling with Shift cycles substrategies instead.
        if (scrollingDown)
            strategy.CycleNextSubStrategy();
        else
            strategy.CyclePrevSubStrategy();
    } // TODO: Maybe show some message on a nonconfigurable current strategy.

    UpdateStatus();
};

showHideTips.Click += delegate {
    if (tips.Visible) {
        tips.Hide();
        showHideTips.Text = "Show Tips";
    } else {
        tips.Show();
        showHideTips.Text = "Hide Tips";
    }
};

openCloseHelp.Click += delegate {
    const string title = "Flood Fill Visualization - Help";

    if (helpPanel is not null) {
        helpPanel.Close();
        return;
    }

    var help = new WebBrowser { Url = GetDocUrl("help.html") };
    helpPanel = PanelManager.DisplayControl(help, title);

    helpPanel.PanelClosed += delegate {
        helpPanel = null;
        openCloseHelp.Text = "Open Help";
    };

    openCloseHelp.Text = "Close Help";
};

ui.Dump("Flood Fill Visualization");

static Func<int, int> CreateRandomGenerator()
    => new Random(RandomNumberGenerator.GetInt32(int.MaxValue)).Next;

async Task FloodFillAsync(IFringe<Point> fringe, Point start, Color toColor)
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

        if (area++ % speed == 0) await Task.Delay(10);

        bmp.SetPixel(src.X, src.Y, toColor);
        canvas.Invalidate();

        foreach (var dest in supplier(src)) fringe.Insert(dest);
    }
}

static int DecideSpeed()
    => (Control.ModifierKeys & (Keys.Shift | Keys.Control)) switch {
        Keys.Shift => 1,
        Keys.Control => 20,
        Keys.Shift | Keys.Control => 10,
        _ => 5
    };

static Uri GetDocUrl(string filename)
{
    var dir = Path.GetDirectoryName(Util.CurrentQueryPath);
    if (dir is null)
        throw new NotSupportedException("Can't find query directory");

    return new Uri(Path.Combine(dir, filename));
}

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
    private protected NeighborEnumerationStrategy(string name) => Name = name;

    public override string ToString() => Name;

    private protected string Name { get; }

    internal abstract Func<Point, Point[]> GetSupplier();
}

internal abstract class ConfigurableNeighborEnumerationStrategy
        : NeighborEnumerationStrategy {
    private protected ConfigurableNeighborEnumerationStrategy(string name)
        : base(name) { }

    public override string ToString() => $"{Name} - {Detail}";

    internal abstract void CycleNextSubStrategy();

    internal abstract void CyclePrevSubStrategy();

    private protected abstract string Detail { get; }
}

internal sealed class UniformStrategy
        : ConfigurableNeighborEnumerationStrategy {
    internal UniformStrategy() : this(FastEnumInfo<Direction>.GetValues()) { }

    internal UniformStrategy(params Direction[] uniformOrder) : base("Uniform")
        => _uniformOrder = uniformOrder[..];

    private protected override string Detail
        => new string(Array.ConvertAll(_uniformOrder,
                                       direction => direction.ToString()[0]));

    internal override Func<Point, Point[]> GetSupplier()
    {
        var uniformOrder = _uniformOrder[..];

        return src => Array.ConvertAll(uniformOrder,
                                       direction => src.Go(direction));
    }

    internal override void CycleNextSubStrategy()
        => _uniformOrder.CycleNextPermutation();

    internal override void CyclePrevSubStrategy()
        => _uniformOrder.CyclePrevPermutation();

    private readonly Direction[] _uniformOrder;
}

internal sealed class RandomPerFillStrategy : NeighborEnumerationStrategy {
    internal RandomPerFillStrategy(Func<int, int> generator)
            : base("Random per fill")
        => _generator = generator;

    internal override Func<Point, Point[]> GetSupplier()
    {
        var order = FastEnumInfo<Direction>.GetValues();
        order.Shuffle(_generator);
        return src => Array.ConvertAll(order, direction => src.Go(direction));
    }

    private readonly Func<int, int> _generator;
}

internal sealed class RandomEachTimeStrategy : NeighborEnumerationStrategy {
    internal RandomEachTimeStrategy(Func<int, int> generator)
            : base("Random each time")
        => _supply = src => {
            var neighbors = FastEnumInfo<Direction>.ConvertValues(
                                direction => src.Go(direction));
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
                var directions = FastEnumInfo<Direction>.GetValues();
                directions.Shuffle(generator);
                perPixelOrders[x, y] = directions;
            }
        }

        return perPixelOrders;
    }

    private readonly Func<Point, Point[]> _supplier;
}

// TODO: Add WhirlpoolStrategy (which will be (counter)clockwise configurable).

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

internal static class Permutations {
    internal static void Shuffle<T>(this T[] items, Func<int, int> generator)
    {
        for (var right = items.Length; right > 0; --right) {
            var left = generator(right);
            items.Swap(left, right - 1);
        }
    }

    internal static void CycleNextPermutation<T>(this T[] items)
        => items.CyclePermutation(Comparer<T>.Default);

    internal static void CyclePrevPermutation<T>(this T[] items)
        => items.CyclePermutation(ReverseComparer<T>.Default);

    // This cycles from max to min but is otherwise the algorithm described in:
    // https://en.wikipedia.org/wiki/Permutation#Generation_in_lexicographic_order
    private static void CyclePermutation<T>(this T[] items,
                                            IComparer<T> comparer)
    {
        var right = items.Length - 2;
        while (right >= 0
                && comparer.Compare(items[right], items[right + 1]) >= 0)
            --right;

        if (right < 0) {
            // This is the last permutaton (w.r.t. comparer) so cycle around.
            Array.Reverse(items);
        } else {
            // Go to the permutation that comes next (w.r.t. comparer).
            var left = items.Length - 1;
            while (comparer.Compare(items[right], items[left]) >= 0) --left;
            Swap(items, right, left);
            ReverseBetween(items, right + 1, items.Length);
        }
    }

    private static void Swap<T>(this T[] items, int i, int j)
        => (items[i], items[j]) = (items[j], items[i]);

    private static void ReverseBetween<T>(this T[] items,
                                          int startInclusive,
                                          int endExclusive)
        => Array.Reverse(items, startInclusive, endExclusive - startInclusive);
}

internal sealed class ReverseComparer<T> : IComparer<T> {
    internal static IComparer<T> Default { get; } =
        new ReverseComparer<T>(Comparer<T>.Default);

    public int Compare(T? lhs, T? rhs) => _comparer.Compare(rhs, lhs);

    internal ReverseComparer(IComparer<T> comparer) => _comparer = comparer;

    private readonly IComparer<T> _comparer;
}

internal static class FastEnumInfo<T> where T : struct, Enum {
    internal static T[] GetValues() => _values[..];

    internal static TOutput[]
    ConvertValues<TOutput>(Converter<T, TOutput> converter)
        => Array.ConvertAll(_values, converter);

    private static readonly T[] _values = Enum.GetValues<T>();
}
