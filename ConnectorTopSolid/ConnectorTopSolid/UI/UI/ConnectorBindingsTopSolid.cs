using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopUI2;
using DesktopUI2.Models;
using DesktopUI2.Models.Filters;
using DesktopUI2.Models.Settings;
using DesktopUI2.ViewModels;
using DynamicData;
using Speckle.Core.Api;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.Core.Models.GraphTraversal;
using Speckle.Core.Transports;
using TopSolid.Cad.Design.DB;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Cad.Design.DB.Representations;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.PointClouds;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.D3.Sketches;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Families.Documents;
using TopSolid.Kernel.DB.Layers;
using TopSolid.Kernel.TX.Documents;
using TopSolid.Kernel.TX.Pdm;
using TopSolid.Kernel.TX.Undo;
using Application = TopSolid.Kernel.UI.Application;

namespace Speckle.ConnectorTopSolid.UI
{
  public partial class ConnectorBindingsTopSolid : ConnectorBindings
  {
    public static ModelingDocument Doc => Application.CurrentDocument as ModelingDocument;

    public static string streamName = null;

    public List<ApplicationObject> Preview { get; set; } = new List<ApplicationObject>();
    public Dictionary<string, Base> StoredObjects = new Dictionary<string, Base>();
    public Dictionary<string, int> LineTypeDictionary = new Dictionary<string, int>();

    private List<ISetting> CurrentSettings { get; set; } // used to store the Stream State settings when sending/receiving

    // TopSolid API should only be called on the main thread.
    // Not doing so results in botched conversions for any that require adding objects to Document model space before modifying (eg adding vertices and faces for meshes)
    // There's no easy way to access main thread from document object, therefore we are creating a control during Connector Bindings constructor (since it's called on main thread) that allows for invoking worker threads on the main thread
    public System.Windows.Forms.Control Control;
    public ConnectorBindingsTopSolid() : base()
    {
      Control = new System.Windows.Forms.Control();
      Control.CreateControl();
    }


    public override List<ReceiveMode> GetReceiveModes()
    {
      return new List<ReceiveMode> { ReceiveMode.Update, ReceiveMode.Create, ReceiveMode.Ignore };
    }

    public override List<string> GetObjectsInView() // this returns all visible doc objects.
    {
      var objs = new List<string>();

      foreach (var item in Doc.Elements.GetAll())
      {
        objs.Add(item.Id.ToString());
      }
      return objs;
    }

    #region local streams 
    public override void WriteStreamsToFile(List<StreamState> streams)
    {

      Storage.SpeckleStreamManager.WriteStreamStateList(Doc, streams);
    }

    public override List<StreamState> GetStreamsInFile()
    {
      var states = new List<StreamState>(); //strings.Select(s => JsonConvert.DeserializeObject<StreamState>(Doc.Strings.GetValue(SpeckleKey, s))).ToList();
      return states;
    }
    #endregion

    #region boilerplate
    public override string GetHostAppNameVersion() => Utils.VersionedAppName.Replace("TopSolid", "TopSolid "); //hack for ADSK store;

    public override string GetHostAppName()
    {
      return Utils.Slug;
    }



    public override string GetDocumentId()
    {
      return Doc.Id.ToString();



    }

    public override string GetDocumentLocation() => null; // GetDocPath(Doc);


    public override string GetFileName()
    {
      return PdmClientStore.CurrentPdmClient.GetCurrentProject().GetName();
    }


    public override string GetActiveViewName()
    {
      return Doc.Name.ToString();
    }


    public override List<string> GetSelectedObjects()
    {

      IEnumerable<TopSolid.Kernel.DB.Elements.Element> elments = TopSolid.Kernel.UI.Selections.CurrentSelections.GetSelectedElements();
      List<string> elementsList = new List<string>();

      foreach (TopSolid.Kernel.DB.Elements.Element element in elments)
      {
        if (element is ShapeEntity || element is SketchEntity || element is PartEntity || element is TopSolid.Kernel.DB.D2.Sketches.SketchEntity)
          elementsList.Add(element.Id.ToString());
        else if (element is PointCloudEntity ptcE)
          elementsList.Add(ptcE.Id.ToString());
      }

      return elementsList;
    }

    private static List<Element> GetEverything(Dictionary<int, DesignDocument> linkedDocs)
    {
      var currentDoc = Doc;
      var selection = new List<Element>();      

      selection.AddRange(currentDoc.ShapesFolderEntity.Constituents);
      selection.AddRange(currentDoc.SketchesFolderEntity.Constituents);

     

      return selection;
    }



