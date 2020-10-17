using System;
using System.Collections;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using Rhino.DocObjects;
using Rhino.Collections;

namespace GanPlanRhino
{
    public static class LayerHelper
    {
        public static string CheckLayerStructure(string rootLayerPath)
        {
            string parentOfNestedLayer = "";

            //The rootPath is optional (if it is used, ensure the existence of each part of the path)
            if (!String.IsNullOrEmpty(rootLayerPath))
            {
                //EnsureFullPath(rootLayerPath + "::" + targetBakeLayer);
                //Break the single monolithic string into an array of the nested layers' names
                string[] rootLayers = rootLayerPath.Split(new string[] { "::" }, StringSplitOptions.None);
                //Check each nested level
                foreach (string nestedLayer in rootLayers)
                {
                    //Confirm the name for the nested level is valid
                    if (string.IsNullOrEmpty(nestedLayer) || !Rhino.DocObjects.Layer.IsValidName(nestedLayer))
                    {
                        RhinoApp.WriteLine("Parent layer name " + nestedLayer + " is invalid");
                        return"";
                        //this.Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Some part of Root Layer Path is invalid");
                    }
                    //Ensure the layer exists
                    RhinoApp.WriteLine("Ensuring parent layer...");
                    string ensuredParent = EnsureLayer(nestedLayer, parentOfNestedLayer);
                    RhinoApp.WriteLine("The layer '" + ensuredParent + "' has been ensured...");
                    RhinoApp.WriteLine("-------------------------");
                    //Once the layer has been ensured, use it as the parent for the next layer
                    parentOfNestedLayer = parentOfNestedLayer + ensuredParent;
                }
            }
            return rootLayerPath;
        }

        public static void BakeObjectToLayer(GeometryBase obj, string layerName, string rootLayerPath)
        {
            string parentOfNestedLayer = null;
            RhinoDoc doc = RhinoDoc.ActiveDoc;


            //Check geometry object is valid
            if (obj == null)
            {
                RhinoApp.WriteLine("Object is null...");
                //this.Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry object is null");
                return;
            }
            if (layerName == null)
            {
                RhinoApp.WriteLine("Target layer is null...");
                //this.Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Target Layer is null");
                return;
            }

            if (!String.IsNullOrEmpty(rootLayerPath))
            {
                parentOfNestedLayer = CheckLayerStructure(rootLayerPath);
            }

            //Check layer name is valid
            if (string.IsNullOrEmpty(layerName) || !Rhino.DocObjects.Layer.IsValidName(layerName))
            {
                RhinoApp.WriteLine("Layer name " + layerName + " is invalid");
                return;
                //this.Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Layer name " + layerName + " is invalid");
            }
            //Ensure the target layer exists (at the end of the optional root path)
            RhinoApp.WriteLine("Ensuring child layer...");
            string ensuredLayer = EnsureLayer(layerName, parentOfNestedLayer);
            RhinoApp.WriteLine("-------------------------");
            //Make new attribute to set name
            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
            Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
            int layerIndex = layerTable.FindByFullPath(ensuredLayer, -1);
            att.LayerIndex = layerIndex;
            Bake(obj, att);
            
        }

