using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.BoundaryRepresentation;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("CreateBox")]
        public void cmdCreateBox()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                var box = new Solid3d();
                box.CreateBox(100, 200, 300);

                var matrix = ed.CurrentUserCoordinateSystem;
                matrix = matrix * Matrix3d.Displacement(new Vector3d(111, 222, 333));
                box.TransformBy(matrix);

                btr.AppendEntity(box);
                tr.AddNewlyCreatedDBObject(box, true);

                tr.Commit();
            }
        }

        [CommandMethod("HighlightRegion")]
        public void cmdHighlightRegion()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var pso = new PromptSelectionOptions();
            pso.SingleOnly = true;
            pso.ForceSubSelections = true;
            var psr = ed.GetSelection(pso);
            if (psr.Status != PromptStatus.OK) return;

            var sSet = psr.Value;
            var sObj = sSet[0];
            var sSubObjs = sObj.GetSubentities();
            var regionSubId = sSubObjs[0].FullSubentityPath.SubentId;
            if (regionSubId.Type != SubentityType.Face) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var solid3d = tr.GetObject(sObj.ObjectId, OpenMode.ForWrite) as Solid3d;
                FullSubentityPath regionPath = new FullSubentityPath(new ObjectId[] { sObj.ObjectId }, regionSubId);
                solid3d.Highlight(regionPath, false);
                solid3d.SetSubentityColor(regionSubId, Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0));
                tr.Commit();
            }
        }

        [CommandMethod("GetSubEntities")]
        public void cmdGetSubEntities()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetEntity("\nPlease select a Box");
            if (result.Status != PromptStatus.OK) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var solid3d = tr.GetObject(result.ObjectId, OpenMode.ForWrite) as Solid3d;
                ObjectId[] entId = new ObjectId[] { solid3d.ObjectId };

                IntPtr pSubentityIdPE = solid3d.QueryX(AssocPersSubentityIdPE.GetClass(typeof(AssocPersSubentityIdPE)));

                AssocPersSubentityIdPE subentityIdPE = AssocPersSubentityIdPE.Create(pSubentityIdPE, false) as AssocPersSubentityIdPE;
                SubentityId[] edgeIds = subentityIdPE.GetAllSubentities(solid3d, SubentityType.Edge);

                foreach (SubentityId subentId in edgeIds)
                {
                    FullSubentityPath path = new FullSubentityPath(entId, subentId);
                    solid3d.Highlight(path, false);
                    solid3d.SetSubentityColor(path.SubentId, Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0));
                }

                tr.Commit();
            }
        }

        [CommandMethod("BooleanBoxes")]
        public void cmdBooleanBoxes()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result1 = ed.GetEntity("\nPlease select first Box");
            if (result1.Status != PromptStatus.OK) return;
            var result2 = ed.GetEntity("\nPlease select second Box");
            if (result2.Status != PromptStatus.OK) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var box1 = tr.GetObject(result1.ObjectId, OpenMode.ForWrite) as Solid3d;
                var box2 = tr.GetObject(result2.ObjectId, OpenMode.ForWrite) as Solid3d;
                box1.BooleanOperation(BooleanOperationType.BoolUnite, box2);

                tr.Commit();
            }
        }
    }
}

