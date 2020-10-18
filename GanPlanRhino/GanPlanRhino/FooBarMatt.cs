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
    public class FooBar
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        private static readonly HttpClient client = new HttpClient();
        //private JObject latestResponse = new JObject();
        private string latestResponse = "";
        public string targetLayerName = "";

        public async Task<string> MakeAsyncRequest()
        {
            JObject json = new JObject();

            using (HttpResponseMessage response = await client.GetAsync("https://optimus.emptybox.io/api/floorplans/selected"))
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                //json = JObject.Parse(responseBody);
                lock (this.latestResponse)
                {
                    //latestResponse = json;
                    latestResponse = responseBody;
                }
                GotWebResponse();
            }

            return latestResponse;
            //MessageBox.Show("Finish");
        }

 
        private void GotWebResponse()
        {
            RhinoApp.Idle += OnRhinoIdle;
        }

        private void OnRhinoIdle(object sender, EventArgs e)
        {
            RhinoApp.Idle -= OnRhinoIdle;
            // Handle web response here...
            lock (latestResponse)
            {
                //Draw rectangles, assign colors and layers, etc
                MakeRectanglesFromString(targetLayerName, latestResponse);
            }
        }
    
        public enum QueryTypeEnum
        {
            real,
            noResults,
            fakeOne,
            fakeTwo
        }

        public void QueryMLServer(QueryTypeEnum queryType = QueryTypeEnum.real)
        {
            string myPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);



            switch (queryType)
            {
                case QueryTypeEnum.noResults:
                    //JObject NoResultsObject = JObject.Parse("{\"data\": {}, \"status\": \"OK\"}");
                    latestResponse = "{\"data\": {}, \"status\": \"OK\"}";
                    GotWebResponse(); //fake but thats ok
                    break;
                case QueryTypeEnum.fakeOne:
                    //JObject FakeResultOne = JObject.Parse();
                    latestResponse = System.IO.File.ReadAllText(myPath + "/FakeResultOne.txt");
                    GotWebResponse(); //fake but thats ok
                    break;
                case QueryTypeEnum.fakeTwo:
                    //JObject FakeResultTwo = JObject.Parse(System.IO.File.ReadAllText(myPath + "/FakeResultTwo.txt"));
                    latestResponse = System.IO.File.ReadAllText(myPath + "/FakeResultTwo.txt");
                    GotWebResponse(); //fake but thats ok
                    break;
                case QueryTypeEnum.real:
                default:
                    //call the real code
                    MakeAsyncRequest();//includes a call to GotWebResponse after the service responds
                    break;
            }
        }

        public void MakeRectanglesFromString(string parentLayerName, string input)
        {
            Response myResponseAsObjects = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(input);

            List<Rhino.Geometry.Rectangle3d> output = new List<Rhino.Geometry.Rectangle3d>();


            Rhino.DocObjects.Tables.LayerTable layerTable = Rhino.RhinoDoc.ActiveDoc.Layers;
            int parentLayerIndex = layerTable.FindByFullPath(parentLayerName, -1);
            
            


            foreach (Room myRoom in myResponseAsObjects.data.rooms)
            {
                Rhino.Geometry.Plane basePlane = Rhino.Geometry.Plane.WorldXY;
                Rhino.Geometry.Point3d cornerA = new Rhino.Geometry.Point3d(myRoom.rectangle[0][0], myRoom.rectangle[0][1], 0);
                Rhino.Geometry.Point3d cornerB = new Rhino.Geometry.Point3d(myRoom.rectangle[1][0], myRoom.rectangle[1][1], 0);
                Rectangle3d oneRectangle = new Rhino.Geometry.Rectangle3d(basePlane, cornerA, cornerB);
                LayerHelper.BakeObjectToLayer(oneRectangle.ToPolyline().ToPolylineCurve(), myRoom.room, parentLayerName);
            }

            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
        }

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
