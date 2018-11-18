using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;

namespace ACADPlugin
{
    public partial class FormModal : Form
    {
        public FormModal()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                var ids = btr.Cast<ObjectId>().ToList();
                if (ids.Any())
                {
                    var ent = tr.GetObject(ids.First(), OpenMode.ForWrite) as Entity;
                    ent.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);
                }

                tr.Commit();
            }

            ed.Regen();
        }
    }
}