    public override List<ISelectionFilter> GetSelectionFilters()
    {
      var layers = new List<string>();
      if (Doc.LayersFolderEntity != null)
      {
        foreach (Entity entity in Doc.LayersFolderEntity.DeepEntities)
        {
          if (entity is LayerEntity)
          {
            layers.Add(entity.Name.ToString());
          }
        }
      }

      //representations
      var representations = new List<string>();
      RepresentationsFolderEntity representationFolder = (Doc as DesignDocument).RepresentationsFolderEntity;
      if (representationFolder != null)
      {
        foreach (Entity entity in representationFolder.DeepEntities)
        {
          if (entity is RepresentationEntity)
          {
            representations.Add(entity.GetFriendlyName().ToString());
          }
        }
      }

      return new List<ISelectionFilter>()
              {
                new ManualSelectionFilter(),
                new ListSelectionFilter {Slug="layer",  Name = "Layers", Icon = "LayersTriple",AllowMultipleSelection=true, Description = "Selects objects based on their layers.", Values = layers},
                new ListSelectionFilter {Slug="representation",  Name = "Representations", AllowMultipleSelection=false, Icon = "LayersTriple", Description = "Select objects based on a specific representation.", Values = representations},
                new AllSelectionFilter {Slug="all",  Name = "Everything", Icon = "CubeScan", Description = "Selects all document objects." }
              };
    }

    public override List<ISetting> GetSettings()
    {
      return new List<ISetting>();
    }

    //List<string> sn = new List<string>() { ConnectorBindingsTopSolid.streamName };
    //{
    //    new ListBoxSetting {Slug = "stream-infos", Name = "Stream infos", Icon ="Link", Values = sn, Selection = ConnectorBindingsTopSolid.streamName, Description = "Stream infos"}

    //};
    //TODO
    public override List<MenuItem> GetCustomStreamMenuItems()
    {
      return new List<MenuItem>();
    }

    public override void SelectClientObjects(List<string> args, bool deselect = false)
    {
      throw new NotImplementedException();
    }

    public override void ResetDocument()
    {
      // TODO!
    }

    //public override async Task<Dictionary<string, List<MappingValue>>> ImportFamilyCommand(Dictionary<string, List<MappingValue>> Mapping)
    //{
    //  await Task.Delay(TimeSpan.FromMilliseconds(500));
    //  return new Dictionary<string, List<MappingValue>>();
    //}

    #endregion

    #region receiving 
    public override bool CanPreviewReceive => false;
    public override Task<StreamState> PreviewReceive(StreamState state, ProgressViewModel progress)
    {
      return null;
    }

    public void SetSettings(ISpeckleConverter converter, Speckle.Core.Api.Stream stream)
    {
      Dictionary<string, string> settings = new Dictionary<string, string>();
      settings.Add("stream-name", SetStreamName(stream));
      converter.SetConverterSettings(settings);

    }

    public static string SetStreamName(Speckle.Core.Api.Stream stream)
    {
      ConnectorBindingsTopSolid.streamName = stream.name + " (" + stream.id + ")";
      return ConnectorBindingsTopSolid.streamName;
    }

    public override async Task<StreamState> ReceiveStream(StreamState state, ProgressViewModel progress)
    {
      var kit = KitManager.GetDefaultKit();
      var converter = kit.LoadConverter(Utils.VersionedAppName);
      if (converter == null)
        throw new Exception("Could not find any Kit!");
      var transport = new ServerTransport(state.Client.Account, state.StreamId);

      var stream = await state.Client.StreamGet(state.StreamId);


      try
      {

        // Start undo Sequence // inSequence
        UndoSequence.Start("SpeckleCreationReceive", false); // no Ghost

        SetSettings(converter, stream); // Send to Converter settings (streamName, etc)

        if (progress.CancellationTokenSource.Token.IsCancellationRequested)
          return null;

        if (Doc == null)
        {
          progress.Report.LogOperationError(new Exception($"No Document is open."));
          progress.CancellationTokenSource.Cancel();
        }

        //if "latest", always make sure we get the latest commit when the user clicks "receive"
        Commit commit = null;
        if (state.CommitId == "latest")
        {
          var res = await state.Client.BranchGet(progress.CancellationTokenSource.Token, state.StreamId, state.BranchName, 1);
          commit = res.commits.items.FirstOrDefault();
        }
        else
        {
          commit = await state.Client.CommitGet(progress.CancellationTokenSource.Token, state.StreamId, state.CommitId);
        }
        string referencedObject = commit.referencedObject;
        Base commitObject = null;
        try
        {
          commitObject = await Operations.Receive(
            referencedObject,
            progress.CancellationTokenSource.Token,
            transport,
            onProgressAction: dict => progress.Update(dict),
            onErrorAction: (s, e) =>
            {
              progress.Report.LogOperationError(e);
              progress.CancellationTokenSource.Cancel();
            },
            onTotalChildrenCountKnown: count => { progress.Max = count; },
            disposeTransports: true
            );

          await state.Client.CommitReceived(new CommitReceivedInput
          {
            streamId = stream?.id,
            commitId = commit?.id,
            message = commit?.message,
            sourceApplication = Utils.VersionedAppName
          });
        }
        catch (Exception e)
        {
          progress.Report.OperationErrors.Add(new Exception($"Could not receive or deserialize commit: {e.Message}"));
        }
        if (progress.Report.OperationErrorsCount != 0 || commitObject == null)
          return state;

        // invoke conversions on the main thread via control
        if (Control.InvokeRequired)
          Control.Invoke(new ReceivingDelegate(ConvertReceiveCommit), new object[] { commitObject, converter, state, progress, stream, commit.id });
        else
          ConvertReceiveCommit(commitObject, converter, state, progress, stream, commit.id);

        UndoSequence.End();
        //Doc.Update(true, true); // TODO : Move end of global process

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        // TODO : Message box
        UndoSequence.UndoCurrent(); // Cancel
      }

      return state;
    }

