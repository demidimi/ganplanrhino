using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace GanPlanRhino
{
    public static class AreaCalc
    {
        public static double[] AreaOf(List<Curve> curves)
        {
            double[] areas = new double[curves.Count()];
            for (int i = 0; i < curves.Count(); i++)
            {
                areas[i] = Math.Round(AreaMassProperties.Compute(curves[i]).Area, 2);
            }
            return areas;
        }

        public static string UpdateArea (List<Curve> curves, List<int> layerIndexs)
        {
            double[] areas = AreaOf(curves);
            List<string> lines = new List<string>();
            if (curves.Count != layerIndexs.Count) return "not same number of names and curves";
            for (int i = 0; i < curves.Count; i++)
            {
                string name = RhinoDoc.ActiveDoc.Layers[layerIndexs[i]].Name;
                lines.Add(name + ": " + areas[i] + " sqft");
            }

            return String.Join("\r\n", lines);
        }

        public static void AddTextLabel(List<Curve> curves, List<int> layerIndexs)
        {
            // remote all old text
            foreach(Rhino.DocObjects.RhinoObject rhob in RhinoDoc.ActiveDoc.Objects)
            {
                if (rhob.ObjectType == Rhino.DocObjects.ObjectType.Annotation)
                {
                    RhinoDoc.ActiveDoc.Objects.Delete(rhob);
                }
            }
            List<Point3d> pts = new List<Point3d>();
            // write new text
            for (int i = 0; i < curves.Count; i++)
            {
                string name = RhinoDoc.ActiveDoc.Layers[layerIndexs[i]].Name;

                pts.Add(AddTextLabel(curves[i], name, pts));
            }
        }
        public static Point3d AddTextLabel (Curve curve, string text, List<Point3d> oldPts)
        {
            double param;
            curve.ClosestPoint(new Point3d(-1000, -1000, 0), out param);
            Point3d pt = curve.PointAt(param);
            foreach (Point3d point in oldPts)
            {
                if (pt.DistanceTo(point) < 10)
                {
                    pt = pt + new Point3d(0, 5, 0);
                }
            }
            AddTextLabel(pt, text);
            return pt;
        }

        public static void AddTextLabel (Point3d pt, string text)
        {
            var doc = RhinoDoc.ActiveDoc;
            const double height = 5.0;
            const string font = "Arial";
            Rhino.Geometry.Plane plane = doc.Views.ActiveView.ActiveViewport.ConstructionPlane();
            plane.Origin = pt;
            Guid id = doc.Objects.AddText(text, plane, height, font, false, false);

            if (id != Guid.Empty)
            {
                doc.Views.Redraw();
            }
        }
    }
}
