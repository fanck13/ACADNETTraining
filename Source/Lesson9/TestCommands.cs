using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System;
using System.IO;
using Autodesk.AutoCAD.Geometry;
using System.Drawing;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System.Windows.Forms;
using OfficeOpenXml;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("ImportFromExternalFile")]
        public void cmdImportFromExternalFile()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*|txt files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            var lines = File.ReadAllLines(openFileDialog.FileName);
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    var strCenter = parts[0];
                    var strRadius = parts[1];
                    var pointParts = strCenter.Split(',');
                    var ptCenter = new Point3d(int.Parse(pointParts[0]), int.Parse(pointParts[1]), int.Parse(pointParts[2]));
                    var radius = int.Parse(strRadius);

                    var circle = new Circle();
                    circle.Center = ptCenter;
                    circle.Radius = radius;

                    btr.AppendEntity(circle);
                    tr.AddNewlyCreatedDBObject(circle, true);
                }

                tr.Commit();
            }            
        }

        [CommandMethod("ExportToExternalFile")]
        public void cmdExternalFile()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "All files (*.*)|*.*|Excel files (*.xlsx)|*.xlsx";
            saveFileDialog.FilterIndex = 2;
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                using (var excelPackage = new ExcelPackage())
                {
                    var worksheet = excelPackage.Workbook.Worksheets.Add("NET Training");
                    worksheet.Cells[1, 1].Value = "圆心";
                    worksheet.Cells[1, 2].Value = "半径";

                    var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                    int index = 2;
                    foreach (ObjectId id in btr)
                    {
                        if (id.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(Circle))))
                        {
                            var circle = tr.GetObject(id, OpenMode.ForRead) as Circle;

                            worksheet.Cells[index, 1].Value = circle.Center.ToString();
                            worksheet.Cells[index, 2].Value = circle.Radius.ToString();
                            index++;
                        }
                    }   

                    try
                    {
                        excelPackage.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }
                    catch (System.Exception ex)
                    {
                        ed.WriteMessage(ex.ToString());
                    }
                }
            }   
        }
    }
}

