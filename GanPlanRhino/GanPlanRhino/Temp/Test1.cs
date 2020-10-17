using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Compute;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using Rhino.Runtime;
using static GanPlanRhino.ComputeGHhelper;

namespace GanPlanRhino
{
    public static class Test1
    {
        public static double TestGhLayer(string layerName, double fh)
        {
/*            ComputeServer.WebAddress = "http://3.237.37.80:80/";*/
            ComputeServer.AuthToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwIjoiUEtDUyM3IiwiYyI6IkFFU18yNTZfQ0JDIiwiYjY0aXYiOiJUSitJaTExZDBNampkVHVwSFFZL21BPT0iLCJiNjRjdCI6InJRNkFMOCs4MGNDKzNaQVhhb2tQNENWMndiYjF4Z0JBZG5tSC80VTQzY2N0VEFxVkpGYW5IOVlDd1VPZEc3eXZtZE5lZjZITnozM2dobVF0OC9lOW5McjJGbFhQaElDV1p3VzVRajY1U1JKc3BCQmpvdGRaQ1R4YVVmUVFQMnBMdEhmcWRqaUtNcjE0WHV3a0IwOWc5d3JGaTJtQWNrd1VGeHRSd0U5cjhEc1ZlQ0hIaGJaU29rSVZNdGNsdnNjZmJiOE9qd1JCU2dBVlFFSGp1SUpLNWc9PSIsImlhdCI6MTU5NTYxNTI0M30.aSrr557SxKN3JgepmCo6Lv3S3-P3Da8XHQaTciMAvvU";

            RhinoApp.WriteLine("Running TestGhLayer");

            RhinoDoc doc = RhinoDoc.ActiveDoc;

            // Read from Rhino Layer
            Rhino.DocObjects.RhinoObject[] rhobjs = doc.Objects.FindByLayer(layerName);
            if (rhobjs == null || rhobjs.Length < 1)
            {
                RhinoApp.WriteLine("no object to be selected on this layer");
                return 0.0;
            }
            List<Brep> breps = new List<Brep>();
            foreach (Rhino.DocObjects.RhinoObject ob in rhobjs)
            {
                if (ob.Geometry.HasBrepForm)
                    breps.Add(Brep.TryConvertBrep(ob.Geometry));
            }

            // create input trees
            List<GrasshopperDataTree> trees = new List<GrasshopperDataTree>();
            trees = AddSingleInput("fl", fh, trees);
            trees = AddListInput("breps", breps, trees);


            // make api call
            // for the file path here, a relative path from the bin folder only works when running in debug mode
            List<GrasshopperDataTree> output = GrasshopperCompute.EvaluateDefinition("TestGh.ghx", trees);

            // convert result to strings
            Dictionary<string, List<string>> unpackedOutput = GetAllResult(output);

            WriteBrepOutputToLayer(layerName + "_floors", "floors", unpackedOutput);

            return System.Double.Parse(unpackedOutput["RH_OUT:area"].First());

        }

    }
}