    delegate void ReceivingDelegate(Base commitObject, ISpeckleConverter converter, StreamState state, ProgressViewModel progress, Speckle.Core.Api.Stream stream, string id);
    private void ConvertReceiveCommit(Base commitObject, ISpeckleConverter converter, StreamState state, ProgressViewModel progress, Speckle.Core.Api.Stream stream, string id)
    {
      try
      {
        // set the context doc for conversion - this is set inside the transaction loop because the converter retrieves this transaction for all db editing when the context doc is set!

        converter.SetContextDocument(Doc);
        converter.ReceiveMode = state.ReceiveMode;

        // set converter settings as tuples (setting slug, setting selection)
        var settings = new Dictionary<string, string>();
        CurrentSettings = state.Settings;
        foreach (var setting in state.Settings)
          settings.Add(setting.Slug, setting.Selection);
        converter.SetConverterSettings(settings);

        // keep track of conversion progress here
        progress.Report = new ProgressReport();
        var conversionProgressDict = new ConcurrentDictionary<string, int>();
        conversionProgressDict["Conversion"] = 0;

        // create a commit prefix: used for layers and block definition names
        var commitPrefix = DesktopUI2.Formatting.CommitInfo(stream.name, state.BranchName, id);

        // give converter a way to access the commit info
        if (commitPrefix != null)
          Storage.SpeckleStreamManager.WriteCommit(Doc, commitPrefix);


        // delete existing commit layers
        try
        {
          //DeleteBlocksWithPrefix(commitPrefix, tr);
          //DeleteLayersWithPrefix(commitPrefix, tr);
        }
        catch
        {
          converter.Report.LogOperationError(
            new Exception(
              $"Failed to remove existing layers or blocks starting with {commitPrefix} before importing new geometry."
            )
          );
        }

        // clear previously stored objects
        StoredObjects.Clear();

        // TODO : Migration to FlattenCommitObject
        var commitObjs = FlattenCommitObject(commitObject, converter);



        // flatten the commit object to retrieve children objs
        //int count = 0;


        // TODO TopSolid Add LineType 
        // Get doc line types for bake: more efficient this way than doing this per object
        //LineTypeDictionary.Clear();
        var lineTypeDictionary = new Dictionary<string, int>();
        //var lineTypeTable = (LinetypeTable)tr.GetObject(Doc.Database.LinetypeTableId, OpenMode.ForRead);
        //foreach (ObjectId lineTypeId in lineTypeTable)
        //{
        //  var linetype = (LinetypeTableRecord)tr.GetObject(lineTypeId, OpenMode.ForRead);
        //  LineTypeDictionary.Add(linetype.Name, lineTypeId);
        //}

        // conversion
        foreach (var commitObj in commitObjs)
        {
          // handle user cancellation
          if (progress.CancellationToken.IsCancellationRequested)
            return;

          if (commitObj.Convertible)
          {
            converter.Report.Log(commitObj); // Log object so converter can access
            try
            {
              commitObj.Converted = ConvertObject(commitObj, converter);
            }
            catch (Exception e)
            {
              commitObj.Log.Add($"Failed conversion: {e.Message}");
            }
          }
          else
          {
            foreach (var fallback in commitObj.Fallback)
            {
              try
              {
                fallback.Converted = ConvertObject(fallback, converter);
              }
              catch (Exception e)
              {
                commitObj.Log.Add($"Fallback {fallback.applicationId} failed conversion: {e.Message}");
              }
              commitObj.Log.AddRange(fallback.Log);
            }

          }

          // if the object wasnt converted, log fallback status
          if (commitObj.Converted == null || commitObj.Converted.Count == 0)
          {
            var convertedFallback = commitObj.Fallback.Where(o => o.Converted != null || o.Converted.Count > 0);
            if (convertedFallback != null && convertedFallback.Count() > 0)
              commitObj.Update(logItem: $"Creating with {convertedFallback.Count()} fallback values");
            else
              commitObj.Update(
                status: ApplicationObject.State.Failed,
                logItem: $"Couldn't convert object or any fallback values"
              );
          }


          // add to progress report
          progress.Report.Log(commitObj);
        }
        progress.Report.Merge(converter.Report);

        foreach (var commitObj in commitObjs)
        {

          // handle user cancellation
          if (progress.CancellationToken.IsCancellationRequested)
            return;

          // find existing doc objects if they exist
          var existingObjs = new List<int>();
          var layer = commitObj.Container;
          switch (state.ReceiveMode)
          {
            case ReceiveMode.Update: // existing objs will be removed if it exists in the received commit
                                     //existingObjs = ApplicationIdManager.GetObjectsByApplicationId(
                                     //  Doc,
                                     //  tr,
                                     //  commitObj.applicationId,
                                     //  fileNameHash
                                     //);
              break;
            default:
              layer = $"{commitPrefix}${commitObj.Container}";
              break;
          }


          // bake
          if (commitObj.Convertible)
          {
            BakeObject(commitObj, converter, layer, existingObjs);
            commitObj.Status = !commitObj.CreatedIds.Any()
              ? ApplicationObject.State.Failed
              : existingObjs.Count > 0
                ? ApplicationObject.State.Updated
                : ApplicationObject.State.Created;
          }
          else
          {
            foreach (var fallback in commitObj.Fallback)
              BakeObject(fallback, converter, layer, existingObjs, commitObj);
            commitObj.Status =
              commitObj.Fallback.Where(o => o.Status == ApplicationObject.State.Failed).Count()
              == commitObj.Fallback.Count
                ? ApplicationObject.State.Failed
                : existingObjs.Count > 0
                  ? ApplicationObject.State.Updated
                  : ApplicationObject.State.Created;
          }

          // log to progress report and update progress
          progress.Report.Log(commitObj);
          conversionProgressDict["Conversion"]++;
          progress.Update(conversionProgressDict);

        }

        // remove commit info from doc userdata
        //Doc.UserData.Remove("commit");




        //    // create the object's bake layer if it doesn't already exist
        //    (Base obj, string layerName) = commitObj;

        //    conversionProgressDict["Conversion"]++;
        //    progress.Update(conversionProgressDict);

        //    object converted = null;
        //    try
        //    {

        //        converted = converter.ConvertToNative(obj);

        //    }
        //    catch (Exception e)
        //    {
        //        progress.Report.LogConversionError(new Exception($"Failed to convert object {obj.id} of type {obj.speckle_type}: {e.Message}"));
        //        continue;
        //    }
        //    var convertedElement = converted as Element;
        //    if (convertedElement != null)
        //    {
        //        if (Utils.GetOrMakeLayer(layerName, Doc, out string cleanName))
        //        {
        //            // record if layer name has been modified
        //            if (!cleanName.Equals(layerName))
        //                changedLayerNames = true;

        //            var res = true; // convertedElement.Append(cleanName); TODO : Link to layer
        //            if (res)
        //            {
        //                // handle display - fallback to rendermaterial if no displaystyle exists
        //                Base display = obj[@"displayStyle"] as Base;
        //                if (display == null) display = obj[@"renderMaterial"] as Base;
        //                if (display != null) Utils.SetStyle(display, convertedElement, lineTypeDictionary);

        //            }
        //            else
        //            {
        //                progress.Report.LogConversionError(new Exception($"Failed to add converted object {obj.id} of type {obj.speckle_type} to the document."));
        //            }

        //        }
        //        else
        //            progress.Report.LogOperationError(new Exception($"Failed to create layer {layerName} to bake objects into."));
        //    }
        //    else if (converted == null)
        //    {
        //        progress.Report.LogConversionError(new Exception($"Failed to convert object {obj.id} of type {obj.speckle_type}."));
        //    }
        //}
        ////progress.Report.Merge(converter.Report); // TODO : write Merge info




        //if (changedLayerNames)
        //    progress.Report.Log($"Layer names were modified: one or more layers contained invalid characters {Utils.invalidChars}");

        // remove commit info from doc userdata
        Storage.SpeckleStreamManager.WriteCommit(Doc, null);
        Doc.Update(true, true);
        //UndoSequence.End();
      }
      catch (Exception ex)
      {
        Console.WriteLine("Convert receive Commit : " + ex.Message);
        // TODO : Message box
        //UndoSequence.UndoCurrent(); // Cancel
        //throw;
      }


    }


