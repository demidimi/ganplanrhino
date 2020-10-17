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

namespace GanPlanRhino
{
    public class ComputeGHhelper
    {

        public static List<GrasshopperDataTree> AddSingleInput(string inputName, object input, List<GrasshopperDataTree> trees)
        {
            GrasshopperObject ginput = new GrasshopperObject(input);

            GrasshopperDataTree tree0 = new GrasshopperDataTree("RH_IN:" + inputName);
            tree0.Append(new List<GrasshopperObject>() { ginput }, new GrasshopperPath(0));
            trees.Add(tree0);
            return trees;
        }
        public static List<GrasshopperDataTree> AddListInput(string inputName, IEnumerable<object> input, List<GrasshopperDataTree> trees)
        {
            List<GrasshopperObject> ginputs = new List<GrasshopperObject>();
            foreach (object ob in input)
            {
                ginputs.Add(new GrasshopperObject(ob));
            }

            GrasshopperDataTree tree0 = new GrasshopperDataTree("RH_IN:" + inputName);
            tree0.Append(ginputs, new GrasshopperPath(0));
            trees.Add(tree0);
            return trees;
        }

        public static void WriteCrvOutputToLayer(string layerName, string outputName, Dictionary<string, List<string>> unpackedOutput)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            // set layer settings
            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
            string outputLayerName = layerName;
            att.LayerIndex = doc.Layers.Add(outputLayerName, new System.Drawing.Color());

            // write geometry to document layer
            foreach (string item in unpackedOutput["RH_OUT:" + outputName])
            {
                var geometry = Newtonsoft.Json.JsonConvert.DeserializeObject<Rhino.Geometry.Curve>(item);
                RhinoDoc.ActiveDoc.Objects.Add(geometry, att);
            }
            doc.Views.Redraw();
        }
        public static void WriteBrepOutputToLayer(string layerName, string outputName, Dictionary<string, List<string>> unpackedOutput)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            // set layer settings
            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
            string outputLayerName = layerName;
            att.LayerIndex = doc.Layers.Add(outputLayerName, new System.Drawing.Color());

            // write geometry to document layer
            foreach (string item in unpackedOutput["RH_OUT:" + outputName])
            {
                var geometry = Newtonsoft.Json.JsonConvert.DeserializeObject<Rhino.Geometry.Brep>(item);
                RhinoDoc.ActiveDoc.Objects.Add(geometry, att);
            }
            doc.Views.Redraw();
        }

        public static void WriteBrepOutputToLayer(string layerName, string outputName, List<GrasshopperDataTree> output)
        {
            Dictionary<string, List<string>> unpackedOutput = GetAllResult(output);
            WriteBrepOutputToLayer(layerName, outputName, unpackedOutput);
        }

        public static Dictionary<string, List<string>> GetAllResult(List<GrasshopperDataTree> output)
        {
            Dictionary<string, List<string>> unpacked = new Dictionary<string, List<string>>();

            foreach (GrasshopperDataTree datatree in output)
            {
                // get param name ("RHI_OUT:XXX")
                string outputName = datatree.ParamName;

                List<string> outputData = GetAllDataFromTree(datatree);
                unpacked.Add(outputName, outputData);
            }

            return unpacked;
        }

        public static List<string> GetAllDataFromTree(GrasshopperDataTree dataTree)
        {
            // get all the grasshopper objects as a list
            List<GrasshopperObject> grasshopperObjects = new List<GrasshopperObject>();
            foreach (var rt in dataTree.InnerTree.Values.ToList())
            {
                grasshopperObjects.AddRange(rt);
            }
            List<string> dataString = new List<string>();

            foreach (GrasshopperObject ob in grasshopperObjects)
            {
                dataString.Add(ob.Data);
            }

            return dataString;

        }

        public static void PrintOutputDic(Dictionary<string, List<string>> dic)
        {
            RhinoApp.WriteLine("----start output from compute----");
            foreach (var output in dic)
            {
                string dataText = string.Join(",", output.Value.ToList());
                RhinoApp.WriteLine(output.Key + " : [" + dataText + "]");
            }
            RhinoApp.WriteLine("----end output from compute----");

        }

    }
}
