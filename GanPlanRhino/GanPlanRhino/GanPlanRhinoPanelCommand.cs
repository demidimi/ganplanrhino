using System;
using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.UI;

namespace GanPlanRhino
{
    public class GanPlanRhinoPanel : Rhino.Commands.Command
    {
        static GanPlanRhinoPanel _instance;
        public GanPlanRhinoPanel()
        {
            _instance = this;

            Panels.RegisterPanel(PlugIn, typeof(MainPanel), LOC.STR("GanPlanRhinoPanel"), null);

        }

        ///<summary>The only instance of the MyCommand1 command.</summary>
        public static GanPlanRhinoPanel Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "GanPlanRhinoPanel"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Panels.OpenPanel(typeof(MainPanel).GUID);
            return Rhino.Commands.Result.Success;
        }

        public static implicit operator Control(GanPlanRhinoPanel v)
        {
            throw new NotImplementedException();
        }
    }
}