    // Recurses through the commit object and flattens it. Returns list of Base objects with their bake layers
    private List<ApplicationObject> FlattenCommitObject(Base obj, ISpeckleConverter converter)
    {
      //TODO: this implementation is almost identical to Rhino, we should try and extract as much of it as we can into Core
      void StoreObject(Base @base, ApplicationObject appObj)
      {
        if (StoredObjects.ContainsKey(@base.id))
          appObj.Update(logItem: "Found another object in this commit with the same id. Skipped other object"); //TODO check if we are actually ignoring duplicates, since we are returning the app object anyway...
        else
          StoredObjects.Add(@base.id, @base);
      }

      ApplicationObject CreateApplicationObject(Base current, string containerId)
      {
        ApplicationObject NewAppObj()
        {
          var speckleType = current.speckle_type
            .Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault();
          return new ApplicationObject(current.id, speckleType)
          {
            applicationId = current.applicationId,
            Container = containerId
          };
        }

        // skip if it is the base commit collection
        if (current.speckle_type.Contains("Collection") && string.IsNullOrEmpty(containerId))
          return null;

        //Handle convertable objects
        if (converter.CanConvertToNative(current))
        {
          var appObj = NewAppObj();
          appObj.Convertible = true;
          StoreObject(current, appObj);
          return appObj;
        }

        //Handle objects convertable using displayValues
        var fallbackMember = current["displayValue"] ?? current["@displayValue"];
        if (fallbackMember != null)
        {
          var appObj = NewAppObj();
          var fallbackObjects = GraphTraversal
            .TraverseMember(fallbackMember)
            .Select(o => CreateApplicationObject(o, containerId));
          appObj.Fallback.AddRange(fallbackObjects);

          StoreObject(current, appObj);
          return appObj;
        }

        return null;
      }

      string LayerId(TraversalContext context) => LayerIdRecurse(context, new StringBuilder()).ToString();
      StringBuilder LayerIdRecurse(TraversalContext context, StringBuilder stringBuilder)
      {
        if (context.propName == null)
          return stringBuilder;

        string objectLayerName = string.Empty;
        if (context.propName.ToLower() == "elements" && context.current.speckle_type.Contains("Collection"))
        {
          objectLayerName = context.current["name"] as string;
        }
        else if (context.propName.ToLower() != "elements") // this is for any other property on the collection. skip elements props in layer structure.
        {
          objectLayerName = context.propName[0] == '@' ? context.propName.Substring(1) : context.propName;
        }
        LayerIdRecurse(context.parent, stringBuilder);
        if (stringBuilder.Length != 0 && !string.IsNullOrEmpty(objectLayerName))
        {
          stringBuilder.Append('$');
        }
        stringBuilder.Append(objectLayerName);

        return stringBuilder;
      }

      var traverseFunction = DefaultTraversal.CreateTraverseFunc(converter);

      var objectsToConvert = traverseFunction
        .Traverse(obj)
        .Select(tc => CreateApplicationObject(tc.current, LayerId(tc)))
        .Where(appObject => appObject != null)
        .Reverse() //just for the sake of matching the previous behaviour as close as possible
        .ToList();

      return objectsToConvert;
    }

