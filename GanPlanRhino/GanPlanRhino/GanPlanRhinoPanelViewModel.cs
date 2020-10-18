using System.Windows.Input;
using Eto.Forms;
using Rhino;

namespace GanPlanRhino
{
    /// <summary>
    /// The view model associated with a specific Rhino document used by
    /// a instance of a MainPanel for the life of the document.  The MainPanel
    /// and view model will get disposed when the document closes.
    /// </summary>
    internal class GanPlanRhinoPanelViewModel : Rhino.UI.ViewModel
    {
        public GanPlanRhinoPanelViewModel(uint documentRuntimeSerialNumber)
        {
            // Button commands
            // TURNED OFF AS TEST 
            //NextCommand = new RelayCommand<object>(obj => { NextButtonCommand(); });
            //BackCommand = new RelayCommand<object>(obj => { BackButtonCommand(); });
            //FinishCommand = new RelayCommand<object>(obj => { FinishButtonCommand(); });


            // Read-only property initialization
            DocumentRuntimeSerialNumber = documentRuntimeSerialNumber;

            // List of wizard panels in the order displayed
            m_panels = new GanPlanRhinoEtoPanel[]
            {
                // TURNED OFF AS TEST 
                //new EtoPanel0(this),
                new EtoPanelGanPlan(this),
                // TURNED OFF AS TEST 
                //new EtoPanel1(this)
            };

            // First panel
            Content = m_panels[0];
        }

        #region Document access
        public uint DocumentRuntimeSerialNumber { get; }
        public RhinoDoc Document => RhinoDoc.FromRuntimeSerialNumber(DocumentRuntimeSerialNumber);
        #endregion Document access

        /// <summary>
        /// Bind this to the main panel container contents, will get set by the
        /// next and back buttons
        /// </summary>
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
        /// <summary>
        /// List of content pages displayed in m_main_panel
        /// </summary>
        private readonly GanPlanRhinoEtoPanel[] m_panels;

        /// <summary>
        /// Current panel index used by the Next and Back buttons to navigate the
        /// m_panels array
        /// </summary>
        private int m_current_panel;

        #region Next, Back and Finish commands
        private void NextButtonCommand()
        {
            if (m_current_panel >= m_panels.Length)
                return;
            Content = m_panels[++m_current_panel];
        }

        private void BackButtonCommand()
        {
            if (m_current_panel <= 0)
                return;
            Content = m_panels[--m_current_panel];
        }

        private void FinishButtonCommand()
        {
            MessageBox.Show("Finish...");
        }

        #endregion Next, Back and Finish commands

        #region Button ICommand handlers
        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand FinishCommand { get; }
        #endregion Button ICommand handlers
    }
}