<Query Kind="Statements">
  <Namespace>LINQPad.Controls</Namespace>
</Query>

static void Swap<T>(T[] items, int i, int j)
    => (items[i], items[j]) = (items[j], items[i]);

static void ReverseBetween<T>(T[] items, int startInclusive, int endExclusive)
    => Array.Reverse(items, startInclusive, endExclusive - startInclusive);

// This is the algorithm described in:
// https://en.wikipedia.org/wiki/Permutation#Generation_in_lexicographic_order
static void CyclePermutation<T>(T[] items, IComparer<T> comparer)
{
    var right = items.Length - 2;
    while (right >= 0 && comparer.Compare(items[right], items[right + 1]) >= 0)
        --right;

    if (right < 0) {
        Array.Reverse(items);
        return;
    }

    var left = items.Length - 1;
    while (comparer.Compare(items[right], items[left]) >= 0) --left;
    Swap(items, right, left);
    ReverseBetween(items, right + 1, items.Length);
}

static void CycleNextPermutation<T>(T[] items)
    => CyclePermutation(items, Comparer<T>.Default);

static void CyclePrevPermutation<T>(T[] items)
    => CyclePermutation(items, ReverseComparer<T>.Default);

var current = Enum.GetValues<Direction>();
var display = new TextBox { Enabled = false };
UpdateDisplay();

var prev = new Button("Prev", delegate {
    CyclePrevPermutation(current);
    UpdateDisplay();
});

var next = new Button("Next", delegate {
    CycleNextPermutation(current);
    UpdateDisplay();
});

void UpdateDisplay() => display.Text = FormatRecord(current);

static string FormatRecord<T>(IEnumerable<T> items) where T : notnull
{
    const int extraPadding = 5;

    static string ToString(T item)
        => item.ToString()
            ?? throw new ArgumentException(
                    paramName: nameof(items),
                    message: "element has null string representation");

    var tokens = items.Select(ToString).ToArray();
    var width = tokens.Max(token => token.Length) + extraPadding;
    var alignedTokens = tokens.Select(token => token.PadRight(width));
    return string.Join(" ", alignedTokens).TrimEnd();
}

var buttons = new StackPanel(horizontal: true, prev, next);
var ui = new StackPanel(horizontal: false, display, buttons);
ui.Dump();

internal enum Direction {
    Left,
    Right,
    Up,
    Down,
}

internal sealed class ReverseComparer<T> : IComparer<T> {
    internal static IComparer<T> Default { get; } =
        new ReverseComparer<T>(Comparer<T>.Default);

    public int Compare(T? lhs, T? rhs) => _comparer.Compare(rhs, lhs);

    internal ReverseComparer(IComparer<T> comparer) => _comparer = comparer;

    private readonly IComparer<T> _comparer;
}
