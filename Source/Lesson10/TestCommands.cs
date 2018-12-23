using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Serialization;


[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class Circle
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int radius { get; set; }
    }

    public class RootObject
    {
        public List<Circle> circles { get; set; }
    }

    public class Circles
    {
        List<Circle> circleList = new List<Circle>();

        [XmlElement(ElementName = "Circle")]
        public List<Circle> CircleList
        {
            get { return circleList; }
            set { circleList = value; }
        }
    }

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

        public static List<ObjectId> GetBlockIds(Database db)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var names = bt.Cast<ObjectId>()
                         .Where(i => !SymbolUtilityServices.IsBlockLayoutName((tr.GetObject(i, OpenMode.ForRead) as BlockTableRecord).Name)
                                     && i != SymbolUtilityServices.GetBlockModelSpaceId(db))
                         .ToList()
                         .Select(i => (tr.GetObject(i, OpenMode.ForRead) as BlockTableRecord).Name).ToList();

                return bt.Cast<ObjectId>()
                         .Where(i => !SymbolUtilityServices.IsBlockLayoutName((tr.GetObject(i, OpenMode.ForRead) as BlockTableRecord).Name)
                                     && i != SymbolUtilityServices.GetBlockModelSpaceId(db))
                         .ToList();
            }
        }

        private static void ImportBlocksFromExternalDrawing(Database db, string externalDwgPath)
        {
            var srcDb = new Database();
            srcDb.ReadDwgFile(externalDwgPath, FileOpenMode.OpenForReadAndReadShare, false, "");
            var blockIds = GetBlockIds(srcDb);

            var blockIdCol = new ObjectIdCollection();
            blockIds.ForEach(i => blockIdCol.Add(i));

            var idMapping = new IdMapping();
            db.WblockCloneObjects(blockIdCol, db.BlockTableId, idMapping, DuplicateRecordCloning.Ignore, false);
        }

        private static void CreateBlockReference(Transaction tr, BlockTableRecord ms, ObjectId blockId, Point3d insertPt)
        {
            var br = new BlockReference(insertPt, blockId);
            var id = ms.AppendEntity(br);
            tr.AddNewlyCreatedDBObject(br, true);
        }

        [CommandMethod("AssemblyFromBlocks")]
        public void cmdAssemblyFromBlocks()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*|DWG files (*.dwg)|*.dwg";
            openFileDialog.FilterIndex = 2;
            if (openFileDialog.ShowDialog() != true) return;
            var filePath = openFileDialog.FileName;

            ImportBlocksFromExternalDrawing(db, filePath);

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var ms = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                var blockVLId = bt["vl"];
                var blockVSId = bt["vs"];
                var blockHLId = bt["hl"];
                var blockHSId = bt["hs"];

                CreateBlockReference(tr, ms, blockVLId, new Point3d(0, 0, 0));
                CreateBlockReference(tr, ms, blockVLId, new Point3d(300, 0, 0));
                CreateBlockReference(tr, ms, blockVSId, new Point3d(700, 0, 0));
                CreateBlockReference(tr, ms, blockVLId, new Point3d(0, 1600, 0));
                CreateBlockReference(tr, ms, blockVLId, new Point3d(300, 1600, 0));
                CreateBlockReference(tr, ms, blockVSId, new Point3d(700, 1600, 0));

                var level1Height = 100;
                var level2Height = 785;

                {
                    var hs = new BlockReference(new Point3d(0, 0, level1Height), blockHSId);
                    hs.TransformBy(Matrix3d.Rotation(90.0 / 180.0 * Math.PI, Vector3d.YAxis, new Point3d(0, 0, level1Height)));
                    var hs2 = hs.Clone() as BlockReference;
                    hs2.TransformBy(Matrix3d.Displacement(new Vector3d(0, 1600, 0)));
                    var hs3 = hs.Clone() as BlockReference;
                    hs3.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, level2Height - level1Height)));
                    var hs4 = hs2.Clone() as BlockReference;
                    hs4.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, level2Height - level1Height)));

                    ms.AppendEntity(hs);
                    tr.AddNewlyCreatedDBObject(hs, true);
                    ms.AppendEntity(hs2);
                    tr.AddNewlyCreatedDBObject(hs2, true);
                    ms.AppendEntity(hs3);
                    tr.AddNewlyCreatedDBObject(hs3, true);
                    ms.AppendEntity(hs4);
                    tr.AddNewlyCreatedDBObject(hs4, true);
                }

                {
                    var hl = new BlockReference(new Point3d(0, 0, level1Height), blockHLId);
                    hl.TransformBy(Matrix3d.Rotation(-90.0 / 180.0 * Math.PI, Vector3d.XAxis, new Point3d(0, 0, level1Height)));
                    var hl2 = hl.Clone() as BlockReference;
                    hl2.TransformBy(Matrix3d.Displacement(new Vector3d(300, 0, 0)));
                    var hl3 = hl.Clone() as BlockReference;
                    hl3.TransformBy(Matrix3d.Displacement(new Vector3d(700, 0, 0)));
                    var hl4 = hl.Clone() as BlockReference;
                    hl4.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, level2Height - level1Height)));
                    var hl5 = hl2.Clone() as BlockReference;
                    hl5.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, level2Height - level1Height)));
                    var hl6 = hl3.Clone() as BlockReference;
                    hl6.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, level2Height - level1Height)));

                    ms.AppendEntity(hl);
                    tr.AddNewlyCreatedDBObject(hl, true);
                    ms.AppendEntity(hl2);
                    tr.AddNewlyCreatedDBObject(hl2, true);
                    ms.AppendEntity(hl3);
                    tr.AddNewlyCreatedDBObject(hl3, true);
                    ms.AppendEntity(hl4);
                    tr.AddNewlyCreatedDBObject(hl4, true);
                    ms.AppendEntity(hl5);
                    tr.AddNewlyCreatedDBObject(hl5, true);
                    ms.AppendEntity(hl6);
                    tr.AddNewlyCreatedDBObject(hl6, true);
                }


                tr.Commit();
            }
        }

        [CommandMethod("ImportFromJSON")]
        public void cmdImportFromJSON()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*|JSON files (*.json)|*.json";
            openFileDialog.FilterIndex = 2;
            if (openFileDialog.ShowDialog() != true) return;
            var filePath = openFileDialog.FileName;

            var jsonText = File.ReadAllText(filePath);
            RootObject ro = JsonConvert.DeserializeObject<RootObject>(jsonText);

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                foreach (var c in ro.circles)
                {
                    var circle = new Autodesk.AutoCAD.DatabaseServices.Circle(new Point3d(c.x, c.y, c.z), Vector3d.ZAxis, c.radius);
                    btr.AppendEntity(circle);
                    tr.AddNewlyCreatedDBObject(circle, true);
                }

                tr.Commit();
            }
        }

        [CommandMethod("ImportFromXML")]
        public void cmdImportFromXML()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*|XML files (*.xml)|*.xml";
            openFileDialog.FilterIndex = 2;
            if (openFileDialog.ShowDialog() != true) return;
            var filePath = openFileDialog.FileName;

            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            XmlSerializer xmlSearializer = new XmlSerializer(typeof(Circles));
            Circles circles = (Circles)xmlSearializer.Deserialize(file);
            file.Close();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                foreach (var c in circles.CircleList)
                {
                    var circle = new Autodesk.AutoCAD.DatabaseServices.Circle(new Point3d(c.x, c.y, c.z), Vector3d.ZAxis, c.radius);
                    btr.AppendEntity(circle);
                    tr.AddNewlyCreatedDBObject(circle, true);
                }

                tr.Commit();
            }
        }

        [CommandMethod("TestUCS")]
        public void cmdTestUCS()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var ms = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var ucs = new CoordinateSystem3d(new Point3d(11, 22, 0), Vector3d.XAxis, Vector3d.YAxis);

                //UCS到WCS的转换矩阵
                var matrix = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis,
                                                            ucs.Origin, ucs.Xaxis, ucs.Yaxis, ucs.Zaxis);

                ed.CurrentUserCoordinateSystem = matrix;

                //API中加进去的都是WCS的坐标，我们构造的时候是通过UCS里的坐标值来构造的，真正加进去之前需要转换到WCS
                var circle = new Autodesk.AutoCAD.DatabaseServices.Circle();
                circle.Center = new Point3d(10, 10, 0);
                circle.Radius = 5;
                circle.TransformBy(matrix);

                ms.AppendEntity(circle);
                tr.AddNewlyCreatedDBObject(circle, true);

                tr.Commit();
            }
        }
    }
}

