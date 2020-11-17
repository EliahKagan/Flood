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
    //if (args.Button == MouseButtons.Left) {
    //    bmp.SetPixel(args.Location.X, args.Location.Y, Color.Black);
    //}

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
        await FloodFillAsync(args.Location, Color.Black);
        break;
    }
};

async Task FloodFillAsync(Point start, Color toColor)
{
    const int speedup = 10;
    var count = 0;
    
    var stack = new Stack<Point>();
    stack.Push(start);
    
    while (stack.Count != 0) {
        var src = stack.Pop();

        if (!rectangle.Contains(src)
                || bmp.GetPixel(src.X, src.Y).ToArgb() == toColor.ToArgb())
            continue;
        
        if (count++ % speedup == 0) await Task.Delay(1);
        
        bmp.SetPixel(src.X, src.Y, toColor);
        canvas.Invalidate();
        bmp.GetPixel(src.X, src.Y).Dump("filled color");
        //Debug.Assert(bmp.GetPixel(src.X, src.Y).ToArgb() == toColor.ToArgb());

        stack.Push(new(src.X - 1, src.Y));
        stack.Push(new(src.X + 1, src.Y));
        stack.Push(new(src.X, src.Y - 1));
        stack.Push(new(src.X, src.Y + 1));
    }
    
    //canvas.Invalidate();
}

canvas.Dump();
