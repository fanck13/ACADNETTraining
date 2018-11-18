using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
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
    }
}

