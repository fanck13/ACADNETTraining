using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using PX.Common.PxNetwork;
using System.Collections.Generic;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using MoreLinq;
using System.Linq;

namespace PX.Common
{
    public class PluginApp : IExtensionApplication
    {
        #region IExtensionApplication Members

        public void Initialize()
        {
            AcApp.DocumentManager.DocumentCreated += DocumentManager_DocumentCreated;
            AcApp.DocumentManager.DocumentDestroyed += DocumentManager_DocumentDestroyed;
        }

        //每个Document被创建的时候，我们需要注册Event监控命令的执行
        //例如PxImportSHPToPipeNetwork,这些命令创建的 通信 管道，我们需要开始监控他的Size，Size变化需要自动刷新Module孔数
        private void DocumentManager_DocumentCreated(object sender, Autodesk.AutoCAD.ApplicationServices.DocumentCollectionEventArgs e)
        {
            var doc = e.Document;
            Assertion.Assert(doc != null);
            var db = doc.Database;

            doc.CommandWillStart += Doc_CommandWillStart;
            doc.CommandEnded += Doc_CommandEnded;
            doc.CommandCancelled += Doc_CommandCancelled;
            doc.CommandFailed += Doc_CommandFailed;

            var TXPipeIds = new List<ObjectId>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var networkIds = Utils.FindEntities<Network>(db);
                if (!networkIds.Any()) return;

                //规划通信 和 现状通信
                var plIds_GHTX = NetworkUtils.GetPartListInformation()[(int)GHPipeType.TXG].GetPartListObjectIds();
                var plIds_XZTX = NetworkUtils.GetPartListInformation()[(int)XZPipeType.TXG].GetPartListObjectIds();
                //取不到这些PartList，说明这个图不符合我们的要求，就不用继续处理了
                if (plIds_GHTX == null || plIds_XZTX == null) return;

                var TXPartListIds = new List<ObjectId> { plIds_GHTX.PartListId, plIds_GHTX.PartListId };

                foreach (ObjectId networkId in networkIds)
                {
                    var network = tr.GetObject(networkId, OpenMode.ForRead) as Network;
                    if (!TXPartListIds.Contains(network.PartsListId)) continue; //不是通信管就不管了

                    var pipeIds = network.GetPipeIds();
                    TXPipeIds.AddRange(pipeIds.ToList());
                }
                tr.Commit();
            }

            var pipeSizeMonitor = PipeSizeMonitorManager.GetInstance(doc);
            TXPipeIds.ForEach(i => pipeSizeMonitor.StartMonitor(i, false));
        }

        private void DocumentManager_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            var doc = (sender as DocumentCollection).CurrentDocument;
            Assertion.Assert(doc != null);

