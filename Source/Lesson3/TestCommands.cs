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
        [CommandMethod("GetObjectIdWithPrompt")]
        public void cmdGetObjectIdWithPrompt()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetEntity("Please select an entity");
            if (result.Status != PromptStatus.OK) return;

            ed.WriteMessage(string.Format("\nType is {0}", result.ObjectId.ObjectClass.Name));
        }
        
        [CommandMethod("GetObjectIdFromHandle")]
        public void cmdGetObjectIdFromHandle()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetString("Please input handle");
            if (result.Status != PromptStatus.OK) return;

            var handle = new Handle(Convert.ToInt32(result.StringResult, 16));  //Convert from hex
            var oid = db.GetObjectId(false, handle, 0);

            ed.WriteMessage(string.Format("\nType is {0}", oid.ObjectClass.Name));
        }

        [CommandMethod("GetObjectIdFromBlockTable")]
        public void cmdGetObjectIdFromBlockTable()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                var ids = btr.Cast<ObjectId>().Where(i => i.ObjectClass == RXClass.GetClass(typeof(Circle)));

                if (ids.Any())
                {
                    ed.WriteMessage(string.Format("\nType is {0}", ids.First().ObjectClass.Name));
                }
            }           
        }
    }
}

