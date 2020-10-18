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
        public static  List<Curve> IntersectCurves(List<Curve> curveList, List<int> layerIndexs, out List<int> layerIds)
        {
            List<Curve> splitCurves = new List<Curve>();
            layerIds = new List<int>();
            Point3d trimPt;
            Plane p;
            string contains;

            //initialize cutting curves with the first curve. Add layerIndex of this first curve.
            splitCurves.Add(curveList[0]);
            layerIds.Add(layerIndexs[0]);
            RhinoApp.WriteLine("First curve added");
            for (int i = 1; i < curveList.Count; i += 1)
            {
                Curve[] newCurve = Curve.CreateBooleanDifference(curveList[i], splitCurves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                //odd error where it is creating a second trim that it should not
                //BUG FIX: check area of trimmed closed curves. If inside original curveList[i], it's good. Otherwise remove.

                foreach (Curve split in newCurve)
                {
                    trimPt = Rhino.Geometry.AreaMassProperties.Compute(split).Centroid;
                    p = Rhino.Geometry.Plane.WorldXY;
                    contains = curveList[i].Contains(trimPt, p, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToString();               
                    RhinoApp.WriteLine(contains);

                    if (contains == "Inside" || contains == "Coincident")
                    {
                        splitCurves.Add(split);
                        layerIds.Add(layerIndexs[i]);
                        RhinoApp.WriteLine("Part of curve " + i.ToString() + " trimmed and added");
                    }
                }                 
                
            }
            return splitCurves;
        }
    }
}
