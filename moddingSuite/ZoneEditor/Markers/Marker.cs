using System.Drawing;
using System.Windows.Forms;

namespace moddingSuite.ZoneEditor.Markers;

internal abstract class Marker : Control
{
    protected Point position;
    public Marker()
    {
        Paint += new PaintEventHandler(paint);
        Location = new System.Drawing.Point(30, 30);
        Name = "pictureBox1";
        //this.Size = new System.Drawing.Size(10, 10);
        //this.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        TabIndex = 1;
        TabStop = false;

        //this.MouseDown += new MouseEventHandler(OnMouseDown);
        //this.MouseUp += new MouseEventHandler(OnMouseUp);
        //dragEventHandler = new MouseEventHandler(OnDrag);

    }
    public abstract void paint(object obj, PaintEventArgs e);

    public void setPosition(Point point)
    {

        position = point;
        UpdateMarker();
    }
    public Point getPosition()
    {

        Point loc = Location;
        loc.Offset((Size.Width / 2) + 1, (Size.Height / 2) + 1);
        return PanAndZoom.fromLocalToGlobal(loc);
    }
    internal void UpdateMarker()
    {
        Point loc = PanAndZoom.fromGlobalToLocal(position);
        loc.Offset(-Size.Width / 2, -Size.Height / 2);
        Location = loc;

    }
}
