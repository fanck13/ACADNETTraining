using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("CreateCircle")]
        public void cmdCreateCircle()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetPoint("Please select a point");
            if (result.Status != PromptStatus.OK) return;
            var ptCenter = result.Value;

            var result2 = ed.GetInteger("Please input radius");
            if (result2.Status != PromptStatus.OK) return;
            var radius = result2.Value;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var circle = new Circle(ptCenter, Vector3d.ZAxis, radius);
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                btr.AppendEntity(circle);
                tr.AddNewlyCreatedDBObject(circle, true);

                tr.Commit();
            }
        }

        [CommandMethod("DeleteEntity")]
        public void cmdDeleteEntity()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetEntity("Please select an entity to delete");
            if (result.Status != PromptStatus.OK) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var obj = tr.GetObject(result.ObjectId, OpenMode.ForWrite);
                obj.Erase();

                tr.Commit();
            }
        }

        [CommandMethod("ModifyCircle")]
        public void cmdModifyCircle()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
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

        [CommandMethod("MoveRotateLine")]
        public void cmdRotateLine()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetEntity("\nPlease select a Line\n");
            if (result.Status != PromptStatus.OK) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var line = tr.GetObject(result.ObjectId, OpenMode.ForWrite) as Line;

                line.TransformBy(Matrix3d.Displacement(new Vector3d(10, 20, 0)));
                line.TransformBy(Matrix3d.Rotation((45.0 / 180) * Math.PI, Vector3d.ZAxis, line.StartPoint));

                tr.Commit();
            }
        }
    }
}

