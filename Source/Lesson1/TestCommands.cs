using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(ACADPlugin.TestCmd))]

namespace ACADPlugin
{
    public class TestCmd
    {
        [CommandMethod("helloworld")]
        public void cmdHelloWorld()
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Hello World!");
        }
    }
}

