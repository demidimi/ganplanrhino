using Eto.Forms;
using System.Windows.Input;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Input;
using System.Collections.Generic;
namespace GanPlanRhino
{
    internal class EtoPanelGanPlan : GanPlanRhinoEtoPanel
    {
        ICommand CallAPI;
        ICommand CalcRecArea;
        ICommand MakeEltjShapes;
        ICommand UpdateEltjShapeAreas;
        ICommand PlaceDoors;
        ICommand Make3DGeometry;

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
        public EtoPanelGanPlan(GanPlanRhinoPanelViewModel dataContext) : base(dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }

        private void InitializeComponent()
        {

            CallAPI = new RelayCommand<object>(obj =>
            {
                LayerHelper.CheckLayerStructure((schemeNameBox.Text + "::Rectangles"));
                LayerHelper.CheckLayerStructure((schemeNameBox.Text + "::EJLT Shapes"));
            });
            CalcRecArea = new RelayCommand<object>(obj => { UpdateArea(schemeNameBox.Text + "::Rectangles", area); });
            MakeEltjShapes = new RelayCommand<object>(obj => { IntersectNow(schemeNameBox.Text + "::Rectangles", schemeNameBox.Text + "::EJLT Shapes"); });
            UpdateEltjShapeAreas = new RelayCommand<object>(obj => { UpdateArea(schemeNameBox.Text + "::EJLT Shapes", area); });
            PlaceDoors = new RelayCommand<object>(obj => { message.Text = "PlaceDoors"; });
            Make3DGeometry = new RelayCommand<object>(obj => { message.Text = "Make3DGeometry"; });


            CallAPIButton = new Button
            {
                Text = Rhino.UI.LOC.STR("Call API"),
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
                Text = Rhino.UI.LOC.STR("Calc Shape Areas"),
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

            message = new Label();
            message.Text = "Paste in URL to start!";
            message.Height = 30;
            message.TextColor = new Eto.Drawing.Color(0, 0, 1);

            warning = new Label();
            warning.Text = "";
            warning.TextColor = new Eto.Drawing.Color(1, .2f, 0);
            warning.Font = new Eto.Drawing.Font(Eto.Drawing.SystemFont.Bold, 11);

            area = new Label();
            area.Text = "";

            urlInputBox = new TextBox();
            urlInputBox.Text = "paste in URL";

            schemeNameBox = new TextBox();
            schemeNameBox.Text = "Scheme1";

            Content = new TableLayout
            {
                Padding = 0,
                Spacing = SpacingSize,
                Rows =
                {
                new Label
                {
                    Text="GanPlan",
                    Font =  new Eto.Drawing.Font(Eto.Drawing.SystemFont.Bold, 13)
                },
                message,

                null,
                new Label { Text= "Name your Scheme: "},
                schemeNameBox,
                null,
                new Label { Text="API URL:" },
                urlInputBox,
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
                new TableRow(new NextBackButtons(ViewModel, false))
                }
            };
        }

        private static void UpdateArea(string layerPath, Label area)
        {
            List<int> layerIndexs;
            List<Curve> curves = LayerHelper.GetCurvesFrom(
                        layerPath, out layerIndexs);
            area.Text = AreaCalc.UpdateArea(curves, layerIndexs);
        }
        public static void IntersectNow(string layerPath, string targetPath)
        {
            //initialize variables
            List<int> layerIndexs;
            List<Curve> c;
            List<Curve> splitCurves;
            string layerName;

            //get curves from the specific layer - rectangles
            c = LayerHelper.GetCurvesFrom(layerPath, out layerIndexs);

            //split curves and bake to target layer
            splitCurves = Intersect.IntersectCurves(c);
            for (int i = 0; i < splitCurves.Count; i++)
            {
                layerName = Rhino.RhinoDoc.ActiveDoc.Layers[layerIndexs[i]].Name;
                LayerHelper.BakeObjectToLayer(splitCurves[i], layerName, targetPath);
                Rhino.RhinoApp.WriteLine("baking split curve {0} to layer {1} ", splitCurves[i].ToString(), layerName);
            }
            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();



        }
    }
}
