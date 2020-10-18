using System.Runtime.InteropServices;
using Eto.Forms;


namespace GanPlanRhino
{
    [Guid("79069458-c551-4151-b8f7-c43fa1603f2e")]
    public class MainPanel : Panel
    {
        public MainPanel(uint documentRuntimeSerialNumber)
        {
            // Padding around the main container, child panels should use a padding of 0
            Padding = 6;

            // ViewModel associated with a specific RhinoDoc.RuntimeSerialNumber
            GanPlanRhinoPanelViewModel view = new GanPlanRhinoPanelViewModel(documentRuntimeSerialNumber);
            
            // Set this panel's DataContext, Page... panels will inherit this DeviceContext
            DataContext = view;

            // Bind this panel's content to the view model, the Next and Back buttons will set this property.
            this.Bind(c => c.Content, view, m => m.Content);
        }
    }
}
