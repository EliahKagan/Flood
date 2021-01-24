<Query Kind="Statements" />

// Can opening the Start Menu be prevented when the Windows key is released, by
// sending a Shift keypress programmatically? (That works from the keyboard.)

using System.Drawing;
using System.Windows.Forms;
using Keyboard = System.Windows.Input.Keyboard;
using Key = System.Windows.Input.Key;

new MainPanel().Dump();

internal sealed class MainPanel : Panel {
    internal MainPanel()
    {
        _canvas.MouseDown += canvas_MouseDown;
        Controls.Add(_canvas);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RefreshCanvas();
    }

    protected override void WndProc(ref Message m)
    {
        static bool HasSuperKey(in Message msg)
            => (VK)msg.WParam is VK.LWIN or VK.RWIN;

        // No need to also check for SYSKEYUP, because releasing a Windows
        // key while Alt is (still) pressed doesn't open the Start Menu.
        if ((WM)m.Msg is WM.KEYUP && HasSuperKey(m) && _suppressStartMenu) {
            _suppressStartMenu = false;
            if (Focused) SendKeys.Send("+()");
        }

        base.WndProc(ref m);
    }

    private enum WM : uint {
        KEYUP      = 0x0101,
    }

    private enum VK : ulong {
        LWIN = 0x5B,
        RWIN = 0x5C,
    }

    private void canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        Focus();
        if (GotKey.Super) _suppressStartMenu = true;
    }

    private void RefreshCanvas()
    {
        _canvas.Location = ClientRectangle.Location;
        _canvas.Size = ClientRectangle.Size;
    }

    private readonly Canvas _canvas = new();

    private bool _suppressStartMenu = false;
}

internal sealed class Canvas : PictureBox {
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        CreateGraphics().FillRectangle(Brushes.DarkRed, ClientRectangle);
    }
}

internal static class GotKey {
    internal static bool Super
        => Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);
}
