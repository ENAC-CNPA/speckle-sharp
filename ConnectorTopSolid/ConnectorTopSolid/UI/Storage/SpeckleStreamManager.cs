﻿using System;
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
        readonly static string SpeckleStreamStates = "SpeckleStream"; // "StreamStates";
        readonly static string SpeckleCommit = "Commit";

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

            string parameterValue = "";
            Element element = doc.Elements[SpeckleStreamStates];
            if (element != null && element is TextParameterEntity parameter)
            {
                parameterValue = parameter.Value;
            }

            streams = JsonConvert.DeserializeObject<List<StreamState>>(parameterValue);

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

            string value = JsonConvert.SerializeObject(streamStates) as string;

            Element element = (TextParameterEntity)doc.Elements[SpeckleStreamStates];
            if (element != null && element is TextParameterEntity parameter)
            {
                try
                {
                    parameter.Value = value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error create State stream : " + ex.Message);
                }
            }
            else
            {
                try
                {
                    TextParameterEntity stateParameter = new TextParameterEntity(doc, 0, true);
                    stateParameter.Name = SpeckleStreamStates;
                    //stateParameter.ExplicitDescription = new LocalizableString(SpeckleStreamStates);
                    stateParameter.Value = value;
                    doc.ParametersFolderEntity.InsertEntity(0, stateParameter);

                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error create State stream : " + ex.Message);
                }
            }

            try
            {

                //FolderOperation folderOperation = new FolderOperation(doc, 0);
                //folderOperation.Name = SpeckleStreamStates + " : " + streamStates[0].Id;
                //folderOperation.Create();

                SpeckleCompositeOperationReceive scor = new SpeckleCompositeOperationReceive(ref doc, 0)
                {
                    states = streamStates,
                    Name = SpeckleStreamStates + " : " + 
                    streamStates[0].CachedStream.name + "-" + streamStates[0].BranchName + "-" + streamStates[0].CommitId
                };
                scor.Create();

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


            string parameterValue = "";
            Element element = doc.Elements[SpeckleCommit];
            if (element != null && element is TextParameterEntity parameter)
            {
                parameterValue = parameter.Value;
            }

            // TODO : Structure commit like Object
            commit = JsonConvert.DeserializeObject<string>(parameterValue);

            return commit;
        }

        /// <summary>
        /// Writes commit info to the current document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="wrap"></param>
        public static void WriteCommit(GeometricDocument doc, string commit)
        {

            if (doc == null)
                return;

         
            string value = "";
            if (commit != null && commit != "") value = JsonConvert.SerializeObject(commit) as string;

            //if (doc.IsReadOnly) doc.IsReadOnly = false;
            doc.EnsureIsDirty();


            Element element = doc.Elements[SpeckleCommit];
            if (element != null && element is TextParameterEntity parameter)
            {
                parameter.Value = value;
            }
            else
            {
                TextParameterEntity commitParameter = new TextParameterEntity(doc, 0);
                commitParameter.Name = SpeckleCommit;
                commitParameter.Create();
                commitParameter.Value = value;
            }

        }




    }
}