        public static void Bake(object obj, Rhino.DocObjects.ObjectAttributes att)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            if (obj == null)
                return;
            Guid id;
            //Bake to the right type of object
            if (obj is GeometryBase)
            {
                GeometryBase geomObj = obj as GeometryBase;
                switch (geomObj.ObjectType)
                {
                    case Rhino.DocObjects.ObjectType.Brep:
                        id = doc.Objects.AddBrep(obj as Brep, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Curve:
                        id = doc.Objects.AddCurve(obj as Curve, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Point:
                        id = doc.Objects.AddPoint((obj as Rhino.Geometry.Point).Location, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Surface:
                        id = doc.Objects.AddSurface(obj as Surface, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Mesh:
                        id = doc.Objects.AddMesh(obj as Mesh, att);
                        break;
                    case (Rhino.DocObjects.ObjectType)1073741824://Rhino.DocObjects.ObjectType.Extrusion:
                        id = (Guid)typeof(Rhino.DocObjects.Tables.ObjectTable).InvokeMember("AddExtrusion", BindingFlags.Instance | BindingFlags.InvokeMethod, null, doc.Objects, new object[] { obj, att });
                        break;
                    case Rhino.DocObjects.ObjectType.PointSet:
                        id = doc.Objects.AddPointCloud(obj as Rhino.Geometry.PointCloud, att); //This is a speculative entry
                        break;
                    default:
                        RhinoApp.WriteLine("The script does not know how to handle this type of geometry: " + obj.GetType().FullName);
                        return;
                }
            }
            else
            {
                Type objectType = obj.GetType();
                if (objectType == typeof(Arc))
                {
                    id = doc.Objects.AddArc((Arc)obj, att);
                }
                else if (objectType == typeof(Box))
                {
                    id = doc.Objects.AddBrep(((Box)obj).ToBrep(), att);
                }
                else if (objectType == typeof(Circle))
                {
                    id = doc.Objects.AddCircle((Circle)obj, att);
                }
                else if (objectType == typeof(Ellipse))
                {
                    id = doc.Objects.AddEllipse((Ellipse)obj, att);
                }
                else if (objectType == typeof(Polyline))
                {
                    id = doc.Objects.AddPolyline((Polyline)obj, att);
                }
                else if (objectType == typeof(Sphere))
                {
                    id = doc.Objects.AddSphere((Sphere)obj, att);
                }
                else if (objectType == typeof(Point3d))
                {
                    id = doc.Objects.AddPoint((Point3d)obj, att);
                }
                else if (objectType == typeof(Line))
                {
                    id = doc.Objects.AddLine((Line)obj, att);
                }
                else if (objectType == typeof(Vector3d))
                {
                    RhinoApp.WriteLine("Impossible to bake vectors");
                    return;
                }
                else
                {
                    RhinoApp.WriteLine("Unhandled type: " + objectType.FullName);
                    return;
                }
            }
        }
        public static string EnsureLayer(string layerName, string parentLayer)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            //Get a fresh copy of the LayerTable
            Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
            //Build a search string to see if the layer exists already
            string layerNameSearchString = "";
            if (parentLayer != null && parentLayer != "")
            {
                layerNameSearchString += parentLayer + "::";
            }
            layerNameSearchString += layerName;
            //Run the actual check to see if the Layer exists
            RhinoApp.WriteLine("Checking '" + layerNameSearchString + "' to see if it exists.");
            int layerIndex = layerTable.FindByFullPath(layerNameSearchString, -1);
            //The layer was found, return it
            if (layerIndex >= 0)
            {
                RhinoApp.WriteLine("Layer '" + layerNameSearchString + "' was found.");
                RhinoApp.WriteLine("Moving on.");
                return layerNameSearchString;
            }
            //This layer was not found/ does not exist... yet
            else
            {
                RhinoApp.WriteLine("Layer '" + layerNameSearchString + "' was not found.");
                //Create the layer
                RhinoApp.WriteLine("Creating layer '" + layerName + "'.");
                Rhino.DocObjects.Layer layer = new Rhino.DocObjects.Layer();
                layer.Name = layerName;
                //Make the new layer a child of the parent layer (if there was one)
                if (parentLayer != null && parentLayer != "")
                {
                    RhinoApp.WriteLine("Setting layer '" + layerName + "' as a child of layer '" + parentLayer + "'.");
                    int parentLayerIndex = layerTable.FindByFullPath(parentLayer, -1);
                    Rhino.DocObjects.Layer parentLayerObject = layerTable.FindIndex(parentLayerIndex);
                    layer.ParentLayerId = parentLayerObject.Id;
                }
                //Add the layer to the document
                layerTable.Add(layer);
                //Return the newly created layer
                return layerNameSearchString;
            }
        }
        public static Rhino.DocObjects.Layer EnsureFullPath(string fullpath)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            //Get a fresh copy of the LayerTable
            Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
            //Run the actual check to see if the Layer exists
            RhinoApp.WriteLine("Checking " + fullpath + " to see if it exists.");
            int layerIndex = layerTable.FindByFullPath(fullpath, -1);
            //The layer was found, return it
            if (layerIndex >= 0)
            {
                RhinoApp.WriteLine("That layer does exist.");
                return layerTable.FindIndex(layerIndex);
            }
            //This layer was not found/ does not exist... yet
            else
            {
                RhinoApp.WriteLine("That layer does not exist.");
                //Ensure the layer's parent exists (recursive)
                int splitPoint = fullpath.LastIndexOf("::");
                string parentPath = fullpath.Substring(0, splitPoint);
                string childLayer = fullpath.Substring(1, splitPoint);
                Rhino.DocObjects.Layer parentLayer = null;
                if (parentPath.Length > 0)
                {
                    RhinoApp.WriteLine("Checking to see if parent layer exists...");
                    parentLayer = EnsureFullPath(parentPath);
                }
                //Create this new layer (on the parent path)
                RhinoApp.WriteLine("Creating layer " + childLayer + "...");
                Rhino.DocObjects.Layer layer = new Rhino.DocObjects.Layer();
                layer.Name = childLayer;
                //Make the new layer a child of the parent layer (if there was one)
                if (parentLayer != null)
                {
                    RhinoApp.WriteLine("Assigning layer " + childLayer + " as of child of " + parentPath); //That layer does not exist.");
                    layer.ParentLayerId = parentLayer.Id;
                }
                //Add the layer to the document
                layerTable.Add(layer);
                //Return the newly created layer
                return layer;
            }
        }

        public static List<Curve> GetCurvesFrom(string fullLayerPath, out List<int> layerIndexs)
        {
            List<Curve> curves = new List<Curve>();
            layerIndexs = new List<int>();
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            // Read from Rhino Layer
            Layer layer = doc.Layers[doc.Layers.FindByFullPath(fullLayerPath, -1)];
            Layer[] children = layer.GetChildren();

            // read all objs from children layers
            List<RhinoObject> rhobjs = new List<RhinoObject>();
            foreach (Layer child in children)
            {
                rhobjs.AddRange(doc.Objects.FindByLayer(child));
            }

            if (rhobjs == null || rhobjs.Count < 1)
            {
                RhinoApp.WriteLine("no object to be selected on this layer");
                return curves;
            }
            foreach (Rhino.DocObjects.RhinoObject ob in rhobjs)
            {
                if (ob.Geometry.ObjectType == ObjectType.Curve)
                {
                    Curve c = (Curve)ob.Geometry;
                    if (c.IsClosed)
                    {
                        curves.Add(c);
                        layerIndexs.Add(ob.Attributes.LayerIndex);
                    }
                }

            }
            return curves;
        }

    }
}
