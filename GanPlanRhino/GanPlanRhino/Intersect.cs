using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace GanPlanRhino
{
    public static class Intersect
    {
        public static  List<Curve> IntersectCurves(List<Curve> curveList)
        {
            List<Curve> splitCurves = new List<Curve>();

            for (int i = 0; i < curveList.Count; i += 1)
            {
                Curve[] newCurve = Curve.CreateBooleanDifference(curveList[i], splitCurves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                splitCurves.AddRange(newCurve.ToList());
                RhinoApp.WriteLine("i");
            }
            return splitCurves;
        }
    }
}
