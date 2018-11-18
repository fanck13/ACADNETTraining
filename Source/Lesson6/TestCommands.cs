using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Linq;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("GetXRecordData")]
        public void cmdGetXRecordData()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetEntity("\nPlease select entity");
            if (result.Status != PromptStatus.OK) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var entity = tr.GetObject(result.ObjectId, OpenMode.ForRead) as Entity;
                var extDictId = entity.ExtensionDictionary;
                if (extDictId.IsNull) return;

                var extDict = tr.GetObject(extDictId, OpenMode.ForRead) as DBDictionary;
                if (!extDict.Contains("MyData")) return;

                var xrecordId = extDict.GetAt("MyData");
                var xrecord = tr.GetObject(xrecordId, OpenMode.ForRead) as Xrecord;
                var data = xrecord.Data;
                ed.WriteMessage(string.Format("\nXrecord data : {0}", data.AsArray().First().Value));
            }
        }

        [CommandMethod("AddXRecordData")]
        public void cmdAddXRecordData()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetEntity("\nPlease select an entity to store data");
            if (result.Status != PromptStatus.OK) return;
            var result2 = ed.GetInteger("\nPlease input value");
            if (result2.Status != PromptStatus.OK) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var entity = tr.GetObject(result.ObjectId, OpenMode.ForWrite) as Entity;
                if (entity.ExtensionDictionary.IsNull)
                {
                    entity.CreateExtensionDictionary();
                }
                var extDictId = entity.ExtensionDictionary;
                var extDict = tr.GetObject(extDictId, OpenMode.ForWrite) as DBDictionary;
                var xrecord = new Xrecord();
                xrecord.Data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, result2.Value));                
                extDict.SetAt("MyData", xrecord);
                tr.AddNewlyCreatedDBObject(xrecord, true);

                tr.Commit();
            }
        }

        [CommandMethod("AddDataTableData")]
        public void cmdAddDataTableData()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var result = ed.GetEntity("\nPlease select an entity to store data");
            if (result.Status != PromptStatus.OK) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var entity = tr.GetObject(result.ObjectId, OpenMode.ForWrite) as Entity;
                if (entity.ExtensionDictionary.IsNull)
                {
                    entity.CreateExtensionDictionary();
                }
                var extDictId = entity.ExtensionDictionary;
                var extDict = tr.GetObject(extDictId, OpenMode.ForWrite) as DBDictionary; 

                var dataTable = new DataTable();
                dataTable.AppendColumn(CellType.Integer, "IntValue");
                dataTable.AppendColumn(CellType.CharPtr, "Text");
                var rowData = new DataCellCollection();
                var intValue = new DataCell();
                intValue.SetInteger(123);
                rowData.Add(intValue);
                var textValue = new DataCell();
                textValue.SetString("this is a text");
                rowData.Add(textValue);
                dataTable.AppendRow(rowData, true);

                extDict["MyData"] = dataTable;
                tr.AddNewlyCreatedDBObject(dataTable, true);

                tr.Commit();
            }
        }
    }
}

