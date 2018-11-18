using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("helloworld")]
        public void cmdHelloWorld()
        {
            AcApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Hello World!");
        }
    }
}

