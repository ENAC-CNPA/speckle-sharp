using System.Collections.Generic;
using System.Linq;
using DynamicData;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using TopSolid.Cad.Design.DB;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Cad.Design.DB.Representations;
using TopSolid.Kernel.DB.D2.Sketches;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.PointClouds;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Sets;
using TopSolid.Kernel.TX.Units;

namespace Speckle.ConnectorTopSolid.UI
{
  public static class Utils
  {

#if TOPSOLID715
    public static string VersionedAppName = HostApplications.TopSolid.GetVersion(HostAppVersion.v715);
    public static string AppName = HostApplications.TopSolid.Name;
    public static string Slug = HostApplications.TopSolid.Slug;
#elif TOPSOLID716
    public static string VersionedAppName = HostApplications.TopSolid.GetVersion(HostAppVersion.v716);
    public static string AppName = HostApplications.TopSolid.Name;
    public static string Slug = HostApplications.TopSolid.Slug;
#elif TOPSOLID717
    public static string VersionedAppName = HostApplications.TopSolid.GetVersion(HostAppVersion.v717);
    public static string AppName = HostApplications.TopSolid.Name;
    public static string Slug = HostApplications.TopSolid.Slug;
#endif

    public static string invalidChars = @"<>/\:;""?*|=,‘";

    /// <summary>
    /// Retrieves the document's units.
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    /// 
    public static string GetUnits(GeometricDocument doc)
    {
      Unit unit = doc.LengthUnit;
      return unit.BaseUnit.Symbol.ToString();
    }

    /// <summary>
    /// Set style for Topsolid Elements when receiving from Speckle
    /// </summary>
    /// <param name="styleBase">Speckle base style</param>
    /// <param name="element">TopSolid Element</param>
    /// <param name="lineTypeDictionary"> dictionary for line types conversion</param>
    public static void SetStyle(Base styleBase, Element element, Dictionary<string, int> lineTypeDictionary)
    {
      var units = styleBase["units"] as string;
      var color = styleBase["color"] as int?;
      if (color == null) color = styleBase["diffuse"] as int?; // in case this is from a rendermaterial base
      var lineType = styleBase["linetype"] as string;
      var lineWidth = styleBase["lineweight"] as double?;

      // TODO Create Line Type TopSolid => LineType

      if (color != null)
      {
        var systemColor = System.Drawing.Color.FromArgb((int)color);
        //element.Color = Color.FromRgb(systemColor.R, systemColor.G, systemColor.B);
        //element.Transparency = new Transparency(systemColor.A);
      }

      double conversionFactor = (units != null) ? Units.GetConversionFactor(Units.GetUnitsFromString(units), Units.Millimeters) : 1;
      //if (lineWidth != null)
      //    element.LineWeight = GetLineWeight((double)lineWidth * conversionFactor);

      //if (lineType != null)
      //    if (lineTypeDictionary.ContainsKey(lineType))
      //        element.LinetypeId = lineTypeDictionary[lineType];
    }

    /// <summary>
    /// Gets the handles of all visible document objects that can be converted to Speckle
    /// </summary>
    /// <param name="doc">Modeling Document</param>
    /// <param name="converter">Speckle Converter</param>
    /// <returns>List of id of elements that can be converted</returns>
    public static List<string> ConvertibleObjects(this ModelingDocument doc, ISpeckleConverter converter)
    {
      DesignDocument designDoc = doc as DesignDocument;
      //RepresentationEntity currentRepresentation = designDoc.CurrentRepresentationEntity;
      var objs = new List<string>();
      ElementList constituents = new ElementList();
      designDoc.RootEntity.GetConstituents(constituents);
      foreach (Element item in constituents)
      {
        if (item is FolderEntity represEntity)
        {
          //EntityList listOfEntities = new EntityList(represEntity.Entities.ToList());
          //listOfEntities = represEntity.Entities as EntityList;
          foreach (Entity entityInside in represEntity.Entities)
          {
            if (entityInside is SketchEntity sketchEntity)
            {
              objs.Add(entityInside.Id.ToString());
            }
            else
            {
              if (entityInside.HasGeometry)
              {
                objs.Add(entityInside.Id.ToString());
              }
              else if (entityInside is SetDefinitionEntity)
              {
                objs.Add(entityInside.Id.ToString());
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
                      objs.Add(entityConstituent.Id.ToString());
                    }
                  }
                }
              }
            }
            
           


          }
        }
      }

      PointCloudsFolderEntity pointsCloudFolder = PointCloudsFolderEntity.GetFolder(doc);
      if (pointsCloudFolder != null)
      {
        ElementList pointClouds = new ElementList();
        pointsCloudFolder.GetConstituents(pointClouds);
        foreach (TopSolid.Kernel.DB.Elements.Element element in pointClouds)
        {
          if (element is PointCloudEntity ptcE)
            objs.Add(ptcE.Id.ToString());
        }
      }      


      return objs;
    }

    /// <summary>
    /// Gets the handles of all visible document objects that can be converted to Speckle
    /// </summary>
    /// <param name="doc">Modeling Document</param>
    /// <param name="converter">Speckle Converter</param>
    /// <returns>List of elements that can be converted</returns>
    public static List<Element> ConvertibleObjectsAsElements(this ModelingDocument doc, ISpeckleConverter converter)
    {
      DesignDocument designDoc = doc as DesignDocument;

      var objs = new List<Element>();
      IEnumerable<Element> elements = designDoc.Elements.GetAll();


      if (designDoc is AssemblyDocument)
      {
        List<Entity> entitiesInsidePartsFolder = new List<Entity>();
        PartsFolderEntity partsFolder = (designDoc as AssemblyDocument).PartsFolderEntity;
        Utils.GetEntities(entitiesInsidePartsFolder, partsFolder);
        List<Entity> partEntities = (from Entity ent in entitiesInsidePartsFolder
                                     where ent is PartEntity
                                     select ent).ToList();
        foreach (PartEntity entity in partEntities)
        {
          if (entity.HasGeometry)
          {
            if (converter.CanConvertToSpeckle(entity))
              objs.Add(entity);
          }
          else
          {
            ElementList constituentsofPart = new ElementList();
            entity.GetDeepConstituents(constituentsofPart);
            foreach (Element entityConstituent in constituentsofPart)
            {
              if (entityConstituent is ShapeEntity shapeEntity)
              {
                if (converter.CanConvertToSpeckle(shapeEntity))
                  objs.Add(shapeEntity);
              }
            }
          }
        }
      }
      else if (designDoc is PartDocument)
      {
        foreach (Element element in elements) // multithread
        {
          if (element is ShapeEntity shapeEntity)
          {
            if (converter.CanConvertToSpeckle(shapeEntity))
              objs.Add(shapeEntity);
          }
        }
      }

      return objs;
    }

    #region Folders helpers
    private static void GetEntities(List<Entity> entitiesInsidePartsFolder, FolderEntity partsFolder)
    {
      ElementList insidePartsFolder = new ElementList();
      partsFolder.GetConstituents(insidePartsFolder);
      foreach (Element constituent in insidePartsFolder)
      {
        if (constituent is FolderEntity folder)
        {
          GetEntities(entitiesInsidePartsFolder, folder);
        }
        else
        {
          if (constituent is PartEntity || constituent is ShapeEntity)
          {
            entitiesInsidePartsFolder.Add(constituent as Entity);
          }
        }
      }
    }
    #endregion

  }
}
