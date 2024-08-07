using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using Application = TopSolid.Kernel.UI.Application;
using DB = TopSolid.Kernel.DB;

using Speckle.Core.Models;
using Objects.Geometry;
using TK = TopSolid.Kernel;
using Objects.BuiltElements;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.G.D3;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.DB.D3.Meshes;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.SX.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TopSolid.Kernel.DB.Sets;

namespace Objects.Converter.TopSolid
{
  public partial class ConverterTopSolid
  {

    //public static ModelingDocument Doc => Application.CurrentDocument as ModelingDocument;

    // Elements
    #region Elements
    public Base ElementToSpeckle(Element topSolidElement, string units = null)
    {
      var u = units ?? ModelUnits;
      Base speckleElement = new Base();

      //speckleElement["renderMaterial"] = new Other.RenderMaterial() { opacity = 0.2, diffuse = System.Drawing.Color.AliceBlue.ToArgb() };


      SetInstanceParameters(speckleElement, topSolidElement);
      GetHostedElements(speckleElement, topSolidElement);

      Console.WriteLine(speckleElement);
      return speckleElement;
    }
    #region not used    
    public System.Collections.Generic.List<ApplicationObject> ElementToNative(Base speckleElement)
    {
      Element topSolidElement = Doc.Elements[Convert.ToInt32(speckleElement.applicationId)];
      if (topSolidElement != null && ReceiveMode == Speckle.Core.Kits.ReceiveMode.Ignore)
        return new System.Collections.Generic.List<ApplicationObject>(); // { new ApplicationPlaceholderObject { applicationId = speckleElement.applicationId, ApplicationGeneratedId = topSolidElement.Id.ToString(), NativeObject = topSolidElement } }; ;

      bool isUpdate = true;
      if (topSolidElement == null) // TODO : Create element
      {

      }
      if (topSolidElement == null) // Check if created
      {
        throw new Speckle.Core.Logging.SpeckleException($"Failed to create Entity ${speckleElement.applicationId}.");
      }

      var placeholders = new System.Collections.Generic.List<ApplicationObject>();

      //{
      //  new ApplicationPlaceholderObject
      //  {
      //  applicationId = speckleElement.applicationId,
      //  ApplicationGeneratedId = topSolidElement.Id.ToString(),
      //  NativeObject = topSolidElement
      //  }
      //};

      var hostedElements = SetHostedElements(speckleElement, topSolidElement);
      placeholders.AddRange(hostedElements);


      Report.Log($"{(isUpdate ? "Updated" : "Created")} Entity {topSolidElement.Id}");

      return placeholders;
    }
    #endregion

    #endregion



