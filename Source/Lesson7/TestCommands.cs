using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Drawing;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("ShowModelDialog")]
        public void cmdShowModelDialog()
        {
            var frm = new FormModal();
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
        }

        [CommandMethod("ShowInteractionModelDialog")]
        public void cmdShowInteractionModelDialog()
        {
            var frm = new FormModalInteration(); 
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(frm);
        }
        
        [CommandMethod("ShowModelessDialog")]
        public void cmdShowModelessDialog()
        {
            var frm = new FormModaless();
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessDialog(frm);
        }

        private static Autodesk.AutoCAD.Windows.PaletteSet s_ps = null;
        [CommandMethod("ShowPalette")]
        public void cmdShowPalette()
        {
            if (s_ps == null)
            {
                s_ps = new Autodesk.AutoCAD.Windows.PaletteSet("Demo", new Guid());
                s_ps.DockEnabled = Autodesk.AutoCAD.Windows.DockSides.Left;
                s_ps.Add("DemoTab", new PaletteUserControl() { Dock = System.Windows.Forms.DockStyle.Fill });
            }

            s_ps.Visible = true;
        }
    }
}

