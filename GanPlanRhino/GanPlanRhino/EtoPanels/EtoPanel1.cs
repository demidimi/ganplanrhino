using Eto.Forms;
using System.Windows.Input;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Input;
using System.Collections.Generic;
namespace GanPlanRhino
{
    internal class EtoPanel1 : GanPlanRhinoEtoPanel
    {
        ICommand TestGhLayer;
        Label message;
        TextBox input1Box;
        TextBox input2Box;
        Label output2Label;
        public EtoPanel1(GanPlanRhinoPanelViewModel dataContext) : base(dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }

        private void InitializeComponent()
        {

            TestGhLayer = new RelayCommand<object>(obj => {
                output2Label.Text
                = "total area = " +
                Test1.TestGhLayer(
                    input1Box.Text,
                    System.Double.Parse(input2Box.Text)
                );
            });

            message = new Label();
            message.Text = "First Set Layer to read from, then click button to start analysis. ";
            message.Height = 30;
            message.TextColor = new Eto.Drawing.Color(0, 0, 1);
            output2Label = new Label();
            output2Label.Text = "total area = empty";
            input1Box = new TextBox();
            input1Box.Text = "Layer1";
            input2Box = new TextBox();
            input2Box.Text = "8";



            Content = new TableLayout
            {
                Padding = 0,
                Spacing = SpacingSize,
                Rows =
                {
                null,
                new Label { Text="Area Track from Layer" },
                null,
                new Label {Text = "calculate floor area of brep with Rhino Compute: "},
                        message,
                        new Label {Text = "layer name: "},
                        input1Box,
                        new Label { Text = "Floor height: "},
                        input2Box,
                        output2Label,
                        new Button
                        {
                        Text = Rhino.UI.LOC.STR("calculate area!"),
                        Command = TestGhLayer
                        },
                null,
                new TableRow(new NextBackButtons(ViewModel, true))
                }
            };
        }
    }
}
