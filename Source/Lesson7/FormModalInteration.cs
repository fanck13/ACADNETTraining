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
using Autodesk.AutoCAD.EditorInput;

namespace ACADPlugin
{
    public partial class FormModalInteration : Form
    {
        public FormModalInteration()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            using (var edUserInteraction = ed.StartUserInteraction(this))
            {
                var result = ed.GetPoint("Please select a point");
                if (result.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) return;

                edUserInteraction.End();
                lblPoint.Text = result.Value.ToString();
                this.Focus();
            }
        }
    }
}
