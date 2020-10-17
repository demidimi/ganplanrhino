using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
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

        public static string UpdateArea (List<Curve> curves)
        {
            double[] areas = AreaOf(curves);
            return String.Join("", areas);
        }
    }
}
