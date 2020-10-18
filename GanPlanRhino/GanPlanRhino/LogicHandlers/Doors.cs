using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Input.Custom;
using Rhino.Input;
using Rhino.Geometry;

namespace GanPlanRhino
{
    public static class Doors
    {

        public static void PlaceDoorsAt(string schemeName)
        {
            string inputLayerPath = schemeName + "::EJLT Shapes";
            string outputLayerPath = schemeName + "::Doors";

            double doorW = 3;
            double doorH = 0.5;

            // get curves
            List<int> layerIndexs;
            List<Curve> curves = LayerHelper.GetCurvesFrom(inputLayerPath, out layerIndexs);

            // explode curves
            List<Polyline> polies = new List<Polyline>();
            List<Line[]> exploded = new List<Line[]>();
            foreach (Curve c in curves)
            {
                Polyline poly;
                c.TryGetPolyline(out poly);
                polies.Add(poly);

                exploded.Add(poly.GetSegments());
            }

            List<Point3d> centers = new List<Point3d>();
            List<bool> vertical = new List<bool>();
            // find overlapping curves, find center, find direction of lines
            for (int i = 0; i < polies.Count; i++)
            {
                foreach (Line line in exploded[i])
                {
                    for (int j = 0; j < polies.Count; j++)
                    {
                        if (i == j) continue; // skip intersection check with current rectangle
                        foreach (Line otherline in exploded[j])
                        {
                            Curve c1 = line.ToNurbsCurve();
                            Curve c2 = otherline.ToNurbsCurve();
                            // get all intersections
                            var intersects = Rhino.Geometry.Intersect.Intersection.CurveCurve(c1, c2, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, 0.1);
                            if (intersects != null && intersects.Count > 0)
                            {
                                foreach (var inter in intersects)
                                {
                                    if (inter.IsOverlap) // a overlap found
                                    {
                                        double length = inter.OverlapA.Length * c1.GetLength();

                                        if (length > 4) // overlap is long enough
                                        {
                                            Point3d mid = c1.PointAt(inter.OverlapA.Mid);

                                            double mindis = 10000.0;
                                            foreach (Point3d oldpt in centers)
                                            {
                                                double distance = mid.DistanceTo(oldpt);
                                                if (distance < mindis) mindis = distance;
                                            }
                                            if (mindis > 1.0) // no duplicates
                                            {
                                                centers.Add(c1.PointAt(inter.OverlapA.Mid));
                                                if (c1.PointAt(0).X == c1.PointAt(1).X) vertical.Add(true);
                                                else vertical.Add(false);
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }

                }
            }


            // draws rectangle
            for (int i = 0; i < centers.Count; i++)
            {
                if (vertical[i])
                {
                    Point3d corner = new Point3d(centers[i].X - doorH / 2, centers[i].Y - doorW / 2, 0.0);
                    Rectangle3d rec = new Rectangle3d(new Plane(corner, Vector3d.ZAxis), doorH, doorW);
                    LayerHelper.BakeObjectToLayer(rec.ToPolyline().ToPolylineCurve(), "Doors", schemeName);

                }
                else
                {
                    Point3d corner = new Point3d(centers[i].X - doorW / 2, centers[i].Y - doorH / 2, 0.0);
                    Rectangle3d rec = new Rectangle3d(new Plane(corner, Vector3d.ZAxis), doorW, doorH);
                    LayerHelper.BakeObjectToLayer(rec.ToPolyline().ToPolylineCurve(), "Doors", schemeName);
                }
            }

            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();


        }
    }
}
