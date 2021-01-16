<Query Kind="Statements">
  <NuGetReference>LiveCharts.NetCore</NuGetReference>
  <NuGetReference>LiveCharts.WinForms.NetCore3</NuGetReference>
  <Namespace>ColumnSeries = LiveCharts.Wpf.ColumnSeries</Namespace>
  <Namespace>LiveCharts</Namespace>
  <Namespace>LiveCharts.WinForms</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

// Unfortunately, LiveCharts is too slow with thousands of data points,
// even if no points are dynamically added after the chart is rendered.
// (LiveCharts.Geared would probably be perfect, but its licensing
// restrictions would impose another non-free dependency--and people
// making derivative works would have to pay a license fee or apply for
// their own LiveCharts.Geared community edition license.)

var random = new Random();

var values = Enumerable.Range(0, 5000)
                       .Select(_ => random.NextDouble())
                       .ToList();

values.Select((Value, Index) => new { Value, Index })
                          .Chart(datum => datum.Index,
                                 datum => datum.Value,
                                 Util.SeriesType.Column) //
                          .Dump();

var chart = new CartesianChart {
    Series = new SeriesCollection() {
        new ColumnSeries {
            Title = "Random Variable",
            //Values = new ChartValues<double> { 10, 50, 39, 50 },
            Values = new ChartValues<double>(values),
            ColumnPadding = 0,
        },
    },

    //Zoom = ZoomingOptions.Xy,
    DisableAnimations = true,
};

chart.Dump();
