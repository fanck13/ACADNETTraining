using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using OfficeOpenXml;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("ImportTxtForAcCoreConsole")]
        public void cmdImportTxtForAcCoreConsole()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var fileName = @"InputFile.txt";
            var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(folderPath, fileName);

            var lines = File.ReadAllLines(filePath);
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    var xLength = double.Parse(parts[0]);
                    var yLength = double.Parse(parts[1]);
                    var zLength = double.Parse(parts[2]);

                    var box = new Solid3d();
                    box.CreateBox(xLength, yLength, zLength);

                    btr.AppendEntity(box);
                    tr.AddNewlyCreatedDBObject(box, true);
                }

                tr.Commit();
            }

            db.SaveAs(Path.Combine(folderPath, @"Generated.dwg"), DwgVersion.Current);
        }
    }
}