            try
            {
                doc.CommandWillStart -= Doc_CommandWillStart;
                doc.CommandEnded -= Doc_CommandEnded;
            }
            catch (System.Exception ex)
            {
                Utils.WriteDebugMessage(ex);
            }
        }

        private List<ObjectId> _TXPipeIds = new List<ObjectId>();
        private List<string> _TXCommandNames = new List<string>
        {
            "aecccreatenetwork",
            "aecccreatenetworkfromobject",
            "aecclayoutpipeonly",
            "aecclayoutpipeandstructure",
            "aecclayoutstructureonly",
            "pxconvertpolylinetonetwork",
            "pxconvertpolylinetonetworkhidelayers",
            "pximportshptopipenetwork",
            "pxbreakpipe",
            "pxbreakpipeex",
            "pximportnetworksfromexcel",
            "pxxzgd"
        };
        
        private void Doc_CommandWillStart(object sender, CommandEventArgs e)
        {
            //记录下来当前的通信管道
            var cmdName = e.GlobalCommandName.ToLower();
            if (!_TXCommandNames.Contains(cmdName)) return;

            var doc = sender as Document;
            Assertion.Assert(doc != null);
            var db = doc.Database;

            _TXPipeIds.Clear();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var networkIds = Utils.FindEntities<Network>(db);
                if (!networkIds.Any()) return;

                //规划通信 和 现状通信
                var plIds_GHTX = NetworkUtils.GetPartListInformation()[(int)GHPipeType.TXG].GetPartListObjectIds();
                var plIds_XZTX = NetworkUtils.GetPartListInformation()[(int)XZPipeType.TXG].GetPartListObjectIds();
                //取不到这些PartList，说明这个图不符合我们的要求，就不用继续处理了
                if (plIds_GHTX == null || plIds_XZTX == null) return;

                var TXPartListIds = new List<ObjectId> { plIds_GHTX.PartListId, plIds_GHTX.PartListId };

                foreach (ObjectId networkId in networkIds)
                {
                    var network = tr.GetObject(networkId, OpenMode.ForRead) as Network;
                    if (!TXPartListIds.Contains(network.PartsListId)) continue; //不是通信管就不管了

                    var pipeIds = network.GetPipeIds();
                    _TXPipeIds.AddRange(pipeIds.ToList());
                }
            }
        }

        private void OnCommandEnded(Document doc, string globalName)
        {
            // 比较之前保存的通信管道，对于新增加的通信管道，添加监控
            //通信管道的判断标准是 使用了 规划或现有 通信管道的PartList
            if (!_TXCommandNames.Contains(globalName)) return;
            var db = doc.Database;

            var TXPipeIds = new List<ObjectId>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var networkIds = Utils.FindEntities<Network>(db);
                if (!networkIds.Any()) return;

                //规划通信 和 现状通信
                var plIds_GHTX = NetworkUtils.GetPartListInformation()[(int)GHPipeType.TXG].GetPartListObjectIds();
                var plIds_XZTX = NetworkUtils.GetPartListInformation()[(int)XZPipeType.TXG].GetPartListObjectIds();
                //取不到这些PartList，说明这个图不符合我们的要求，就不用继续处理了
                if (plIds_GHTX == null || plIds_XZTX == null) return;

                var TXPartListIds = new List<ObjectId> { plIds_GHTX.PartListId, plIds_GHTX.PartListId };

                foreach (ObjectId networkId in networkIds)
                {
                    var network = tr.GetObject(networkId, OpenMode.ForRead) as Network;
                    if (!TXPartListIds.Contains(network.PartsListId)) continue; //不是通信管就不管了

                    var pipeIds = network.GetPipeIds();
                    TXPipeIds.AddRange(pipeIds.ToList());
                }
            }

            var newTXIds = TXPipeIds.Except(_TXPipeIds);
            if (!newTXIds.Any()) return;

            var pipeSizeMonitor = PipeSizeMonitorManager.GetInstance(doc);
            newTXIds.ForEach(i => pipeSizeMonitor.StartMonitor(i, true));
        }

        private void Doc_CommandEnded(object sender, CommandEventArgs e)
        {
            var cmdName = e.GlobalCommandName.ToLower();
            if (!_TXCommandNames.Contains(cmdName)) return;
            var doc = (sender as Document);
            Assertion.Assert(doc != null);

            OnCommandEnded(doc, cmdName);
        }

        private void Doc_CommandCancelled(object sender, CommandEventArgs e)
        {
            var cmdName = e.GlobalCommandName.ToLower();
            if (!_TXCommandNames.Contains(cmdName)) return;
            var doc = (sender as Document);
            Assertion.Assert(doc != null);

            OnCommandEnded(doc, cmdName);
        }

        private void Doc_CommandFailed(object sender, CommandEventArgs e)
        {
            var cmdName = e.GlobalCommandName.ToLower();
            if (!_TXCommandNames.Contains(cmdName)) return;
            var doc = (sender as Document);
            Assertion.Assert(doc != null);

            OnCommandEnded(doc, cmdName);
        }

        public void Terminate()
        {
        }

        #endregion
    }
}
