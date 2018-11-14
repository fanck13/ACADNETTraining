using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System;
using Autodesk.AutoCAD.Geometry;
using System.Drawing;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("ModifyCircle")]
        public void cmdModifyCircle()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetEntity("\nPlease select a Circle\n");
            if (result.Status != PromptStatus.OK) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var circle = tr.GetObject(result.ObjectId, OpenMode.ForWrite) as Circle;

                circle.Center = new Point3d(circle.Center.X + 100, circle.Center.Y + 50, circle.Center.Z);
                circle.Radius = circle.Radius * 2;
                circle.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);    //Red

                tr.Commit();
            }
        }
    }
}