    // SketchEntity
    #region SketchEntity
    #region Not used
    /*
    public Base D3SketchEntityToSpeckle(DB.D3.Sketches.SketchEntity topSolidElement, string units = null)
        {
            var u = units ?? ModelUnits;
            Base speckleElement = new Base();

            //speckleElement["renderMaterial"] = new Other.RenderMaterial() { opacity = 0.2, diffuse = System.Drawing.Color.AliceBlue.ToArgb() };


            //SetInstanceParameters(speckleElement, topSolidElement);
            GetHostedElements(speckleElement, topSolidElement, new Transform());

            Console.WriteLine(speckleElement);
            return speckleElement;
        }
        public List<ApplicationObject> D3SketchEntityToNative(Base speckleElement)
        {
            Element topSolidElement = Doc.Elements[Convert.ToInt32(speckleElement.applicationId)];
            if (topSolidElement != null && ReceiveMode == Speckle.Core.Kits.ReceiveMode.Ignore)
                return new List<ApplicationObject>(); // { new ApplicationPlaceholderObject { applicationId = speckleElement.applicationId, ApplicationGeneratedId = topSolidElement.Id.ToString(), NativeObject = topSolidElement } }; ;

            bool isUpdate = true;
            if (topSolidElement == null) // TODO : Create element
            {

            }
            if (topSolidElement == null) // Check if created
            {
                throw new Speckle.Core.Logging.SpeckleException($"Failed to create Entity ${speckleElement.applicationId}.");
            }

            var placeholders = new List<ApplicationObject>();

            //{
            //  new ApplicationPlaceholderObject
            //  {
            //  applicationId = speckleElement.applicationId,
            //  ApplicationGeneratedId = topSolidElement.Id.ToString(),
            //  NativeObject = topSolidElement
            //  }
            //};

            var hostedElements = SetHostedElements(speckleElement, topSolidElement);
            placeholders.AddRange(hostedElements);


            Report.Log($"{(isUpdate ? "Updated" : "Created")} Entity {topSolidElement.Id}");

            return placeholders;
        }
        public Base D2SketchEntityToSpeckle(DB.D2.Sketches.SketchEntity topSolidSketchEntity, string units = null)
        {
            var u = units ?? ModelUnits;
            Base speckleSketchEntity = new Base();

            speckleSketchEntity["renderMaterial"] = new Other.RenderMaterial() { opacity = 1 - topSolidSketchEntity.ExplicitTransparency.ToFloat(), diffuse = topSolidSketchEntity.ExplicitColor.Argb.ToArgb() };

            //SetInstanceParameters(speckleElement, topSolidElement);
            GetHostedElements(speckleSketchEntity, topSolidSketchEntity,new Transform());


            Console.WriteLine(speckleSketchEntity);
            return speckleSketchEntity;
        }
        public List<ApplicationObject> D2SketchEntityToNative(Base speckleElement)
        {
            Element topSolidElement = Doc.Elements[Convert.ToInt32(speckleElement.applicationId)];
            if (topSolidElement != null && ReceiveMode == Speckle.Core.Kits.ReceiveMode.Ignore)
                return new List<ApplicationObject>(); // { new ApplicationPlaceholderObject { applicationId = speckleElement.applicationId, ApplicationGeneratedId = topSolidElement.Id.ToString(), NativeObject = topSolidElement } }; ;

            bool isUpdate = true;
            if (topSolidElement == null) // TODO : Create element
            {

            }
            if (topSolidElement == null) // Check if created
            {
                throw new Speckle.Core.Logging.SpeckleException($"Failed to create Entity ${speckleElement.applicationId}.");
            }


            var placeholders = new List<ApplicationObject>();

            //{
            //  new ApplicationPlaceholderObject
            //  {
            //  applicationId = speckleElement.applicationId,
            //  ApplicationGeneratedId = topSolidElement.Id.ToString(),
            //  NativeObject = topSolidElement
            //  }
            //};

            var hostedElements = SetHostedElements(speckleElement, topSolidElement);
            placeholders.AddRange(hostedElements);


            Report.Log($"{(isUpdate ? "Updated" : "Created")} Entity {topSolidElement.Id}");

            return placeholders;
        }
    */
    #endregion
    public ShapeEntity EntityBrepToNative(Brep brep)
    {
      ShapeEntity topSolidElement = Doc.Elements[Convert.ToInt32(brep["elementId"])] as ShapeEntity;
      int id = 0;
      if (brep["elementId"] != null) id = Convert.ToInt32(brep["elementId"]);

      if (topSolidElement != null && ReceiveMode == Speckle.Core.Kits.ReceiveMode.Ignore)
      {
        return null; // { new ApplicationPlaceholderObject { applicationId = speckleElement.applicationId, ApplicationGeneratedId = topSolidElement.Id.ToString(), NativeObject = topSolidElement } }; ;
      }

      bool isUpdate = true; // TODO : for optimize check if modified

      EntitiesCreation entitiesCreation = new EntitiesCreation(Doc, 0);

      ShapeEntity shapeEntity = new ShapeEntity(Doc, 0/*id*/);
      entitiesCreation.AddChildEntity(shapeEntity);//afs add

      var entCreations = EntitiesCreation.GetEntitiesCreation(Doc);

      if (topSolidElement == null) // TODO : Create element
      {
        shapeEntity.Geometry = BrepToNative(brep, null);
        //shapeEntity.Create();

        var display = DiplayToNative(brep);
        shapeEntity.ExplicitColor = display.Item1;
        shapeEntity.ExplicitTransparency = display.Item2;
        shapeEntity.Name = "Brep " + shapeEntity.Id.ToString() + "";


        if (!entitiesCreation.IsCreated)
        {
          try
          {
            entitiesCreation.Create(sfo);
          }
          catch (Exception eee)
          {
            string err = eee.Message.ToString();
          }
        }


        TK.DB.D3.Modeling.Documents.ModelingDocument doc = Doc as TK.DB.D3.Modeling.Documents.ModelingDocument;
        ShapesFolderEntity folder = doc.ShapesFolderEntity;
        folder.AddEntity(shapeEntity);

        GetInstanceParameters(brep);

      }
      else // Check if created
      {
        // Get current insertion operation.
        Operation currentInsertionOperation = Doc.SynchronizedInsertionOperation;

        // Move insertion after EntitiesCreation.
        Doc.MoveSynchronizedInsertionOperation(sfo.NextLocalOperation);

        shapeEntity = topSolidElement;
        shapeEntity.Parent.IsEdited = true;

        // move the cursor just after Speckle Operation Folder

        try
        {
          var shape = BrepToNative(brep, null); // Move Synchronize insertion
          shapeEntity.Geometry = shape;
        }
        finally
        {
          //HealingTools.SetOperationEditable(entitiesCreation, false);
          shapeEntity.Parent.IsEdited = false;
          shapeEntity.Parent.NeedsExecuting = true;
          //shapeEntity.MakeDisplay(); // Not necessary
        }


        // Restore position of synchronized insertion operation.
        if (currentInsertionOperation != null)
          Doc.MoveSynchronizedInsertionOperation(currentInsertionOperation);

        // throw new Speckle.Core.Logging.SpeckleException($"Failed to create Entity ${brep.applicationId}.");
      }




      return shapeEntity;

    }

    #endregion


    public Collection SetToSpeckle(SetDefinitionEntity entitySet)
    {      
      Collection speckleElement = new Collection();
      //System.Collections.Generic.List<Object> list = new System.Collections.Generic.List<object>();
      foreach (var entity in entitySet.Entities)
      {      
          if (entity is CompositeEntity compositeEntity)
        {
          speckleElement.elements.Add(GetConstituents(compositeEntity));
        }

        else
        {
          var baseObj = new Base();
          baseObj["referenced obj id"] = entity.Id;
          speckleElement.elements.Add(baseObj);
        }


      }
        return speckleElement;
    }

    private Collection GetConstituents(CompositeEntity compositeEntity)
    {
      Collection collection = new Collection();

      foreach(var entity in compositeEntity.Entities)
      {
        if (entity is CompositeEntity compositeEntity2)
        {
          collection.elements.Add(GetConstituents(compositeEntity));
        }
        else
        {
          var baseObj = new Base();
          baseObj["referenced obj id"] = entity.Id;
          collection.elements.Add(baseObj);
        }

      }

      return collection;
    }
  }
}
