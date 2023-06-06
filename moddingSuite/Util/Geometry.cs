using moddingSuite.Model.Common;
using moddingSuite.Model.Scenario;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Media3D;

namespace moddingSuite.Util;

public static class Geometry
{
    public const int scaleFactor = 1280;
    public const double groundLevel = 32946.4921875;
    public static List<Point> getOutline(AreaContent content)
    {

        List<AreaVertex> outline =content.Vertices.GetRange(content.BorderVertex.StartVertex,content.BorderVertex.VertexCount);
        return outline.ConvertAll(x => new Point((int)x.X / scaleFactor, (int)x.Y / scaleFactor));
    }
    public static AreaContent getFromOutline(List<AreaVertex> outline)
    {

        AreaContent content = new();

        if (!isRightHanded(outline))
            outline.Reverse();
        outline = outline.Select(x => { x.Center = 1; return x; }).ToList();
        content.Vertices.AddRange(outline);

        //var bottomRing = new List<AreaVertex>(outline);
        List<AreaVertex> bottomRing = outline.Select(x =>
        {
            AreaVertex y = new()
            {
                X = x.X,
                Y = x.Y,
                Z = x.Z,
                W = x.W,
                Center = 0
            };
            return y;
        }).ToList();
        content.Vertices.AddRange(bottomRing);

        List<AreaVertex> innerRing = getInnerRing(outline);
        content.Vertices.AddRange(innerRing);

        List<MeshTriangularFace> triangles = getTriangles(outline, 0, outline.Count - 1);
        content.Triangles.AddRange(triangles);
        List<MeshTriangularFace> borderTriangles = getBorderTriangles(outline.Count);
        content.Triangles.AddRange(borderTriangles);

        AreaClipped ac = new()
        {
            StartTriangle = 0,
            StartVertex = 0,
            TriangleCount = triangles.Count,
            VertexCount = outline.Count
        };
        //ac.TriangleCount = content.Triangles.Count;
        //ac.VertexCount = content.Vertices.Count;
        content.ClippedAreas.Add(ac);
        AreaClipped ac2 = new();
        content.ClippedAreas.Add(ac2);

        AreaClipped bt = new()
        {
            StartTriangle = triangles.Count,
            StartVertex = outline.Count,
            TriangleCount = borderTriangles.Count,
            VertexCount = 2 * outline.Count
        };
        content.BorderTriangle = bt;

        AreaClipped bv = new()
        {
            StartTriangle = 0,
            StartVertex = 2 * outline.Count,
            TriangleCount = 0,
            VertexCount = outline.Count
        };
        content.BorderVertex = bv;

        //Console.Write("vertices=[");
        //var scen = content;
        /*foreach (var v in scen.Vertices)
        {
            Console.WriteLine("{0:G},{1:G},{2:G};", (int)v.X, (int)v.Y, (int)v.Center);
        }
        Console.WriteLine("]");

        Console.Write("tri=[");
        foreach (var v in scen.Triangles)
        {
            Console.WriteLine("{0},{1},{2};", (int)v.Point1, (int)v.Point2, (int)v.Point3);
        }
        Console.WriteLine("]");


        Console.WriteLine("bt=[{0:G},{1:G},{2:G},{3:G}]", scen.BorderTriangle.StartTriangle, scen.BorderTriangle.StartVertex, scen.BorderTriangle.TriangleCount, scen.BorderTriangle.VertexCount);
        Console.WriteLine("bv=[{0:G},{1:G},{2:G},{3:G}]", scen.BorderVertex.StartTriangle, scen.BorderVertex.StartVertex, scen.BorderVertex.TriangleCount, scen.BorderVertex.VertexCount);
        var k = 0;
        foreach (var a in scen.ClippedAreas)
        {
            Console.WriteLine("t{4}=[{0:G},{1:G},{2:G},{3:G}]", a.StartTriangle, a.StartVertex, a.TriangleCount, a.VertexCount, k++);
        }
        */
        return content;

    }
    public static Point convertPoint(Point3D p) => new((int)p.X / scaleFactor, (int)p.Y / scaleFactor);
    public static Point3D convertPoint(Point p) => new(p.X * scaleFactor, p.Y * scaleFactor, groundLevel);
    public static bool isInside(AreaVertex v1, List<AreaVertex> outline)
    {
        AreaVertex v2 = new();

        int crossings = 0;
        for (int i = 0; i < outline.Count; i++)
        {
            AreaVertex b1 = outline.ElementAt(i);
            int k = i + 1;
            if (i == outline.Count - 1)
                k = 0;
            AreaVertex b2 = outline.ElementAt(k);
            if (v1.Equals(b1) || v1.Equals(b2) || v2.Equals(b1) || v2.Equals(b2))
                continue;
            if (isRightHanded(new List<AreaVertex>() { v1, b1, b2 }) == isRightHanded(new List<AreaVertex>() { v2, b1, b2 }))
            {
                continue;
            }
            else if (isRightHanded(new List<AreaVertex>() { v1, v2, b1 }) == isRightHanded(new List<AreaVertex>() { v1, v2, b2 }))
            {
                continue;
            }
            else
            {
                crossings++;
            }
        }
        return crossings % 2 == 1;
    }
    private static List<AreaVertex> getInnerRing(List<AreaVertex> outline)
    {
        List<AreaVertex> innerRing = outline.Select(x =>
        {
            AreaVertex y = new()
            {
                X = x.X,
                Y = x.Y,
                Z = x.Z,
                W = x.W,
                Center = x.Center
            };
            return y;
        }).ToList();
        for (int i = 0; i < outline.Count; i++)
        {
            int im = i - 1;
            if (im < 0)
                im = outline.Count - 1;
            int ip = i + 1;
            if (ip == outline.Count)
                ip = 0;
            double localAngle = (getAngle(outline.ElementAt(im), outline.ElementAt(i), outline.ElementAt(ip)) + Math.PI) / 2;
            double angle=localAngle+ getAbsoluteAngle(outline.ElementAt(im), outline.ElementAt(i));
            float offset = 5 * scaleFactor / (float)Math.Abs(Math.Sin(localAngle));

            AreaVertex tmp = innerRing[i];
            tmp.X += offset * (float)Math.Cos(angle);
            tmp.Y += offset * (float)Math.Sin(angle);
            innerRing[i] = tmp;
        }
        return innerRing;
    }
    private static List<MeshTriangularFace> getBorderTriangles(int count)
    {
        int lower = count;
        int forwardLower = count+1;
        int upper = 2*count;
        int forwardUpper = (2 * count) + 1;
        List<MeshTriangularFace> triangles = new();
        for (int i = 0; i < count - 1; i++)
        {
            /*
            var triangle1 = new MeshTriangularFace(lower + i, forwardLower + i, upper + i);
            var triangle2 = new MeshTriangularFace(upper + i, forwardLower + i, forwardUpper + i);
          */

            MeshTriangularFace triangle1 = new(lower + i, upper + i, forwardLower + i);
            MeshTriangularFace triangle2 = new(upper + i, forwardUpper + i, forwardLower + i);
            triangles.Add(triangle1);
            triangles.Add(triangle2);
        }
        MeshTriangularFace triangle3 = new((2*count)-1, lower, (3*count)-1);
        MeshTriangularFace triangle4 = new((3 * count) - 1, lower, upper);
        /*
        var triangle3 = new MeshTriangularFace(2 * count - 1, 3 * count - 1, lower);
        var triangle4 = new MeshTriangularFace(3 * count - 1, upper, lower);*/
        triangles.Add(triangle3);
        triangles.Add(triangle4);
        return triangles;

    }
    private static List<MeshTriangularFace> getTriangles(List<AreaVertex> outline, int start, int stop)
    {
        List<MeshTriangularFace> triangles = new();
        int i = start;
        AreaVertex firstNode = outline.ElementAt(start);
        int secondIndex = ++i;

        while (i < stop)
        {
            int thirdIndex = ++i;

            AreaVertex secondNode = outline.ElementAt(secondIndex);
            AreaVertex thirdNode=outline.ElementAt(thirdIndex);
            //var t=thirdIndex;
            //bool allRightHanded = true;
            /*while (t < stop)
            {
                var tNode = outline.ElementAt(t++);

                if (!isRightHanded(new List<AreaVertex>() { firstNode, secondNode, tNode }))
                {
                    allRightHanded = false;
                    break;
                }
            }
            if (!allRightHanded) continue;*/
            if (!isRightHanded(new List<AreaVertex>() { firstNode, secondNode, thirdNode }) ||
                lineIntersect(firstNode, thirdNode, outline) ||
                lineIntersect(secondNode, thirdNode, outline))
            {
                continue;
            }
            MeshTriangularFace triangle;
            triangle = new MeshTriangularFace(start, secondIndex, thirdIndex);
            triangles.AddRange(getTriangles(outline, secondIndex, thirdIndex));
            secondIndex = thirdIndex;
            triangles.Add(triangle);

        }
        return triangles;

    }
    private static bool lineIntersect(AreaVertex v1, AreaVertex v2, List<AreaVertex> outline)
    {
        for (int i = 0; i < outline.Count; i++)
        {
            AreaVertex b1 = outline.ElementAt(i);
            int k=i+1;
            if (i == outline.Count - 1)
                k = 0;
            AreaVertex b2 = outline.ElementAt(k);
            if (v1.Equals(b1) || v1.Equals(b2) || v2.Equals(b1) || v2.Equals(b2))
                continue;
            if (isRightHanded(new List<AreaVertex>() { v1, b1, b2 }) == isRightHanded(new List<AreaVertex>() { v2, b1, b2 }))
            {
                continue;
            }
            else if (isRightHanded(new List<AreaVertex>() { v1, v2, b1 }) == isRightHanded(new List<AreaVertex>() { v1, v2, b2 }))
            {
                continue;
            }
            else
            {
                return true;
            }
        }
        return false;
    }
    private static bool isRightHanded(List<AreaVertex> polygon)
    {
        double angleSum = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            int im = i - 1;
            if (im < 0)
                im = polygon.Count - 1;
            int ip = i + 1;
            if (ip == polygon.Count)
                ip = 0;
            angleSum += getAngle(polygon.ElementAt(im), polygon.ElementAt(i), polygon.ElementAt(ip));
        }
        //Console.WriteLine(angleSum);
        return angleSum > 0;
    }
    private static double getAbsoluteAngle(AreaVertex v1, AreaVertex v2)
    {
        double angle = Math.Atan2(v2.Y - v1.Y, v2.X - v1.X);
        return angle;
    }
    private static double getAngle(AreaVertex v0, AreaVertex v1, AreaVertex v2)
    {
        double angle1 = getAbsoluteAngle(v0, v1);
        double angle2 = getAbsoluteAngle(v1, v2);
        double deltaAngle=angle2-angle1;
        if (deltaAngle < -Math.PI)
            deltaAngle += 2 * Math.PI;
        if (deltaAngle > Math.PI)
            deltaAngle -= 2 * Math.PI;
        return deltaAngle;
    }
}
