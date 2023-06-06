using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.Util;
using moddingSuite.ZoneEditor.Markers;
using moddingSuite.ZoneEditor.ScenarioItems.PropertyPanels;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace moddingSuite.ZoneEditor.ScenarioItems;

public class Icon : ScenarioItem
{
    private readonly VertexMarker position;
    private Image image;
    private IconType _type;
    private int _priority;
    public int priority
    {
        get => _priority;
        set
        {
            _priority = value;
            ((IconProperty)propertypanel).update();
            updateImage();
            position.Parent?.Refresh();
        }
    }
    public IconType type
    {
        get => _type;
        set
        {
            _type = value;
            ((IconProperty)propertypanel).update();
            updateImage();
            position.Parent?.Refresh();
        }
    }
    public Icon(Point p, int i, IconType t, int prio = 1)
    {

        position = new VertexMarker
        {
            Colour = Brushes.Green
        };
        position.setPosition(p);
        propertypanel = new IconProperty(this);
        type = t;
        Name = string.Format("Start Position {0}", i);
        setSelected(false);
        priority = prio;
    }
    public override void attachTo(Control c)
    {
        c.Controls.Add(position);
        c.Paint += paintEvent;
    }
    public override void detachFrom(Control c)
    {
        c.Controls.Remove(position);
        c.Paint -= paintEvent;
    }
    private void updateImage()
    {
        string typeString="";
        switch (type)
        {
            case IconType.CV:
                typeString = "CV.png";
                break;
            case IconType.FOB:
                typeString = "FOB.png";
                break;
        }
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.IO.Stream imgStream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".ZoneEditor.Images." + typeString);
        image = new Bitmap(imgStream);
    }
    protected override void paint(object o, PaintEventArgs e)
    {
        e.Graphics.ResetTransform();
        Point p = PanAndZoom.fromGlobalToLocal(position.getPosition());
        int size=20;

        e.Graphics.TranslateTransform(p.X, p.Y);

        e.Graphics.DrawImage(image, new Rectangle(-size / 2, -size / 2, size, size));
    }
    public override void setSelected(bool selected) => position.Visible = selected;
    public override void buildNdf(NdfBinary data, ref int i)
    {
        string name = type switch
        {
            IconType.CV => "TGameDesignAddOn_StartingCommandUnit",
            IconType.FOB => "TGameDesignAddOn_StartingFOB"
        };
        NdfObject spawnPoint = createNdfObject(data, name);
        NdfPropertyValue allocationProperty = getProperty(spawnPoint, "AllocationPriority");
        //NOT RIGHT
        allocationProperty.Value = new NdfInt32(priority);

        NdfObject designItem = createNdfObject(data, "TGameDesignItem");
        NdfCollection list = data.Classes.First().Instances.First().PropertyValues.First().Value as NdfCollection;
        CollectionItemValueHolder ci = new(new NdfObjectReference(designItem.Class, designItem.Id), data);
        list.Add(ci);

        NdfPropertyValue positionProperty = getProperty(designItem, "Position");
        Point hp = position.getPosition();
        System.Windows.Media.Media3D.Point3D p = Geometry.convertPoint(hp);
        positionProperty.Value = new NdfVector(p);

        NdfPropertyValue rotationProperty = getProperty(designItem, "Rotation");

        rotationProperty.Value = new NdfSingle(0f);

        NdfPropertyValue addOnProperty = getProperty(designItem, "AddOn");
        addOnProperty.Value = new NdfObjectReference(spawnPoint.Class, spawnPoint.Id);
    }
}
