<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

// flood-scratch.linq - Prototype for interactive flood-fill visualizer.

// TODO: Maybe set numerical values in just one place and interpolate them
// into this help text, so that the help text won't become wrong as easily.
// TODO: Write this in Markdown instead of HTML and use CommonMark.NET.
// TODO: Make a stylesheet instead of abusing <h2> to mean <h1>.
const string helpText = @"<!DOCTYPE html>
<html>
  <head>
    <title>Flood (prototype) - Help</title>
  </head>
  <body>
    <h3 id=""table-of-contents"">Table of Contents</h3>
    <ol>
      <li>
        <a href=""#basic-actions"">Basic Actions</a>
      </li>
      <li>
        <a href=""#speed-modifiers"">Speed Modifiers</a>
      </li>
      <li>
        <a href=""#basic-actions"">Neighbor Enumeration Strategies</a>
      </li>
    </ol>
    <hr/>
    <h3 id=""introduction"">Flood-Fill Visualization</h3>
    <p>
      A flood fill is a sparse-graph traversal in which both adjacency and
      visitation information is implicit in the image being read and written.
      When a flood fill is performed atomically (no other changes to the image
      while the fill is happening), all variations produce the same results.
      But the order in which pixels are found and filled depends on:
    </p>
    <ul>
      <li>
        The data structure used for the <em>fringe</em>, i.e., the vertices
        (pixel locations) that have been discovered but are not yet filled.
        This is most often a stack (last-in, first-out) or ""queue"" (first-in,
        first-out), but any kind of generalized queue will work. See
        <a href=""#basic-actions"">Basic Actions</a>.
      </li>
      <li>
        The order in which neighboring verties (pixel locations) that may need
        to be filled are checked and added to the fringe. This is especially
        significant when a stack is used for the fringe, because with a stack,
        many steps often progress between when a vertex (pixel location) is
        added to the fringe and when it is actually filled. See
        <a href=""#basic-actions"">Neighbor Enumeration Strategies</a>.
      </li>
    </ul>
    <p>
      This is a prototype, which is why fill type and configuration can only be
      affected in shortcut-style ways, and not through menus. In addition to
      fixing that, other accessibility improvements should be made: at minimum,
      it should be no harder to use without a mouse (or limited mousing) than
      popular raster graphics editors.
    </p>
    <h3 id=""basic-actions"">Basic Actions</h3>
    <ul>
      <li>
        Left-click and drag to draw on the canvas.
      </li>
      <li>
        Right-click to flood-fill with a stack (a LIFO queue).
      </li>
      <li>
        Left-click to flood-fill ""queue"" (i.e., a FIFO queue).
      </li>
      <li>
        <kbd>Alt</kbd> + right-click to flood-fill with random extraction
        queue.
      </li>
    </ul>
    <h3 id=""speed-modifiers"">Speed Modifiers</h3>
    <p>
      By default, fills proceeed at a rate of 5 pixels per frame. You can
      override that by pressing:
    </p>
    <ul>
      <li>
        <kbd>Shift</kbd> to fill slowly (1 pixel per frame).
      </li>
      <li>
        <kbd>Ctrl</kbd> to fill very fast (20 pixels per frame).
      </li>
      <li>
        <kbd>Ctrl</kbd>+<kbd>Shift</kbd> to fill pretty fast (10 pixels per
        frame).
      </li>
    </ul>
    <p>
      Modifiers affect the speed of <strong>newly started fills</strong>. That
      way, you can have concurrent fills proceeding at different speeds.
    </p>
    <h3 id=""neighbor-enumeration-strategies"">
      Neighbor Enumeration Strategies
    </h3>
    <p>
      There are several major strategies for the order in which neighbors are
      enumerated, and some (currently, just one) are configurable by the
      selection of sub-strategies.
    </p>
    <p>
      Use the scroll-wheel (with no modifier keys) to cycle between major
      neighbor-enumeration strategies.
    </p>
    <p>
      Hold down <kbd>Shift</kbd> while using the scroll-wheel to cycle
      between minor neighbor-enumeration strategies for the currently selected
      major strategy.
    </p>
    <p>
      <strong>The mouse pointer must be over the canvas when
      scrolling.</strong> This is to avoid accidentally changing the neighbor
      enumeration strategy when attempting to scroll something else (such as
      this help).
    </p>
    <p>
      The available neighbor enumeration strategies (with their substrategies)
      are:
    </p>
    <ul>
      <li>
        <strong>Uniform</strong> - Neighbors are always enumerated in the same
        order. The substrategy determines which order that is. There are 24
        substrategies, since that's how many permutations of left, right, up,
        and down there are. Substrategies are abbreviated by the first letters
        of each direction, in the order in which neighbors are enumerated. The
        default substrategy is <strong>LRUD</strong>.
      </li>
      <li>
        <strong>Random per-fill</strong> - Neighbors are enuemrated in an order
        that is uniform in each fill, but different&mdash;and randomly
        chosen&mdash;each time a fill is peformed. This is thus the same as
        using the <strong>Uniform</strong> strategy an choosing a random
        substrategy immediately before every fill.
      </li>
      <li>
        <strong>Random each time</strong> - Each time neighbors are enumerated,
        even within the same fill, the order is randomly selected. If the same
        pixel is reached multiple times, even in the same fill (which is
        possible in the case of concurrent <em>physically nested</em> fills
        interfering with each other), a different order may be chosen.
      </li>
      <li>
        <strong>Random per pixel</strong> - Each pixel in the image has an
        order in which neighbors are always enumerated, and each order is
        randomly determined separately. If the same pixel is reached multiple
        times, even in separate fills (whether or not they overlap in time),
        the same order is chosen for each pixel. Restarting the program
        generates a different order. Without looking closely, you won't notice
        the difference between this and <strong>Random each time</strong>.
      </li>
    </ul>
  </body>
</html>";

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

var showHideHelp = new Button();

var infoBar = new TableLayoutPanel {
    RowCount = 1,
    ColumnCount = 2,
    GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
    AutoSize = true,
};
infoBar.Controls.Add(status);
infoBar.Controls.Add(showHideHelp);

var help = new WebBrowser {
    Visible = false,
    Size = new Size(width: 600, height: 300),
    DocumentText = helpText,
};

void UpdateStatus()
    => status.Text = $"Neighbor enumeration strategy:"
                   + $" {neighborEnumerationStrategies.Current}";

void UpdateShowHideHelp()
    => showHideHelp.Text = help.Visible ? "Hide Help" : "Show Help";

UpdateStatus();
UpdateShowHideHelp();

var ui = new TableLayoutPanel {
    RowCount = 3,
    ColumnCount = 1,
    GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
    AutoSize = true,
};
ui.Controls.Add(canvas);
ui.Controls.Add(infoBar);
ui.Controls.Add(help);

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

showHideHelp.Click += delegate {
    help.Visible = !help.Visible;
    UpdateShowHideHelp();
};

ui.Dump("Watching Paint Dry");

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

static Func<int, int> CreateRandomGenerator()
    => new Random(RandomNumberGenerator.GetInt32(int.MaxValue)).Next;

static int DecideSpeed()
    => (Control.ModifierKeys & (Keys.Shift | Keys.Control)) switch {
        Keys.Shift => 1,
        Keys.Control => 20,
        Keys.Shift | Keys.Control => 10,
        _ => 5
    };

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
