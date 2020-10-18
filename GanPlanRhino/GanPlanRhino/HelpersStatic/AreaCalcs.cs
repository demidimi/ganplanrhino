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
                lines.Add(RhinoDoc.ActiveDoc.Layers[layerIndexs[i]].Name + ": " + areas[i] + " sqft");
            }
            return String.Join("\r\n", lines);
        }


    }
}
