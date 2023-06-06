using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.Util;
using moddingSuite.ZoneEditor.Markers;
using moddingSuite.ZoneEditor.ScenarioItems.PropertyPanels;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace moddingSuite.ZoneEditor.ScenarioItems;

public class Spawn : ScenarioItem
{
    private readonly VertexMarker head;
    private readonly VertexMarker source;
    private int arrowLength = 250000;
    private readonly int arrowHeadLength = 1500;
    private SpawnType _type;
    public SpawnType type
    {
        get => _type;
        set
        {
            _type = value;

            try
            { head.Parent.Refresh(); }
            catch (NullReferenceException) { }
            arrowLength = _type == SpawnType.Sea ? 350000 : 250000;
            ((SpawnProperty)propertypanel).update();
        }
    }
    public Spawn(Point p, int i, SpawnType t) : this(p, 0, 1f, i, t)
    {
    }
    public Spawn(Point p, float rot, float scale, int i, SpawnType t)
    {
        //if (scale == null) scale = 1;
        propertypanel = new SpawnProperty(this);
        type = t;
        head = new VertexMarker
        {
            Colour = Brushes.Yellow
        };
        head.setPosition(p);

        source = new VertexMarker
        {
            Colour = Brushes.Yellow
        };
        p.Offset(-(int)(scale * arrowLength * Math.Cos(rot) / Geometry.scaleFactor), -(int)(scale * arrowLength * Math.Sin(rot) / Geometry.scaleFactor));
        source.setPosition(p);
        Name = string.Format("Spawn {0}", i);

        setSelected(false);

    }
    public override void attachTo(System.Windows.Forms.Control c)
    {
        c.Controls.Add(head);
        c.Controls.Add(source);
        c.Paint += paintEvent;
    }
    public override void detachFrom(System.Windows.Forms.Control c)
    {
        c.Controls.Remove(head);
        c.Controls.Remove(source);
        c.Paint -= paintEvent;
    }
    protected override void paint(object sen, PaintEventArgs e)
    {
        PanAndZoom.Transform(e);
        _ = new Pen(Brushes.White, 10);
        int width = 5;
        Brush b=Brushes.White;
        switch (type)
        {
            case SpawnType.Land:
                width = 10;

                break;
            case SpawnType.Air:
                b = Brushes.Blue;

                break;
            case SpawnType.Sea:

                break;
        }
        Pen p = new(b, width);
        Point ah = head.getPosition();
        Point ahBase = ah;
        float rot = getRotation();
        ahBase.Offset(-(int)(width * arrowHeadLength * Math.Cos(rot) / Geometry.scaleFactor), -(int)(width * arrowHeadLength * Math.Sin(rot) / Geometry.scaleFactor));
        Point ahLeft = ahBase;
        rot += (float)Math.PI / 2;
        ahLeft.Offset(-(int)(width * arrowHeadLength * Math.Cos(rot) / Geometry.scaleFactor), -(int)(width * arrowHeadLength * Math.Sin(rot) / Geometry.scaleFactor));
        Point ahRight = ahBase;
        rot -= (float)Math.PI;
        ahRight.Offset(-(int)(width * arrowHeadLength * Math.Cos(rot) / Geometry.scaleFactor), -(int)(width * arrowHeadLength * Math.Sin(rot) / Geometry.scaleFactor));
        rot += (float)Math.PI / 2;
        ahBase.Offset((int)(500 * width * Math.Cos(rot) / Geometry.scaleFactor), (int)(500 * width * Math.Sin(rot) / Geometry.scaleFactor));
        e.Graphics.DrawLine(p, source.getPosition(), ahBase);
        e.Graphics.FillPolygon(b, new Point[] { ah, ahLeft, ahRight });
    }
    public override void setSelected(bool selected)
    {
        head.Visible = selected;
        source.Visible = selected;
    }
    public override void buildNdf(NdfBinary data, ref int i)
    {
        string name = "";
        switch (type)
        {
            case SpawnType.Land:
                name = "TGameDesignAddOn_ReinforcementLocation";

                break;
            case SpawnType.Sea:
                name = "TGameDesignAddOn_MaritimeCorridor";
                break;
            case SpawnType.Air:
                name = "TGameDesignAddOn_AerialCorridor";
                break;

        }
        NdfObject spawnPoint = createNdfObject(data, name);
        /*var nameProperty = getProperty(spawnPoint, "Name");
        nameProperty.Value = getAutoName(data, i++);
        var rankingProperty = getProperty(spawnPoint, "Ranking");
        rankingProperty.Value = getString(data, ranking);
        var guidProperty = getProperty(spawnPoint, "GUID");
        rankingProperty.Value = new NdfGuid(Guid.NewGuid());
        var allianceProperty = getProperty(spawnPoint, "NumAlliance");
        allianceProperty.Value = new NdfInt32(1);*/

        NdfObject designItem = createNdfObject(data, "TGameDesignItem");
        NdfCollection list = data.Classes.First().Instances.First().PropertyValues.First().Value as NdfCollection;
        CollectionItemValueHolder ci = new(new NdfObjectReference(designItem.Class, designItem.Id), data);
        list.Add(ci);

        NdfPropertyValue positionProperty = getProperty(designItem, "Position");
        Point hp=head.getPosition();
        System.Windows.Media.Media3D.Point3D p = Geometry.convertPoint(hp);
        positionProperty.Value = new NdfVector(p);

        NdfPropertyValue rotationProperty = getProperty(designItem, "Rotation");
        Point sp = source.getPosition();
        double rot = Math.Atan2(hp.Y - sp.Y, hp.X - sp.X);
        rotationProperty.Value = new NdfSingle((float)rot);

        NdfPropertyValue scaleProperty = getProperty(designItem, "Scale");
        double length = Math.Sqrt(((hp.Y - sp.Y) * (hp.Y - sp.Y)) + ((hp.X - sp.X) * (hp.X - sp.X)));
        float xScale = (float)(length * Geometry.scaleFactor) / arrowLength;
        scaleProperty.Value = new NdfVector(new System.Windows.Media.Media3D.Point3D(xScale, 1, 1));

        NdfPropertyValue addOnProperty = getProperty(designItem, "AddOn");
        addOnProperty.Value = new NdfObjectReference(spawnPoint.Class, spawnPoint.Id);
    }
    public float getRotation()
    {
        Point hp = head.getPosition();
        Point sp = source.getPosition();
        return (float)Math.Atan2(hp.Y - sp.Y, hp.X - sp.X);
    }
}
