﻿using System;
using System.Collections.Generic;
using Speckle.DesktopUI;
using Speckle.DesktopUI.Utils;
using Speckle.Core.Models;
using Speckle.ConnectorETABS.Util;
using System.Timers;
using Objects.Converter.ETABS;

namespace Speckle.ConnectorETABS.UI
{
    public partial class ConnectorBindingsETABS : ConnectorBindings
    {
        public static ConnectorETABSDocument Doc { get; set; } = new ConnectorETABSDocument();
        public List<Exception> Exceptions { get; set; } = new List<Exception>();

        public Timer SelectionTimer;
        public List<StreamState> DocumentStreams { get; set; } = new List<StreamState>();


        public ConnectorBindingsETABS(ConnectorETABSDocument doc)
        {
            Doc = doc;
            SelectionTimer = new Timer(2000) { AutoReset = true, Enabled = true };
            SelectionTimer.Elapsed += SelectionTimer_Elapsed;
            SelectionTimer.Start();
        }

        private void SelectionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Doc == null)
            {
                return;
            }

            var selection = GetSelectedObjects();

            NotifyUi(new UpdateSelectionCountEvent() { SelectionCount = selection.Count });
            NotifyUi(new UpdateSelectionEvent() { ObjectIds = selection });
        }




        #region boilerplate
        public override string GetActiveViewName()
        {
            throw new NotImplementedException();
        }

        public override string GetDocumentId() => GetDocHash(Doc);

        private string GetDocHash(ConnectorETABSDocument doc) => Speckle.Core.Models.Utilities.hashString(doc.Document.GetModelFilepath() + doc.Document.GetModelFilename(), Utilities.HashingFuctions.MD5);

        public override string GetDocumentLocation() => Doc.Document.GetModelFilepath();

        public override string GetFileName() => Doc.Document.GetModelFilename();

        public override string GetHostAppName() => ConnectorETABSUtils.ETABSAppName;

        public override List<string> GetObjectsInView()
        {
            throw new NotImplementedException();
        }


        #endregion





    }
}
