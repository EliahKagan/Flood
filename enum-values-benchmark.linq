<Query Kind="Statements" />

//internal static class Cache {
//    static ReadOnlySpan<Direction> Directions = (Direction[])Enum.GetValues<Direction>();
//}

const int testCount = 10;
const int repCount = 1_000_000;

static void TestWithoutCaching()
{
    var max = default(Direction?);
    var stopwatch = Stopwatch.StartNew();
    for (var i = 0; i < repCount; ++i)
        max = Enum.GetValues<Direction>().Max();
    stopwatch.Stop();
    new { max, ms = stopwatch.ElapsedMilliseconds }.Dump("without caching");
}

static void TestWithCaching()
{
    var max = default(Direction?);
    var stopwatch = Stopwatch.StartNew();
    for (var i = 0; i < repCount; ++i)
        max = FastEnumInfo<Direction>.GetValues().Max();
    stopwatch.Stop();
    new { max, ms = stopwatch.ElapsedMilliseconds }.Dump("with caching");
}

static void TestCheater()
{
    var max = default(Direction?);
    var stopwatch = Stopwatch.StartNew();
    for (var i = 0; i < repCount; ++i)
        max = Cheater.GetValues().Max();
    stopwatch.Stop();
    new { max, ms = stopwatch.ElapsedMilliseconds }.Dump("cheater");
}

for (var i = 0; i < testCount; ++i) {
    TestWithoutCaching();
    TestWithCaching();
    TestCheater();
}

internal enum Direction {
    Left,
    Right,
    Up,
    Down,
}

internal static class FastEnumInfo<T> where T : struct, Enum {
    internal static T[] GetValues() => _values[..];

    private static readonly T[] _values = Enum.GetValues<T>();
}

internal static class Cheater {
    internal static Direction[] GetValues() => new[] {
        Direction.Left,
        Direction.Right,
        Direction.Up,
        Direction.Down,
    };
}