    private List<object> ConvertObject(ApplicationObject appObj, ISpeckleConverter converter)
    {
      var obj = StoredObjects[appObj.OriginalId];
      var convertedList = new List<object>();

      var converted = converter.ConvertToNative(obj);

      if (converted == null)
        return convertedList;

      //Iteratively flatten any lists
      void FlattenConvertedObject(object item)
      {
        if (item is IList list)
          foreach (object child in list)
            FlattenConvertedObject(child);
        else
          convertedList.Add(item);
      }

      FlattenConvertedObject(converted);

      return convertedList;
    }

    private void BakeObject(
    ApplicationObject appObj,
    ISpeckleConverter converter,
    string layer,
    List<int> toRemove,
      ApplicationObject parent = null
    )
    {
      var obj = StoredObjects[appObj.OriginalId];
      int bakedCount = 0;
      bool remove =
        appObj.Status == ApplicationObject.State.Created
        || appObj.Status == ApplicationObject.State.Updated
        || appObj.Status == ApplicationObject.State.Failed
          ? false
          : true;

      foreach (var convertedItem in appObj.Converted)
      {
        switch (convertedItem)
        {
          case Entity o:

            if (o == null)
              continue;


            // handle display - fallback to rendermaterial if no displaystyle exists
            Base display = obj[@"displayStyle"] as Base;
            if (display == null)
              display = obj[@"renderMaterial"] as Base;
            if (display != null)
              Utils.SetStyle(display, o, LineTypeDictionary);

            // set application id
            var appId = parent != null ? parent.applicationId : obj.applicationId;
            var newObj = Doc.Elements[Convert.ToInt32(o.Id)];

            // TODO : Replace with Speckle Operation Folder
            //if (!ApplicationIdManager.SetObjectCustomApplicationId(newObj, appId, out appId))
            //{
            //  appObj.Log.Add($"Could not attach applicationId xdata");
            //}

            //tr.TransactionManager.QueueForGraphicsFlush();

            if (parent != null)
              parent.Update(createdId: o.Id.ToString());
            else
              appObj.Update(createdId: o.Id.ToString());

            bakedCount++;


            break;
          default:
            break;
        }
      }

      if (bakedCount == 0)
      {
        if (parent != null)
          parent.Update(logItem: $"fallback {appObj.applicationId}: could not bake object");
        else
          appObj.Update(status: ApplicationObject.State.Failed, logItem: $"Could not bake object");
      }
      else
      {
        // remove existing objects if they exist
        if (remove)
        {
          foreach (var objId in toRemove)
          {
            try
            {
              // TODO : Remove objet in Document
              //DBObject objToRemove = tr.GetObject(objId, OpenMode.ForWrite);
              //objToRemove.Erase();
            }
            catch (Exception e)
            {
              if (!e.Message.Contains("eWasErased")) // this couldve been previously received and deleted
              {
                if (parent != null)
                  parent.Log.Add(e.Message);
                else
                  appObj.Log.Add(e.Message);
              }
            }
          }
          appObj.Status = toRemove.Count > 0 ? ApplicationObject.State.Updated : ApplicationObject.State.Created;
        }
      }
    }


