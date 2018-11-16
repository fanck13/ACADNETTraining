using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;

namespace ACADPlugin
{
    public partial class PaletteUserControl : UserControl
    {
        public PaletteUserControl()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            using (var docLock = doc.LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                var ids = btr.Cast<ObjectId>().ToList();
                if (ids.Any())
                {
                    var ent = tr.GetObject(ids.First(), OpenMode.ForWrite) as Entity;
                    var rnd = new Random();
                    ent.Color = Autodesk.AutoCAD.Colors.Color.FromRgb((byte)rnd.Next(255),
                                                                      (byte)rnd.Next(255),
                                                                      (byte)rnd.Next(255));
                }

                tr.Commit();
            }
            ed.Regen();
        }
    }
}
