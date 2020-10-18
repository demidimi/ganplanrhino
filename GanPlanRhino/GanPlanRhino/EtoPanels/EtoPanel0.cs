using Eto.Forms;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;
using System.Windows.Input;
using Rhino.Geometry;
using System.Collections.Generic;

namespace GanPlanRhino
{
    internal class EtoPanel0 : GanPlanRhinoEtoPanel
    {
        ICommand CalcCommand;
        ICommand SelCommand;
        ICommand IntersectCommand;
        TextBox floorHeightBox;
        Label message;

        Brep b;
        List<Curve> c = new List<Curve>();
        List<Curve> splitCurves;

        public EtoPanel0(GanPlanRhinoPanelViewModel dataContext) : base(dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }

        /// <summary>
        /// Create Panel.Content
        /// </summary>
        private void InitializeComponent()
        {
            IntersectCommand = new RelayCommand<object>(obj => { IntersectNow(); });
            CalcCommand = new RelayCommand<object>(obj => { CalculateArea(); });
            SelCommand = new RelayCommand<object>(obj => { ReadSelectedCurves(); });

            message = new Label();
            message.Text = "First Select Geometry, then click button to start analysis. ";
            message.Height = 30;
            message.TextColor = new Eto.Drawing.Color(0, 0, 1);

            floorHeightBox = new TextBox();
            floorHeightBox.Text = "10";

            Content = new TableLayout
            {
                Spacing = SpacingSize,
                Rows =
                {
                    message,

                    new Button {Text = Rhino.UI.LOC.STR("Selected Curves"), Command = SelCommand },

                    new Label { Text = "Floor Height: " },
                    new TableRow(floorHeightBox) { ScaleHeight = false},

                   

                    new StackLayout
                    {
                        Padding = 0,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Spacing = Spacing,
                        Items =
                        {
                            new Button {Text = Rhino.UI.LOC.STR("Next >"), Command = ViewModel.NextCommand },
                            new Button {Text = Rhino.UI.LOC.STR("Calculate Area"), Command = CalcCommand},
                            new Button {Text = Rhino.UI.LOC.STR("Intersect Curves"), Command = IntersectCommand},
                            // new Button {Text = Rhino.UI.LOC.STR("Calculate Area with Rhino Compute"), Command = ViewModel.NextCommand }
                        }
                    }
                }
            };
        }

        public void CalculateArea()
        {
            message.Text = "calculating Area";
            AreaCalcResult result = AreaCalculation.CalcBrepArea(b, System.Double.Parse(floorHeightBox.Text));
            message.Text = string.Format(
                "Total areai is {1} with {0} floors",
                result.floorNumber,
                result.totalArea
                );
        }
        public void IntersectNow()
        {
            //split curves then bake them
            message.Text = "Intersecting curves";
            //splitCurves = Intersect.IntersectCurves(c);
            message.Text = "Baking split curves";
            foreach (Curve i in splitCurves)
            {
                LayerHelper.BakeObjectToLayer(i, "Layer 02", "Layer 01");
            }
            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();

            //get curves from the specific layer - rectangles
            // bake curves to the correct new split curves layers



        }
        public void ReadSelectedBrep()
        {
            GetObject go = new GetObject();
            go.GeometryFilter = Rhino.DocObjects.ObjectType.Brep;
            go.Get();

            message.Text = "selected " + go.ObjectCount.ToString() + " brep.";
            b = go.Object(0).Brep();

        }
        public void ReadSelectedCurves()
        {
            GetObject go = new GetObject();
            go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
            go.Get();

            message.Text = "selected " + go.ObjectCount.ToString() + " curves.";
            ObjRef[] oReference = go.Objects();
            foreach (ObjRef i in oReference)
                {
                c.Add(i.Curve());
                Rhino.RhinoApp.WriteLine(i.ToString());
                }

        }

    
    }
}
