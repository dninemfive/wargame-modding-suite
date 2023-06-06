using System.Windows.Media.Media3D;

namespace moddingSuite.Model.Scenario;

public class Area
{
    private Point3D _attachmentPoint;

    public string Name { get; set; }

    public int Id { get; set; }

    public Point3D AttachmentPoint
    {
        get => _attachmentPoint;
        set => _attachmentPoint = value;
    }

    public AreaContent Content { get; set; }
}
