using System.Drawing;
using System.Windows.Forms;

namespace moddingSuite.ZoneEditor.Markers;

internal class VertexMarker : Marker
{
    private readonly MouseEventHandler dragEventHandler;
    public Brush Colour;
    public VertexMarker() : base()
    {
        Colour = Brushes.Blue;
        Location = new System.Drawing.Point(30, 30);
        Name = "pictureBox1";
        Size = new System.Drawing.Size(10, 10);
        //this.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        TabIndex = 1;
        TabStop = false;

        MouseDown += new MouseEventHandler(OnMouseDown);
        MouseUp += new MouseEventHandler(OnMouseUp);
        dragEventHandler = new MouseEventHandler(OnDrag);

    }
    public override void paint(object obj, PaintEventArgs e) => e.Graphics.FillRectangle(Colour, new Rectangle(0, 0, 10, 10));
    public void OnMouseDown(object obj, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        Parent.MouseMove += dragEventHandler;
        MouseMove += dragEventHandler;
        //Console.WriteLine(e.Location);
    }
    public void OnMouseUp(object obj, MouseEventArgs e)
    {
        //this.Parent.DoDragDrop(this, DragDropEffects.Move);
        if (e.Button != MouseButtons.Left)
            return;
        Parent.MouseMove -= dragEventHandler;
        MouseMove -= dragEventHandler;
        //Console.WriteLine(e.Location);
    }
    public void OnDrag(object obj, MouseEventArgs e)
    {

        Point p = e.Location;
        p.Offset((-Size.Width / 2) - 1, (-Size.Height / 2) - 1);
        if (obj.Equals(this))
        {
            p.Offset(Location);
        }
        Parent.Invalidate();
        Location = p;
        position = getPosition();
    }
}
