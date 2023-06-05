using moddingSuite.Model.Common;
using System.Collections.Generic;

namespace moddingSuite.Model.Scenario;

public class AreaContent
{
    private List<AreaClipped> _clippedAreas  = new();
    private AreaClipped _borderTriangle = new();
    private AreaClipped _borderVertex = new();

    private List<AreaVertex> _vertices = new();
    private List<MeshTriangularFace> _triangles = new();

    public List<AreaClipped> ClippedAreas
    {
        get { return _clippedAreas; }
        set { _clippedAreas = value; }
    }

    public AreaClipped BorderTriangle
    {
        get { return _borderTriangle; }
        set { _borderTriangle = value; }
    }

    public AreaClipped BorderVertex
    {
        get { return _borderVertex; }
        set { _borderVertex = value; }
    }

    public List<AreaVertex> Vertices
    {
        get { return _vertices; }
        set { _vertices = value; }
    }

    public List<MeshTriangularFace> Triangles
    {
        get { return _triangles; }
        set { _triangles = value; }
    }
}
