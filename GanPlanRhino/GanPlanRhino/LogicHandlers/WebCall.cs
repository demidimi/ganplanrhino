using System;
using System.IO;
using System.Threading.Tasks;
using Rhino;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;

using System.Collections.Generic;
using Rhino.Geometry;
using System.Net.Http;
using System.Drawing;

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
            //if data is empty, return with nothing.. Add a note?

            Response myResponseAsObjects = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(input);
            List<Room> roomsSortedLargeToSmall = myResponseAsObjects.data.rooms.OrderByDescending(x => x.rectangleArea).ToList();
            

            //int drawHeightTest = 0;

            foreach (Room myRoom in roomsSortedLargeToSmall)
            {
                Plane basePlane = Plane.WorldXY;
                //Plane tempBasePlane = new Plane(new Point3d(0, 0, drawHeightTest), new Vector3d(0, 0, 1));
                //drawHeightTest += 24;

                Rectangle3d oneRectangle = new Rectangle3d(basePlane, myRoom.cornerA, myRoom.cornerB);
                
                LayerHelper.BakeObjectToLayer(oneRectangle.ToPolyline().ToPolylineCurve(), myRoom.room, parentLayerName);
                
                Color layerColor = System.Drawing.ColorTranslator.FromHtml(myRoom.roomColor);
                LayerHelper.ConfirmLayerColor(layerColor, myRoom.room, parentLayerName);
            }

            RhinoDoc.ActiveDoc.Views.Redraw();
        }

        //Classes that are auto-populated by Json serialization
        public class Room
        {
            public List<List<double>> rectangle { get; set; }

            public double ScaleToFitDocument(double input)
            {
                //The input is coming in from 0-300
                //This correlates with roughly 20 feet
                double scaledToFeet = input / 10;

                UnitSystem currentDocUnits = RhinoDoc.ActiveDoc.ModelUnitSystem;
                double unitSystemScaler = RhinoMath.UnitScale(UnitSystem.Feet, currentDocUnits);

                return scaledToFeet * unitSystemScaler;
            }

            public Point3d cornerA
            {
                get
                {
                    return new Point3d(ScaleToFitDocument(this.rectangle[0][0]), ScaleToFitDocument (- this.rectangle[0][1]), 0);
                }
            }

            public Point3d cornerB
            {
                get
                {
                    return new Point3d(ScaleToFitDocument(this.rectangle[1][0]), ScaleToFitDocument (- this.rectangle[1][1]), 0);
                }
            }

            public double rectangleArea {
                get {
                    double width = this.rectangle[1][0] - this.rectangle[0][0];
                    double height = this.rectangle[1][1] - this.rectangle[0][1];
                    double area = width * height;
                    return area;
                }
                
            }
            public string room { get; set; }
            public string roomColor { get; set; }
        }

        public class Data
        {
            public int iteration { get; set; }
            public List<Room> rooms { get; set; }
        }

        public class Response
        {
            public Data data { get; set; }
            public string status { get; set; }
        }

    }
}
