using System;
using System.Collections.Generic;
using System.Linq;

using DesktopUI2.Models;
using Speckle.Newtonsoft.Json;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.TX.Undo;
using Speckle.ConnectorTopSolid.DB.Operations;
using TopSolid.Kernel.SX.Globalization;
using TopSolid.Kernel.DB.Operations;
using Avalonia.Data.Core;

namespace Speckle.ConnectorTopSolid.UI.Storage
{
    /// <summary>
    /// Manages the serialisation of speckle stream state
    /// </summary>
    /// <remarks>
    /// Uses a child dictionary for custom data in the Named Object Dictionary (NOD) which is the root level dictionary.
    /// This is because NOD persists after a document is closed (unlike file User Data).
    /// Custom data is stored as XRecord key value entries of type (string, ResultBuffer).
    /// ResultBuffers are TypedValue arrays, with the DxfCode of the input type as an integer.
    /// Used for DesktopUI2
    /// </remarks>
    public static class SpeckleStreamManager
    {
        // readonly static string SpeckleExtensionDictionary = "Speckle";

        /// <summary>
        /// Returns all the speckle stream states present in the current document.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<StreamState> ReadState(ModelingDocument doc)
        {
            var streams = new List<StreamState>();

            if (doc == null)
                return streams;

            var op = doc.RootOperation.DeepConstituents.Where(x => x.Name == ConnectorBindingsTopSolid.streamName).FirstOrDefault();
            if (op != null)
            {
                SpeckleFolderOperation sop = op as SpeckleFolderOperation;
                streams = sop.states;

            }

            return streams;
        }

        /// <summary>
        /// Writes the stream states to the current document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="wrap"></param>
        public static void WriteStreamStateList(TopSolid.Kernel.DB.Documents.Document doc, List<StreamState> streamStates)
        {
            if (doc == null)
                return;

            //if (doc.IsReadOnly) doc.IsReadOnly = false;
            doc.EnsureIsDirty();


            try
            {
                ConnectorBindingsTopSolid.SetStreamName(streamStates.FirstOrDefault().CachedStream);

                Element opExist = doc.Elements[ConnectorBindingsTopSolid.streamName];
                if (opExist != null)
                {
                    SpeckleFolderOperationReceive op = opExist as SpeckleFolderOperationReceive;
                    //if (op.states.Count > 0)
                    //{
                    Console.WriteLine("Stream existing");
                    //}
                } else
                {

                    SpeckleFolderOperation scor = new SpeckleFolderOperation(doc, 0)
                    {
                        states = streamStates,
                        Name = ConnectorBindingsTopSolid.streamName
                    };
                    scor.Create();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error create State stream : " + ex.Message);
            }
       

        }


        /// <summary>
        /// Returns commit info present in the current document.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string ReadCommit(ModelingDocument doc)
        {
            string commit = null;

            if (doc == null)
                return null;

            var op = doc.RootOperation.DeepConstituents.Where(x => x.Name == ConnectorBindingsTopSolid.streamName).FirstOrDefault();
            if (op != null)
            {
                SpeckleFolderOperation sop = op as SpeckleFolderOperation;
                commit = sop.commit;

            }

            return commit;
        }

        /// <summary>
        /// Writes commit info to the current document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="wrap"></param>
        public static void WriteCommit(GeometricDocument doc, string _commit)
        {

            if (doc == null)
                return;


            //if (doc.IsReadOnly) doc.IsReadOnly = false;
            doc.EnsureIsDirty();

            var op = doc.RootOperation.DeepConstituents.Where(x => x.Name == ConnectorBindingsTopSolid.streamName).FirstOrDefault();
            if (op != null)
            {
                SpeckleFolderOperation sop = op as SpeckleFolderOperation;
                sop.commit = _commit;

            } else
            {
                    SpeckleFolderOperation scor = new SpeckleFolderOperation(doc, 0)
                    {
                        commit = _commit,
                        Name = ConnectorBindingsTopSolid.streamName
                    };
                    scor.Create();
            }

        }




    }
}