    #endregion

    #region sending
    public override async Task<string> SendStream(StreamState state, ProgressViewModel progress)
    {
      var kit = KitManager.GetDefaultKit();
      var converter = kit.LoadConverter(Utils.VersionedAppName);
      var streamId = state.StreamId;
      var client = state.Client;

      //List<KeyValuePair<string, string>> speckleParameters = Utils.getParameters(Doc);

      if (state == null)
      {
        Console.WriteLine(state);
      }


      if (state.Filter != null)
        state.SelectedObjectIds = GetObjectsFromFilter(state.Filter, converter);


      try
      {

        // Start undo Sequence // inSequence
        //UndoSequence.Start("SpeckleCreation", false); // no Ghost

        SetSettings(converter, state.CachedStream); // Send to Converter settings (streamName, etc)

        // remove deleted object ids
        var deletedElements = new List<string>();

        foreach (var id in state.SelectedObjectIds)
        {
          if (!Doc.Elements.Contains(Convert.ToInt32(id)))
          {
            deletedElements.Add(id);
          }

          Element e = Doc.Elements[Convert.ToInt32(id)];
          //List<KeyValuePair<string, string>> speckleParametersElements = Utils.getParameters(e);

        }
        state.SelectedObjectIds = state.SelectedObjectIds.Where(o => !deletedElements.Contains(o)).ToList();

        if (state.SelectedObjectIds.Count == 0)
        {

          progress.Report.LogOperationError(new Exception("Zero objects selected; send stopped. Please select some objects, or check that your filter can actually select something."));
          return null;
        }

        var commitObject = new Base();
        commitObject["units"] = Utils.GetUnits(Doc); // TODO: check whether commits base needs units attached

        int convertedCount = 0;

        // invoke conversions on the main thread via control
        if (Control.InvokeRequired)
          Control.Invoke(new Action(() => ConvertSendCommit(commitObject, converter, state, progress, ref convertedCount)), new object[] { });
        else
          ConvertSendCommit(commitObject, converter, state, progress, ref convertedCount);

        //progress.Report.Merge(converter.Report); // TODO Fixe Merge Empty

        if (convertedCount == 0)
        {
          // TODO Fix crash TopSolid
          //progress.Report.LogOperationError(new SpeckleException("Zero objects converted successfully. Send stopped.", false));
          return null;
        }

        if (progress.CancellationTokenSource.Token.IsCancellationRequested)
          return null;

        var transports = new List<ITransport>() { new ServerTransport(client.Account, streamId) };

        var commitObjId = await Operations.Send(
            commitObject,
            progress.CancellationTokenSource.Token,
            transports,
            onProgressAction: dict => progress.Update(dict),
            onErrorAction: (err, exception) =>
            {
              progress.Report.LogOperationError(exception);
              progress.CancellationTokenSource.Cancel();
            },
            disposeTransports: true
            );

        if (progress.Report.OperationErrorsCount != 0)
          return null;


        var actualCommit = new CommitCreateInput
        {
          streamId = streamId,
          objectId = commitObjId,
          branchName = state.BranchName,
          message = state.CommitMessage != null ? state.CommitMessage : $"Pushed {convertedCount} elements from {Utils.AppName}.",
          sourceApplication = Utils.VersionedAppName
        };

        if (state.PreviousCommitId != null) { actualCommit.parents = new List<string>() { state.PreviousCommitId }; }

        try
        {
          var commitId = await client.CommitCreate(actualCommit);
          state.PreviousCommitId = commitId;
          //UndoSequence.End();
          return commitId;
        }
        catch (Exception e)
        {
          progress.Report.LogOperationError(e);
          //UndoSequence.UndoCurrent(); // Cancel
          return null;
        }


      }
      catch (Exception ex)
      {
        // TODO : Message box
        progress.Report.LogOperationError(ex);
        //UndoSequence.UndoCurrent(); // Cancel
        return null;
      }

    }

