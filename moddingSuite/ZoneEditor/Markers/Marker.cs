﻿using System.Drawing;
using System.Windows.Forms;
using ZoneEditor;

namespace moddingSuite.ZoneEditor.Markers;

abstract class Marker : Control
{
    protected Point position;
    public Marker()
    {
        this.Paint += new PaintEventHandler(paint);
        this.Location = new System.Drawing.Point(30, 30);
        this.Name = "pictureBox1";
        //this.Size = new System.Drawing.Size(10, 10);
        //this.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.TabIndex = 1;
        this.TabStop = false;

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
        loc.Offset(Size.Width / 2 + 1, Size.Height / 2 + 1);
        return PanAndZoom.fromLocalToGlobal(loc);
    }
    internal void UpdateMarker()
    {
        Point loc = PanAndZoom.fromGlobalToLocal(position);
        loc.Offset(-Size.Width / 2, -Size.Height / 2);
        Location = loc;

    }
}
