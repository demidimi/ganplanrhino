using Eto.Forms;
using Eto.Drawing;
using System.Windows.Input;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Input;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GanPlanRhino
{


    internal class EtoPanelGanPlan : Panel
    {
        ICommand OpenWebPage;
        ICommand CallAPI;
        ICommand CalcRecArea;
        ICommand MakeEltjShapes;
        ICommand UpdateEltjShapeAreas;
        ICommand PlaceDoors;
        ICommand Make3DGeometry;

        Button OpenWebPageButton;
        Button CallAPIButton;
        Button CalcRecAreaButton;
        Button MakeEltjShapesButton;
        Button UpdateEltjShapeAreasButton;
        Button PlaceDoorsButton;
        Button Make3DGeometryButton;

        Label message;
        Label warning;
        TextBox urlInputBox;
        TextBox schemeNameBox;
        public Label area;
        public EtoPanelGanPlan(GanPlanRhinoPanelViewModel dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            #region COMMAND SETUP
            OpenWebPage = new RelayCommand<object>(obj => {
                System.Diagnostics.Process.Start("https://ganplan.emptybox.io/");
                message.Text = "After Selecting a Scheme on the web, bring it into Rhino with an API call. ";
            });

            CallAPI = new RelayCommand<object>(obj => {
                WebCallManager myFooBar = new WebCallManager();
                myFooBar.QueryMLServer(schemeNameBox.Text + "::Rectangles");

                LayerHelper.CheckLayerStructure((schemeNameBox.Text+"::Rectangles"));
                LayerHelper.CheckLayerStructure((schemeNameBox.Text + "::EJLT Shapes"));

                message.Text = "Adjust the rectangles to match your desired area. ";
            });

            CalcRecArea = new RelayCommand<object>(obj => { 
                UpdateArea(schemeNameBox.Text + "::Rectangles", area);
                message.Text = "Cut out specific shapes of your rooms. ";
            });

            MakeEltjShapes = new RelayCommand<object>(obj => { IntersectNow(schemeNameBox.Text + "::Rectangles",schemeNameBox.Text + "::EJLT Shapes"); });
            
            UpdateEltjShapeAreas = new RelayCommand<object>(obj => { UpdateArea(schemeNameBox.Text + "::EJLT Shapes", area); });
            
            PlaceDoors = new RelayCommand<object>(obj => { Doors.PlaceDoorsAt(schemeNameBox.Text); });
            
            Make3DGeometry = new RelayCommand<object>(obj => { message.Text = "Make3DGeometry"; });
            #endregion


            #region Button Setup
            OpenWebPageButton = new Button
            {
                Text = Rhino.UI.LOC.STR("Open Web Page"),
                Command = OpenWebPage
            };

            CallAPIButton = new Button
            {
                Text = Rhino.UI.LOC.STR("Call API for selected Scheme"),
                Command = CallAPI
            };

            CalcRecAreaButton = new Button
            {
                Text = Rhino.UI.LOC.STR("Calc Rectangle Area"),
                Command = CalcRecArea
            };

            MakeEltjShapesButton = new Button
            {
                Text = Rhino.UI.LOC.STR("Cut Shapes"),
                Command = MakeEltjShapes
            };

            UpdateEltjShapeAreasButton = new Button
            {
                Text = Rhino.UI.LOC.STR("Calc Shape Area"),
                Command = UpdateEltjShapeAreas
            };

            PlaceDoorsButton = new Button
            {
                Text = Rhino.UI.LOC.STR("Place Some Doors"),
                Command = PlaceDoors
            };

            Make3DGeometryButton = new Button
            {
                Text = Rhino.UI.LOC.STR("See it in 3D"),
                Command = Make3DGeometry
            };
            #endregion


            #region 
            message = new Label();
            message.Text = "Open our web tool to start!";
            message.Height = 30;
            message.TextColor = new Eto.Drawing.Color(0, 0, 1);

            warning = new Label();
            warning.Text = "";
            warning.TextColor = new Eto.Drawing.Color(1, .2f, 0);
            warning.Font = new Eto.Drawing.Font(Eto.Drawing.SystemFont.Bold, 11);

            area = new Label();
            area.Text = "";
            area.Font = new Eto.Drawing.Font(Eto.Drawing.SystemFont.Bold, 11);

            urlInputBox = new TextBox();
            urlInputBox.Text = "paste in URL";

            schemeNameBox = new TextBox();
            schemeNameBox.Text = "Scheme1";
            #endregion




            Content = new TableLayout
            {

                Padding = 0,
                Spacing = new Size(4, 4),
                Rows =
                {
                    new Label
                    {
                        Text="GanPlan",
                        Font =  new Eto.Drawing.Font(Eto.Drawing.SystemFont.Bold, 13)
                    },
                    message,
                    null,
                    OpenWebPageButton,
                    null,
                    new Label { Text= "Name your Scheme: "},
                    schemeNameBox,
                    CallAPIButton,
                    null,
                    area,
                    CalcRecAreaButton,
                    MakeEltjShapesButton,
                    UpdateEltjShapeAreasButton,
                    null,
                    PlaceDoorsButton,
                    null,
                    Make3DGeometryButton,
                    null,
                }
            };
        }

        private static void UpdateArea(string layerPath, Label area)
        {
            List<int> layerIndexs;
            List<Curve> curves = LayerHelper.GetCurvesFrom(
                        layerPath, out layerIndexs);
            List<int> layerIds;


            if (layerPath.EndsWith("Rectangles"))
            {
                //split curves
                curves = Intersect.IntersectCurves(curves, layerIndexs, out layerIds);
                area.Text = AreaCalc.UpdateArea(curves, layerIds);

            }
            else
            {
                area.Text = AreaCalc.UpdateArea(curves, layerIndexs);
            }
        }

        public static void IntersectNow(string layerPath, string targetPath)
        {
            //initialize variables
            List<int> layerIndexs;
            List<Curve> c;
            List<Curve> splitCurves;
            List<int> layerIds;
            string layerName;

            //get curves from the specific layer - rectangles
            c = LayerHelper.GetCurvesFrom(layerPath, out layerIndexs);

            //split curves and bake to target layer
            splitCurves = Intersect.IntersectCurves(c, layerIndexs, out layerIds);
            for (int i = 0; i < splitCurves.Count; i++)
            {
                layerName = Rhino.RhinoDoc.ActiveDoc.Layers[layerIds[i]].Name;
                LayerHelper.BakeObjectToLayer(splitCurves[i], layerName, targetPath);
                Rhino.RhinoApp.WriteLine("baking split curve {0} to layer {1} ", splitCurves[i].ToString(), layerName);
            }
            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
        }
    }
}
