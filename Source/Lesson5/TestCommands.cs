using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("AssociateEntities")]
        public void cmdAssociateEntities()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var res1 = ed.GetEntity("Please select first Circle");
            if (res1.Status != PromptStatus.OK) return;
            var res2 = ed.GetEntity("Please select second Circle");
            if (res2.Status != PromptStatus.OK) return;
            circle2Id = res2.ObjectId;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var circle1 = tr.GetObject(res1.ObjectId, OpenMode.ForWrite) as Circle;
                if (circle1 == null) return;

                circle1.Modified += circle1_Modified;
                tr.Commit();
            }
        }

        private ObjectId circle2Id = ObjectId.Null;
        void circle1_Modified(object sender, EventArgs e)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var circle1 = sender as Circle;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var circle2 = tr.GetObject(circle2Id, OpenMode.ForWrite) as Circle;
                circle2.Radius = circle1.Radius;

                tr.Commit();
            }
        }
    }
}

