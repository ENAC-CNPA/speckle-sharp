﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ConnectorGrashopper.Extras;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Speckle.Core.Api;
using Speckle.Core.Credentials;

namespace ConnectorGrashopper.Streams
{
    public class StreamDetailsComponent: GH_Component
    {
        public StreamDetailsComponent(): base("Stream Details", "sDet", "Extracts the details of a given stream","Speckle 2","Streams"){}
        
        public override Guid ComponentGuid => new Guid("B47CAD66-187C-4D1F-AC77-9CA03BF82A0E");
        
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stream/ID", "S", "A stream object or a unique ID of the stream to be updated.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Account", "A", "Account to update stream.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Stream ID", "ID", "Unique ID of the stream to be updated.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Name of the stream.", GH_ParamAccess.item);
            pManager.AddTextParameter("Description", "D", "Description of the stream", GH_ParamAccess.item);
            pManager.AddTextParameter("Created At", "C", "Date of creation", GH_ParamAccess.item);
            pManager.AddTextParameter("Updated At", "U", "Date when it was last modified", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Public", "P", "True if the stream is to be publicly available.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Collaborators", "Cs", "Users that have collaborated in this stream", GH_ParamAccess.list);
            pManager.AddGenericParameter("Branches", "B", "List of branches for this stream", GH_ParamAccess.list);
        }

        private Stream stream;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper streamInput = null;
            string accountId = null; 
            if (stream == null)
            {
                Account account;
                if (!DA.GetData(0, ref streamInput)) return;
                var streamWrapper = (StreamWrapper) streamInput.Value;
                
                Task.Run(async () =>
                {
                    Account account = streamInput.AccountId == null
                        ? AccountManager.GetDefaultAccount() 
                        : AccountManager.GetAccounts().FirstOrDefault(a => a.id == accountId);
                
                    var client = new Client(account);
                    stream = await client.StreamGet(streamWrapper.StreamId);
                    Rhino.RhinoApp.InvokeOnUiThread((Action)delegate
                    {
                        ExpireSolution(true);
                    });
                });
            }
            else
            {
                DA.SetData(0, stream.id);
                DA.SetData(1, stream.name);
                DA.SetData(2, stream.description);
                DA.SetData(3, stream.createdAt);
                DA.SetData(4, stream.updatedAt);
                DA.SetData(5, stream.isPublic);
                DA.SetDataList(6, stream.collaborators);
                DA.SetDataList(7, stream.branches.items);
                stream = null;
            }
            

        }
    }
}