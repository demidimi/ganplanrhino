using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace GanPlanRhino
{
    public class AreaCalcResult
    {
        public double totalArea;
        public int floorNumber;
        public double[] floorArea;

        public AreaCalcResult(double[] floorAreaArray)
        {
            floorArea = floorAreaArray;
            totalArea = floorAreaArray.Sum();
            floorNumber = floorAreaArray.Length;
        }
    }
    public class AreaCalculation
    {
        public static double CalcTotalBrepArea(Brep b, double floorHeight)
        {
            return CalcBrepArea(b, floorHeight).totalArea;
        }

        public static AreaCalcResult CalcBrepArea(Brep b, double floorHeight)
        {
            BoundingBox bounding = b.GetBoundingBox(false);
            Point3d topPt = bounding.Corner(true, true, false);
            Point3d bottomPt = bounding.Corner(true, true, true);
            Curve[] contourCurves = Brep.CreateContourCurves(b, bottomPt, topPt, floorHeight);

            AreaCalcResult areaResult = new AreaCalcResult(contourCurves.Select(c => AreaMassProperties.Compute(c).Area).ToArray());
            return areaResult;
        }
    }
}