    /// <summary>
    /// Previews a send operation
    /// </summary>
    /// <param name="state"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public override void PreviewSend(StreamState state, ProgressViewModel progress)
    {
      // TODO!
      Console.WriteLine("Send");
    }

    delegate void SendingDelegate(Base commitObject, ISpeckleConverter converter, StreamState state, ProgressViewModel progress, ref int convertedCount);
    private void ConvertSendCommit(Base commitObject, ISpeckleConverter converter, StreamState state, ProgressViewModel progress, ref int convertedCount)
    {

      try
      {
        UndoSequence.Start("SpeckleCreationSend", false); // no Ghost


        // set the context doc for conversion - this is set inside the transaction loop because the converter retrieves this transaction for all db editing when the context doc is set!
        //converter.SetContextDocument(Doc);

        var conversionProgressDict = new ConcurrentDictionary<string, int>();
        conversionProgressDict["Conversion"] = 0;

        foreach (string elementId in state.SelectedObjectIds)
        {
          if (progress.CancellationTokenSource.Token.IsCancellationRequested)
          {
            return;
          }

          conversionProgressDict["Conversion"]++;
          progress.Update(conversionProgressDict);

          int id = Convert.ToInt32(elementId);
          Element obj = Doc.Elements[id];
          string type = null;

          if (obj == null)
          {
            progress.Report.Log($"Skipped not found object: ${elementId}.");
            continue;
          }
          else
          {
            type = obj.GetType().ToString();
          }

          if (!converter.CanConvertToSpeckle(obj))
          {
            progress.Report.Log($"Skipped not supported type: ${type}. Object ${obj.Id} not sent.");
            continue;
          }

          try
          {
            // convert obj
            Base converted = null;
            string containerName = string.Empty;

            converted = converter.ConvertToSpeckle(obj);
            if (converted == null)
            {
              progress.Report.LogConversionError(new Exception($"Failed to convert object {elementId} of type {type}."));
              continue;
            }


            // Search layer
            string layerName = null;
            if (Doc.LayersFolderEntity != null && Doc.LayersFolderEntity.Entities != null)
            {
              foreach (LayerEntity layerEntity in Doc.LayersFolderEntity.Entities)
              {
                if (layerEntity.Layer.Id == obj.Layer.Id)
                {
                  layerName = layerEntity.Name;
                  break;
                }
                else if (layerEntity.Layer.Id == Doc.DefaultLayer.Id)
                {
                  if (layerName == null) layerName = layerEntity.Name;
                }
              }
            }

            if (layerName == null) layerName = "DefaultLayerName";
            containerName = layerName;

            if (commitObject[$"@{containerName}"] == null)
              commitObject[$"@{containerName}"] = new List<Base>();
            ((List<Base>)commitObject[$"@{containerName}"]).Add(converted);

            conversionProgressDict["Conversion"]++;
            progress.Update(conversionProgressDict);

            converted.applicationId = elementId;
          }
          catch (Exception e)
          {
            progress.Report.LogConversionError(new Exception($"Failed to convert object {elementId} of type {type}: {e.Message}"));
          }
          convertedCount++;
        }



        UndoSequence.End();
        //Doc.Update(true, true);

      }
      catch (Exception ex)
      {

        Console.WriteLine(ex.Message);
        // TODO : Message box
        UndoSequence.UndoCurrent(); // Cancel
      }

    }


