using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Input.Custom;
using Rhino.Input;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace GanPlanRhino
{
    public static class Doors
    {
        public static void Make3d(string schemeName)
        {
            string inputLayerPath = schemeName + "::EJLT Shapes";
            string outputLayerPath = schemeName + "::Walls";
            string doorsLayerPath = schemeName + "::Doors";

            List<int> layerIndexs;
            List<Curve> curves = LayerHelper.GetCurvesFromChild(inputLayerPath, out layerIndexs);
            List<Curve> doors = LayerHelper.GetCurvesFrom(doorsLayerPath);
            List<LineCurve> afterCut = new List<LineCurve>();

            List<Line> exploded = new List<Line>();
            foreach (Curve c in curves)
            {
                Polyline poly;
                c.TryGetPolyline(out poly);

                exploded.AddRange(poly.GetSegments());
            }

            List<Line> lineSegDoor = new List<Line>();
            List<List<LineCurve>> doorsPerSeg = new List<List<LineCurve>>();
            foreach (Line lineSeg in exploded)
            {
                List<LineCurve> doorsThisSeg = new List<LineCurve>();
                foreach(Curve door in doors)
                {
                    CurveIntersections inter = Intersection.CurveLine(door, lineSeg, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, 0.0);
                    if (inter.Count == 2)
                    {
                        LineCurve l1;

                        Point3d p1 = inter.First().PointA;
                        Point3d p2 = inter[1].PointA;
                        l1 = new LineCurve(p1, p2);
                        doorsThisSeg.Add(l1);
                    }
                }
                if (doorsThisSeg.Count > 0)
                {
                    lineSegDoor.Add(lineSeg);
                    doorsPerSeg.Add(doorsThisSeg);
                } else
                {
                    // no intersection, add to after cut
                    afterCut.Add(new LineCurve(lineSeg));
                }
            }

            for (int i = 0; i < lineSegDoor.Count; i++)
            {
                // points from all intersection points
                List<Point3d> intersectionPts = new List<Point3d>();
                intersectionPts.Add(lineSegDoor[i].From);
                intersectionPts.Add(lineSegDoor[i].To);
                foreach(LineCurve doorLine in doorsPerSeg[i])
                {
                    intersectionPts.Add(doorLine.PointAtStart);
                    intersectionPts.Add(doorLine.PointAtEnd);
                }
                List<Point3d> sortedPoints = intersectionPts.OrderBy(pnt => pnt.Y).ThenBy(pnt => pnt.X).ToList();

                // construct line segments
                for (int pi = 0; pi < sortedPoints.Count ; pi=pi+2)
                {
                    LineCurve cuttedSegment = new LineCurve(sortedPoints[pi], sortedPoints[pi+1]);
                    bool indoor = false;
                    foreach (Curve door in doors)
                    {
                        if (door.Contains(cuttedSegment.PointAt(0.5), Plane.WorldXY, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)==PointContainment.Inside){
                            indoor = true;
                            break;
                        }
                    }
                    if (!indoor) afterCut.Add(cuttedSegment);

                }
            }

            foreach (LineCurve wallLine in afterCut)
            {
                LayerHelper.BakeObjectToLayer(Extrusion.Create(wallLine, 20, false).ToBrep(), "Walls", schemeName);

            }

        }
        public static void PlaceDoorsAt(string schemeName)
        {
            string inputLayerPath = schemeName + "::EJLT Shapes";
            string outputLayerPath = schemeName + "::Doors";

            double doorW = 3;
            double doorH = 0.5;

            // get curves
            List<int> layerIndexs;
            List<Curve> curves = LayerHelper.GetCurvesFromChild(inputLayerPath, out layerIndexs);

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
                            var intersects = Intersection.CurveCurve(c1, c2, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, 0.1);
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
