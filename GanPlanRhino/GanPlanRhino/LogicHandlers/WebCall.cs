using System;
using System.IO;
using System.Threading.Tasks;
using Rhino;
using Newtonsoft.Json.Linq;
using System.Reflection;

using System.Collections.Generic;
using Rhino.Geometry;
using System.Net.Http;

namespace GanPlanRhino
{
    public class WebCallManager
    {
        private static readonly HttpClient client = new HttpClient();
        private string latestResponse = "";
        public string targetLayerName = "";

        public void QueryMLServer(string parentLayerName)
        {
            //call the real code
            targetLayerName = parentLayerName;
            MakeAsyncRequest();
        }

        public async Task MakeAsyncRequest()
        {
            using (HttpResponseMessage response = await client.GetAsync("https://ganplan.emptybox.io/api/floorplans/selected"))
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                lock (this.latestResponse)
                {
                    //Check server is giving an acceptable response code
                    //if so, update
                    latestResponse = responseBody;

                    //if not...?
                }
                GotWebResponse();
            }
        }
 
        private void GotWebResponse()
        {
            //Register an event listener so Rhino knows we're waiting to do stuff
            RhinoApp.Idle += OnRhinoIdle;
        }

        private void OnRhinoIdle(object sender, EventArgs e)
        {
            //When Rhino is ready, unregister the event listener, then go ahead and draw the rectangles
            RhinoApp.Idle -= OnRhinoIdle;
            lock (latestResponse)
            {
                //Draw rectangles, assign colors and layers, etc
                MakeRectanglesFromString(targetLayerName, latestResponse);
            }
        }
    
        public void MakeRectanglesFromString(string parentLayerName, string input)
        {
            Response myResponseAsObjects = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(input);

            List<Rhino.Geometry.Rectangle3d> output = new List<Rhino.Geometry.Rectangle3d>();


            Rhino.DocObjects.Tables.LayerTable layerTable = Rhino.RhinoDoc.ActiveDoc.Layers;
            int parentLayerIndex = layerTable.FindByFullPath(parentLayerName, -1);



            int i = 0;

            foreach (Room myRoom in myResponseAsObjects.data.rooms)
            {
                Rhino.Geometry.Plane basePlane = Rhino.Geometry.Plane.WorldXY;
                Rhino.Geometry.Point3d cornerA = new Rhino.Geometry.Point3d(myRoom.rectangle[0][0], -myRoom.rectangle[0][1], 0);
                Rhino.Geometry.Point3d cornerB = new Rhino.Geometry.Point3d(myRoom.rectangle[1][0], -myRoom.rectangle[1][1], 0);
                Rectangle3d oneRectangle = new Rhino.Geometry.Rectangle3d(basePlane, cornerA, cornerB);
                LayerHelper.BakeObjectToLayer(oneRectangle.ToPolyline().ToPolylineCurve(), i + "_" + myRoom.room, parentLayerName);
                i++;
            }

            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
        }

        //Classes that are auto-populated by Json serialization
        public class Room
        {
            public List<List<double>> rectangle { get; set; }
            public string room { get; set; }
            public string roomColor { get; set; }
        }

        public class Data
        {
            public int iteration { get; set; }
            public IList<Room> rooms { get; set; }
        }

        public class Response
        {
            public Data data { get; set; }
            public string status { get; set; }
        }

    }
}
