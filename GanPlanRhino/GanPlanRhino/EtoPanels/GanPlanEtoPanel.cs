using Eto.Drawing;
using Eto.Forms;

namespace GanPlanRhino
{
    internal abstract class GanPlanRhinoEtoPanel : Panel
    {
        /// <summary>
        /// Abstract class used by the AreaTrackerPanelViewModel as MainPanel.Content
        /// </summary>
        protected GanPlanRhinoEtoPanel(GanPlanRhinoPanelViewModel dataContext)
        {
            ViewModel = dataContext;
        }

        /// <summary>
        /// The view model associated with a specific Rhino document used by
        /// a instance of a MainPanel for the life of the document.  The MainPanel
        /// and view model will get disposed when the document closes.
        /// </summary>
        protected GanPlanRhinoPanelViewModel ViewModel { get; }
        /// <summary>
        /// Standard spacing used by all wizard pages
        /// </summary>
        public static int Spacing => 4;
        /// <summary>
        /// Standard spacing used by all wizard pages
        /// </summary>
        public static Size SpacingSize = new Size(Spacing, Spacing);
    }
}
