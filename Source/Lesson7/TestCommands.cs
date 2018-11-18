using Autodesk.AutoCAD.Runtime;
using System;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("ShowModelDialog")]
        public void cmdShowModelDialog()
        {
            var frm = new FormModal();
            AcApp.ShowModalDialog(frm);
        }

        [CommandMethod("ShowInteractionModelDialog")]
        public void cmdShowInteractionModelDialog()
        {
            var frm = new FormModalInteration(); 
            AcApp.ShowModalDialog(frm);
        }
        
        [CommandMethod("ShowModelessDialog")]
        public void cmdShowModelessDialog()
        {
            var frm = new FormModaless();
            AcApp.ShowModelessDialog(frm);
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

