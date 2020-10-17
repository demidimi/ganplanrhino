﻿using System.Runtime.InteropServices;
using Eto.Forms;

namespace GanPlanRhino
{
    /// <summary>
    /// A MainPanel will be created for each new Rhino document and will bet 
    /// disposed of when the document closes
    /// </summary>
    [Guid("79069458-c551-4151-b8f7-c43fa1603f2e")]
    public class MainPanel : Panel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="documentRuntimeSerialNumber">
        /// RhinoDoc.RuntimeSerialNumber associated with this MainPanel instance.
        /// </param>
        public MainPanel(uint documentRuntimeSerialNumber)
        {
            // Padding around the main container, child panels should use a padding 
            // of 0
            Padding = 6;
            // ViewModel associated with a specific RhinoDoc.RuntimeSerialNumber
            var view = new GanPlanRhinoPanelViewModel(documentRuntimeSerialNumber);
            // Set this panels DataContext, Page... panels will inherit this
            // DeviceContext
            DataContext = view;
            // Bind this panel's content to the view model, the Next and Back
            // buttons will set this property.
            this.Bind(c => c.Content, view, m => m.Content);
        }
    }
}