using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using System.Linq;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ACADPlugin
{
    public class PluginApp : IExtensionApplication
    {
        #region IExtensionApplication Members

        public void Initialize()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            doc.Database.ObjectAppended += Database_ObjectAppended;
        }

        void Database_ObjectAppended(object sender, ObjectEventArgs e)
        {
            var obj = e.DBObject;
            if (obj is Entity)
                AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format("\nObject added {0}\n", obj.GetRXClass().Name));
        }

        public void Terminate()
        {
        }

        #endregion
    }
}
