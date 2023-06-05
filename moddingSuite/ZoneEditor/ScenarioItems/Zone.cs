using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.Model.Scenario;
using moddingSuite.ZoneEditor.Markers;
using moddingSuite.ZoneEditor.ScenarioItems.PropertyPanels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZoneEditor;
namespace moddingSuite.ZoneEditor.ScenarioItems;

public class Zone : ScenarioItem
{
    VertexMarker attachPoint;
    Outline outline;

    Area area;
    //private string name;
    private Possession _possession;
    private int _value;
    public int value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            ((ZoneProperty)propertypanel).update();
            editor.Refresh();
        }
    }

    public Possession possession
    {
        get
        {
            return _possession;
        }
        set
        {
            _possession = value;
            outline.possession = _possession;
            ((ZoneProperty)propertypanel).update();
            editor.Refresh();
        }
    }
    Editor editor;
    public Zone(Editor e, Point p, int index)
    {
        editor = e;
        attachPoint = new VertexMarker();
        attachPoint.setPosition(PanAndZoom.fromLocalToGlobal(p));
        attachPoint.Colour = System.Drawing.Brushes.Green;
        outline = new Outline(p);
        propertypanel = new ZoneProperty(this);

        possession = Possession.Neutral;
        Name = string.Format("Zone {0}", index);
        setSelected(false);
    }
    public Zone(Editor e, Area a)
    {
        //name = a.Name;
        editor = e;
        area = a;
        attachPoint = new VertexMarker();
        attachPoint.setPosition(Geometry.Geometry.convertPoint(a.AttachmentPoint));
        attachPoint.Colour = System.Drawing.Brushes.Green;
        outline = new Outline(Geometry.Geometry.getOutline(a.Content));
        propertypanel = new ZoneProperty(this);

        possession = Possession.Neutral;
        Name = string.Format("Zone {0}", a.Id);
        setSelected(false);



    }
    public override void detachFrom(Control c)
    {
        c.Paint -= paintEvent;
        c.Controls.Remove(attachPoint);
        outline.detachFrom(c);

    }
    public override void attachTo(Control c)
    {
        c.Paint += new PaintEventHandler(paint);
        c.Controls.Add(attachPoint);
        outline.attachTo(c);

    }
    public override void setSelected(bool selected)
    {
        attachPoint.Visible = selected;
        outline.setSelected(selected);
    }
    protected override void paint(object sen, PaintEventArgs e)
    {
        PanAndZoom.Transform(e);
        Font font = new(System.Drawing.FontFamily.GenericSansSerif, 16);
        Point namePos = attachPoint.getPosition();
        namePos.Offset(-(int)(10 * Name.Length) / 2, -font.Height);
        e.Graphics.DrawString(Name, font, System.Drawing.Brushes.White, namePos);
        Point valPos = attachPoint.getPosition();
        string valString=string.Format("{0}",value);
        valPos.Offset(-(int)(10 * valString.Length) / 2, 0);
        e.Graphics.DrawString(valString, font, System.Drawing.Brushes.White, valPos);
    }
    public List<AreaVertex> getRawOutline()
    {
        if (area != null)
            return area.Content.Vertices.GetRange(area.Content.BorderVertex.StartVertex, area.Content.BorderVertex.VertexCount);
        else
            return new List<AreaVertex>();
    }
    public Area getArea()
    {
        Area area = new()
        {
            AttachmentPoint = Geometry.Geometry.convertPoint(attachPoint.getPosition()),
            Content = Geometry.Geometry.getFromOutline(outline.getOutline()),
            Name = string.Format("zone_{0}", Guid.NewGuid().ToString())
        };
        return area;
    }
    public override void buildNdf(NdfBinary data, ref int i)
    {

        NdfObject commandPoint = createNdfObject(data, "TGameDesignAddOn_CommandPoints");
        /*var nameProperty = getProperty(commandPoint, "Name");
        nameProperty.Value = getAutoName(data, i++);
        var rankingProperty = getProperty(commandPoint, "Ranking");
        rankingProperty.Value = getString(data, "CommandPoints");
        var guidProperty = getProperty(commandPoint, "GUID");
        rankingProperty.Value = new NdfGuid(Guid.NewGuid());*/
        NdfPropertyValue pointsProperty = getProperty(commandPoint, "Points");
        pointsProperty.Value = new NdfInt32(value);

        NdfObject designItem = createNdfObject(data, "TGameDesignItem");
        NdfCollection list = data.Classes.First().Instances.First().PropertyValues.First().Value as NdfCollection;
        CollectionItemValueHolder ci=new(new NdfObjectReference(designItem.Class,designItem.Id),data);
        list.Add(ci);

        NdfPropertyValue positionProperty=getProperty(designItem,"Position");
        System.Windows.Media.Media3D.Point3D p = Geometry.Geometry.convertPoint(attachPoint.getPosition());
        positionProperty.Value = new NdfVector(p);

        NdfPropertyValue addOnProperty = getProperty(designItem, "AddOn");
        addOnProperty.Value = new NdfObjectReference(commandPoint.Class, commandPoint.Id);

        if (possession != Possession.Neutral)
        {
            NdfObject startPoint = createNdfObject(data, "TGameDesignAddOn_StartingPoint");

            /*var nameProperty = getProperty(startPoint, "Name");
            nameProperty.Value = getAutoName(data, i++);

            var rankingProperty = getProperty(startPoint, "Ranking");
            rankingProperty.Value = getString(data, "StartingPoints");
            var guidProperty = getProperty(startPoint, "GUID");
            guidProperty.Value = new NdfGuid(Guid.NewGuid());*/
            if ((int)possession == 1)
            {
                NdfPropertyValue allianceProperty = getProperty(startPoint, "AllianceNum");
                allianceProperty.Value = new NdfInt32(1);

                allianceProperty = getProperty(startPoint, "WarmupCamPath");
                allianceProperty.Value = getString(data, "Camp2");
            }
            else
            {
                NdfPropertyValue allianceProperty = getProperty(startPoint, "WarmupCamPath");
                allianceProperty.Value = getString(data, "Camp1");
            }

            designItem = createNdfObject(data, "TGameDesignItem");
            ci = new CollectionItemValueHolder(new NdfObjectReference(designItem.Class, designItem.Id), data);
            list.Add(ci);


            positionProperty = getProperty(designItem, "Position");
            p = Geometry.Geometry.convertPoint(attachPoint.getPosition());
            positionProperty.Value = new NdfVector(p);
            NdfPropertyValue rotationProperty = getProperty(designItem, "Rotation");
            rotationProperty.Value = new NdfSingle(0f);

            addOnProperty = getProperty(designItem, "AddOn");
            addOnProperty.Value = new NdfObjectReference(startPoint.Class, startPoint.Id);

        }
        //item.Value = NdfTypeManager.GetValue(new byte[NdfTypeManager.SizeofType(type)], type, item.Manager);
        //.Object.Manager.CreateInstanceOf
    }
}
