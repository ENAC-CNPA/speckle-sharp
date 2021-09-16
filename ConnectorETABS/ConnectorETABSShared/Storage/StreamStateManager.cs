﻿using System;
using Speckle.ConnectorETABS.Util;
using System.Collections.Generic;
using Speckle.Core.Logging;
using Speckle.DesktopUI.Utils;
using Speckle.Newtonsoft.Json;
using System.IO;
using System.Text;
using Objects.Converter.ETABS;

namespace ConnectorETABS.Storage
{
    public static class StreamStateManager
    {
        private static string _speckleFilePath;
        public static List<StreamState> ReadState(ConnectorETABSDocument doc)
        {
            Tracker.TrackPageview(Tracker.DESERIALIZE);
            var strings = ReadSpeckleFile(doc);
            if (strings == "")
            {
                return new List<StreamState>();
            }
            try
            {
                Tracker.TrackPageview(Tracker.DESERIALIZE);
                return JsonConvert.DeserializeObject<List<StreamState>>(strings);
            }
            catch
            {
                return new List<StreamState>();
            }
        }

        /// <summary>
        /// Writes the stream states to the <ETABSModelName>.txt file in speckle folder
        /// that exists or is created in the folder where the etabs model exists.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="streamStates"></param>
        public static void WriteStreamStateList(ConnectorETABSDocument doc, List<StreamState> streamStates)
        {
            if (_speckleFilePath == null) GetOrCreateSpeckleFilePath(doc);
            FileStream fileStream = new FileStream(_speckleFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(streamStates) as string);
                    streamWriter.Flush();
                    Tracker.TrackPageview(Tracker.SERIALIZE);
                }
            }
            catch { }
        }

        /// <summary>
        /// We need a folder in etabs model folder named "speckle" and a file in it
        /// called "<ETABSModelName>.txt". This function create this file and folder if
        /// they doesn't exists and returns it, otherwise just returns the file path
        /// </summary>
        /// <param name="doc"></param>
        private static void GetOrCreateSpeckleFilePath(ConnectorETABSDocument doc)
        {
            string etabsModelfilePath = doc.Document.GetModelFilename(true);
            if (etabsModelfilePath == "")
            {
                // etabs model is probably not saved, so speckle shouldn't do much
                _speckleFilePath = null;
                return;
            }
            string etabsFileName = Path.GetFileNameWithoutExtension(etabsModelfilePath);
            string etabsModelFolder = Path.GetDirectoryName(etabsModelfilePath);
            string speckleFolderPath = Path.Combine(etabsModelFolder, "speckle");
            string speckleFilePath = Path.Combine(etabsModelFolder, "speckle", $"{etabsFileName}.txt");
            try
            {
                if (!Directory.Exists(speckleFolderPath))
                {
                    Directory.CreateDirectory(speckleFolderPath);
                }
                if (!File.Exists(speckleFilePath))
                {
                    File.CreateText(speckleFilePath);
                }
                _speckleFilePath = speckleFilePath;
            }
            catch
            {
                _speckleFilePath = null;
                return;
            }
        }

        /// <summary>
        /// Reads the "/speckle/<ETABSModelName>.txt" file and returns the string in it
        /// </summary>
        /// <param name="doc"></param>
        private static string ReadSpeckleFile(ConnectorETABSDocument doc)
        {
            if (_speckleFilePath == null)
                GetOrCreateSpeckleFilePath(doc);

            if (_speckleFilePath == null) return "";
            FileStream fileStream = new FileStream(_speckleFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            try
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch { return ""; }
        }

    }
}
