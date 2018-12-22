using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.IO;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("ListModelSpaceInfo")]
        public void cmdListModelSpaceInfo()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database; 

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                foreach (ObjectId id in btr)
                {
                    var entity = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    ed.WriteMessage(string.Format("\nEntity type : {0}", entity.GetRXClass().Name));
                }

                //LINQ
                //btr.Cast<ObjectId>().ToList().ForEach(i => ed.WriteMessage(string.Format("\nEntity type : {0}", (tr.GetObject(i, OpenMode.ForRead) as Entity).GetRXClass().Name)));
            }
        }

        [CommandMethod("ListLayoutDictInfo")]
        public void cmdListLayoutDictInfo()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var layoutDict = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                foreach (var layoutEntry in layoutDict)
                {
                    var layout = tr.GetObject(layoutEntry.Value, OpenMode.ForRead) as Layout;
                    ed.WriteMessage(string.Format("\nLayout name : {0}", layout.LayoutName));
                }

                //LINQ
                //layoutDict.Cast<DBDictionaryEntry>().ToList().ForEach(i => ed.WriteMessage(string.Format("\nLayout name : {0}", (tr.GetObject(i.Value, OpenMode.ForRead)).GetRXClass().Name)));
            } 
        }

        [CommandMethod("CreateBlock")]
        public void cmdCreateBlock()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                var circle = new Circle();
                circle.Radius = 100;
                var block = new BlockTableRecord();
                block.Name = "ACircle";
                bt.Add(block);
                tr.AddNewlyCreatedDBObject(block, true);

                block.AppendEntity(circle);
                tr.AddNewlyCreatedDBObject(circle, true);

                tr.Commit();
            }
        }

        [CommandMethod("GetBlocks")]
        public void cmdGetBlocks()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database; 

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                foreach (ObjectId id in bt)
                {
                    var btr = tr.GetObject(id, OpenMode.ForRead) as BlockTableRecord;
                    if (id != SymbolUtilityServices.GetBlockModelSpaceId(db)        //Block must have a Name
                        && !SymbolUtilityServices.IsBlockLayoutName(btr.Name))
                    {
                        ed.WriteMessage(string.Format("\nBlock : {0}", btr.Name));
                    }
                }
            }
        }

        [CommandMethod("GetBlockReferences")]
        public void cmdGetBlockReferences()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                foreach (ObjectId id in btr)
                {
                    if (id.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(BlockReference))))
                    {
                        var br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                        var block = tr.GetObject(br.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                        ed.WriteMessage(string.Format("\nParent Block : {0}", block.Name));
                    }
                }
            }
        }

        [CommandMethod("CreateBlockReferences")]
        public void cmdCreateBlockReferences()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var blockId = ObjectId.Null;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                foreach (ObjectId id in bt)
                {
                    var btr = tr.GetObject(id, OpenMode.ForRead) as BlockTableRecord;
                    if (id != SymbolUtilityServices.GetBlockModelSpaceId(db)        //Block must have a Name
                        && !SymbolUtilityServices.IsBlockLayoutName(btr.Name))
                    {
                        blockId = id;
                        break;
                    }
                }
            }

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                for (int i = 0; i < 5; i++)
                {
                    var br = new BlockReference(new Point3d(i * 30, 0, 0), blockId);
                    btr.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);
                }

                tr.Commit();
            }
        }

        [CommandMethod("ImportBlockFromExternalDrawing")]
        public void cmdImportBlockFromExternalDrawing()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.ShowDialog();
            var srcPath = dlg.FileName;
            var srcDb = new Database();
            srcDb.ReadDwgFile(srcPath, FileOpenMode.OpenForReadAndReadShare, false, "");
            var ids = new ObjectIdCollection();
            using (var tr = srcDb.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(srcDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                foreach (ObjectId id in bt)
                {
                    var btr = tr.GetObject(id, OpenMode.ForRead) as BlockTableRecord;
                    if (id != SymbolUtilityServices.GetBlockModelSpaceId(db)        //Block must have a Name
                        && !SymbolUtilityServices.IsBlockLayoutName(btr.Name))
                    {
                        ids.Add(id);
                    }
                }
            }

            var idmap = new IdMapping();
            db.WblockCloneObjects(ids, db.BlockTableId, idmap, DuplicateRecordCloning.Ignore, false);
        }
    }
}

