using System.Windows.Input;
using Eto.Forms;
using Rhino;

namespace GanPlanRhino
{
    internal class GanPlanRhinoPanelViewModel : Rhino.UI.ViewModel
    {
        public GanPlanRhinoPanelViewModel(uint documentRuntimeSerialNumber)
        {
            // Read-only property initialization
            DocumentRuntimeSerialNumber = documentRuntimeSerialNumber;

            Content = new EtoPanelGanPlan(this);
        }

        public Control Content
        {
            get => m_content;
            set
            {
                if (m_content == value)
                    return;
                m_content = value;
                RaisePropertyChanged(nameof(Content));
            }
        }
        private Control m_content;

        #region Document access
        public uint DocumentRuntimeSerialNumber { get; }
        public RhinoDoc Document => RhinoDoc.FromRuntimeSerialNumber(DocumentRuntimeSerialNumber);
        #endregion Document access
    }
}