using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Activities;

namespace GanPlanRhino
{
    public class FooBar
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        private static readonly HttpClient client = new HttpClient();
        private JObject latestResponse = new JObject();

        public async Task<JObject> MakeAsyncRequest(string[] IDs)
        {
            JObject json = new JObject();

            using (HttpResponseMessage response = await client.GetAsync("http://www.ganplan.emptybox.io/api/floorplans/selected"))
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                json = JObject.Parse(responseBody);
                lock (this.latestResponse)
                {
                    latestResponse = json;
                }
                GotWebResponse();
            }

            return json;
            //MessageBox.Show("Finish");
        }

        private class ListEventArgs : EventArgs
        {
            public JObject Data { get; set; }
            public ListEventArgs(JObject data)
            {
                Data = data;
            }
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

            }
        }
    
        public enum QueryTypeEnum
        {
            real,
            noResults,
            fakeOne,
            fakeTwo
        }

        public JObject QueryMLServer(QueryTypeEnum queryType = QueryTypeEnum.real)
        {
            string myPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            JObject outputBuffer = new JObject();

            switch (queryType)
            {
                case QueryTypeEnum.noResults:
                    JObject NoResultsObject = JObject.Parse("{\"data\": {}, \"status\": \"OK\"}");
                    outputBuffer = NoResultsObject;
                    break;
                case QueryTypeEnum.fakeOne:
                    JObject FakeResultOne = JObject.Parse(System.IO.File.ReadAllText(myPath + "/FakeResultOne.txt"));
                    outputBuffer = FakeResultOne;
                    break;
                case QueryTypeEnum.fakeTwo:
                    JObject FakeResultTwo = JObject.Parse(System.IO.File.ReadAllText(myPath + "/FakeResultTwo.txt"));
                    outputBuffer = FakeResultTwo;
                    break;
                case QueryTypeEnum.real:
                default:
                    //call the real code
                    break;
            }

            return outputBuffer;
           
        }


    }
}
