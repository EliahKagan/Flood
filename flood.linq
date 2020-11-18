<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

// Currently everything interesting is in the prototype, flood-scratch.linq.

internal sealed class FloodWindow : Form {
    internal FloodWindow()
    {
        Text = "Flood Filler";
        Size = new Size(width: 600, height: 600);
        FormBorderStyle = FormBorderStyle.Fixed3D;
        MaximizeBox = false;
        
        //_canvas.Paint += 
        
        Controls.Add(_canvas);
    }
    
    private readonly PictureBox _canvas = new() {
        Location = new Point(x: 20, y: 20),
        Size = new Size(width: 540, height: 520),
        BackColor = Color.Black,
    };
}

internal static class Program {
    [STAThread]
    private static void Main() => Application.Run(new FloodWindow());
}
