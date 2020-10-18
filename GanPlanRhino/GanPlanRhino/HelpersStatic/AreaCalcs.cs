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
            UnitSystem currentDocUnits = RhinoDoc.ActiveDoc.ModelUnitSystem;
            double unitSystemScaler = RhinoMath.UnitScale(currentDocUnits, UnitSystem.Feet);

            double[] areas = new double[curves.Count()];
            for (int i = 0; i < curves.Count(); i++)
            {
                areas[i] = Math.Round(AreaMassProperties.Compute(curves[i]).Area*unitSystemScaler* unitSystemScaler, 2);
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
            UnitSystem currentDocUnits = RhinoDoc.ActiveDoc.ModelUnitSystem;
            double unitSystemScaler = RhinoMath.UnitScale(UnitSystem.Feet, currentDocUnits);
            double param;
            curve.ClosestPoint(new Point3d(-1000 * unitSystemScaler, -1000 * unitSystemScaler, 0), out param);
            Point3d pt = curve.PointAt(param);
            foreach (Point3d point in oldPts)
            {
                if (pt.Y == point.Y && Math.Abs(pt.X - point.X) <10 * unitSystemScaler)
                {
                    pt = pt + new Point3d(0, 2 * unitSystemScaler, 0);
                }
            }
            AddTextLabel(pt, text);
            return pt;
        }

        public static void AddTextLabel (Point3d pt, string text)
        {
            UnitSystem currentDocUnits = RhinoDoc.ActiveDoc.ModelUnitSystem;
            double unitSystemScaler = RhinoMath.UnitScale(UnitSystem.Feet, currentDocUnits);
            var doc = RhinoDoc.ActiveDoc;
            double height = (0.35 * unitSystemScaler);
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