    private List<string> GetObjectsFromFilter(ISelectionFilter filter, ISpeckleConverter converter)
    {
      var selection = new List<string>();
      switch (filter.Slug)
      {
        case "manual":
          return GetSelectedObjects();
        case "all":
          return Doc.ConvertibleObjects(converter);
        case "layer":
          {
            List<Element> allElements = Doc.ConvertibleObjectsAsElements(converter);
            foreach (Element eltToAnalyze in allElements)
            {
              LayerEntity layerEntityFromPart = Doc.LayersFolderEntity.SearchLayer(eltToAnalyze.Layer);
              if (layerEntityFromPart != null)
              {
                List<string> summaries = filter.Summary.Split(',').Select(x => x.Trim()).ToList();
                if (summaries.Contains(layerEntityFromPart.Name.Trim()))
                {
                  selection.Add(eltToAnalyze.Id.ToString());
                }
              }
            }
          }
          return selection;
        case "representation":
          {
            ElementList eltsInRepresentationFolder = new ElementList();
            (Doc as DesignDocument).RepresentationsFolderEntity.GetDeepConstituents(eltsInRepresentationFolder);
            foreach (Element representationFound in eltsInRepresentationFolder)
            {
              if (representationFound is RepresentationEntity representationEntity)
              {
                if (representationEntity.GetFriendlyName() == filter.Summary)
                {
                  DesignDocument designDoc = Doc as DesignDocument;
                  RepresentationEntity currentRepresentation = representationEntity;
                  ElementList constituents = new ElementList();
                  currentRepresentation.GetConstituents(constituents);
                  foreach (Element item in constituents)
                  {
                    if (item is RepresentationConstituentEntity represEntity)
                    {
                      EntityList listOfEntities = new EntityList();
                      represEntity.GetDeepContents(listOfEntities);
                      foreach (Entity entityInside in listOfEntities)
                      {
                        if (entityInside is SketchEntity sketchEntity)
                        {
                          selection.Add(entityInside.Id.ToString());
                        }
                        else
                        {
                          if (entityInside.HasGeometry)
                          {
                            selection.Add(entityInside.Id.ToString());
                          }
                          else
                          {
                            ElementList constituentsofPart = new ElementList();
                            entityInside.GetConstituents(constituentsofPart);
                            foreach (Element entityConstituent in constituentsofPart)
                            {
                              if (entityConstituent is ShapeEntity shapeEntity)
                              {
                                if (converter.CanConvertToSpeckle(shapeEntity))
                                {
                                  selection.Add(entityConstituent.Id.ToString());
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }

          }
          return selection;
      }
      return selection;
    }
    #endregion

    #region events

    public void RegisterAppEvents()
    {
      //// GLOBAL EVENT HANDLERS
      TopSolid.Kernel.WX.Application.CurrentDocumentChanged += Application_CurrentDocumentChanged;
    }

    private void Application_CurrentDocumentChanged(object sender, EventArgs e)
    {
      try
      {
        // Triggered when a document window is activated.This will happen automatically if a document is newly created or opened.
        if (e == null)
          return;


        var streams = GetStreamsInFile();//toujours vide
        if (streams.Count > 0)
          //SpeckleAutocadCommand.CreateOrFocusSpeckle();

          if (UpdateSavedStreams != null)
            UpdateSavedStreams(streams);
      }
      catch { }
    }

    private void Application_LayerChanged(object sender, TopSolid.Kernel.TX.Documents.DocumentClosingEventArgs e)
    {
      if (UpdateSelectedStream != null)
        UpdateSelectedStream();
    }

    //checks whether to refresh the stream list in case the user changes active view and selects a different document
    private void Application_WindowActivated(object sender)

    {
      try
      {
        var streams = GetStreamsInFile();
        UpdateSavedStreams(streams);

        MainViewModel.GoHome();
      }
      catch { }
    }

    private void Application_DocumentClosed(object sender)
    {
      try
      {
        MainViewModel.GoHome();
      }
      catch { }
    }

    private void Application_DocumentActivated(object sender)

    {
      try
      {
        var streams = GetStreamsInFile();
        if (streams.Count > 0)
          //SpeckleAutocadCommand.CreateOrFocusSpeckle();

          if (UpdateSavedStreams != null)
            UpdateSavedStreams(streams);

        MainViewModel.GoHome();
      }
      catch { }
    }
    #endregion
  }
}
