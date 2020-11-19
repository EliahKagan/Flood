<Query Kind="Statements" />

static void Swap<T>(T[] items, int i, int j)
    => (items[i], items[j]) = (items[j], items[i]);

static void ReverseBetween<T>(T[] items, int startInclusive, int endExclusive)
    => Array.Reverse(items, startInclusive, endExclusive - startInclusive);

// This is the algorithm described in:
// https://en.wikipedia.org/wiki/Permutation#Generation_in_lexicographic_order
static void CycleNextPermutation(int[] items)
{
    var right = items.Length - 2;
    while (right >= 0 && items[right] >= items[right + 1]) --right;
    
    if (right < 0) {
        Array.Reverse(items);
        return;
    }
    
    var left = items.Length - 1;
    while (items[right] >= items[left]) --left;
    Swap(items, right, left);
    ReverseBetween(items, right + 1, items.Length);
}

var a = new[] { 11, 22, 33, 44 };
a.Dump();
for (var count = 30; count > 0; --count) {
    CycleNextPermutation(a);
    string.Join(", ", a).Dump();
}
