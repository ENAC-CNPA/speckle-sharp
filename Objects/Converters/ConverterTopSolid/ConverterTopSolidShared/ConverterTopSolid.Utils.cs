using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Media;
using Objects.Geometry;
using Objects.Other;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using Speckle.Newtonsoft.Json;
using TopSolid.Cad.Design.DB;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.PointClouds;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.DB.Sets;
using TopSolid.Kernel.G;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.SX.Collections;
using TopSolid.Kernel.SX.Drawing;
using TopSolid.Kernel.TX.Units;
using TKD = TopSolid.Kernel.DB;
using TKG = TopSolid.Kernel.G;
using SX = TopSolid.Kernel.SX;
using TsApp = TopSolid.Kernel.UI.Application;

namespace Objects.Converter.TopSolid
{
  public partial class ConverterTopSolid
  {


    #region units
    private string _modelUnits;
    public string ModelUnits
    {
      get
      {

        GeometricDocument Doc = TsApp.CurrentDocument as ModelingDocument;

        if (string.IsNullOrEmpty(_modelUnits))
          _modelUnits = UnitToSpeckle(LengthUnits.Meter);
        return _modelUnits;
      }
    }
    private void SetUnits(Base geom)
    {
      //geom.units = ModelUnits;//TODO check this
    }

    private double ScaleToNative(double value, string units)
    {
      if (units == "m") return value;
      var f = Units.GetConversionFactor(units, "m");
      return value * f;
    }

    private string UnitToSpeckle(Unit units)
    {

      switch (units.BaseUnit.Symbol) // TODO: Check Name conversion
      {
        case "mm":
          return Units.Millimeters;
        case "cm":
          return Units.Centimeters;
        case "m":
          return Units.Meters;
        case "km":
          return Units.Kilometers;
        case "in":
          return Units.Inches;
        case "ft":
          return Units.Feet;
        case "yd":
          return Units.Yards;
        case "mi":
          return Units.Miles;
        //case "Millimeter":
        //    return Units.Millimeters;
        //case "Centimeter":
        //    return Units.Centimeters;
        //case "Meter":
        //    return Units.Meters;
        //case "Kilometer":
        //    return Units.Kilometers;
        //case "Inche":
        //    return Units.Inches;
        //case "Fee":
        //    return Units.Feet;
        //case "Yard":
        //    return Units.Yards;
        //case "Mile":
        //    return Units.Miles;
        default:
          throw new System.Exception("The current Unit System is unsupported.");
      }
    }
    #endregion

    #region convertList
    public PointList ToNativePointList(IEnumerable<Geometry.Point> list, string units = null)
    {
      //var u = units ?? ModelUnits;
      //    var count = list.CountU * list.CountV;
      var points = new PointList();

      foreach (var p in list)
      {
        var pt = new TKG.D3.Point(ScaleToNative(p.x, p.units), ScaleToNative(p.y, p.units), ScaleToNative(p.z, p.units));
        points.Add(pt);
      }

      return points;
    }

    public static PointList ToPointList(List<TKG.D3.Point> list)
    {
      //    var count = list.CountU * list.CountV;
      var points = new PointList();

      foreach (var p in list)
      {
        points.Add(p);
      }

      return points;
    }


    static DoubleList ToNativeDoubleList(List<double> list)
    {
      DoubleList tsDblList = new DoubleList(list.Count);

      foreach (var d in list)
      {
        tsDblList.Add(d);
      }

      return tsDblList;
    }


    static DoubleList ToDoubleList(IEnumerable<double> list)
    {
      DoubleList tsDblList = new DoubleList();

      foreach (var d in list)
      {
        tsDblList.Add(d);
      }

      return tsDblList;
    }

    #endregion

    #region hosted elements

    /// <summary>
    /// Gets the hosted element of a host and adds the to a Base object
    /// </summary>
    /// <param name="host"></param>
    /// <param name="base"></param>
    public void GetHostedElements(Base @base, Element host)
    {
      if (host.HasGeometry)
      {
        //local copy ensures good positioning of entities
        ShapeEntity copyEntity = null;
        if (host is ShapeEntity partEntity)
        {
          Doc.EnsureIsDirty();
          copyEntity = (ShapeEntity)partEntity.MakeCopy(Doc, 0, TKG.D3.Transform.Identity, new EntityCopyOptions());
        }

        // TODO check if I have to add a code for multiple geometries ?
        var hostedGeometry = host.Geometry;
        if (copyEntity != null)
          hostedGeometry = ((Element)copyEntity).Geometry;
        IList<TKG.IGeometry> hostedGeometries = new List<TKG.IGeometry>();
        hostedGeometries.Add(hostedGeometry);

        var convertedHostedElements = new List<Base>();

        foreach (TKG.IGeometry geometry in hostedGeometries)
        {
          string geoType = geometry.GetType().Name;

          if (CanConvertToSpeckle(geometry))
          {
            Base obj = null;
            obj = ConvertToSpeckle(geometry);

            if (obj != null)
            {
              convertedHostedElements.Add(obj);
              //ConvertedObjectsList.Add(Convert.ToInt32(obj.applicationId));
            }
          }
        }

        if (convertedHostedElements.Any())
        {
          if (@base["@elements"] == null || !(@base["@elements"] is List<Base>))
            @base["@elements"] = new List<Base>();

          (@base["@elements"] as List<Base>).AddRange(convertedHostedElements);
        }

        //delete copied entity
        if (copyEntity != null)
        {
          Doc.EnsureIsDirty();
          TKD.Entities.CompositeEntity.Delete(copyEntity);
        }

      }
      else
      {
        //cas du pointCloud
        if (host is PointCloudEntity pointCloudEntity)
        {
          PointCloudItem pointCloudItem = pointCloudEntity.Vertices;
          List<Tuple<Geometry.Point, SX.Drawing.Color>> listOfTopSolidPoints = new List<Tuple<Geometry.Point, SX.Drawing.Color>>();
          for (int i = 0; i < pointCloudItem.VertexCount; i++)
          {
            SX.Drawing.Color pointColor = pointCloudItem.GetColor(i);
            TKG.D3.Point d3Point = pointCloudItem.GetVertex(i);
            listOfTopSolidPoints.Add(Tuple.Create(PointToSpeckle(d3Point, ModelUnits), pointColor));
          }

          List<Geometry.Point> pointListToFlatten = listOfTopSolidPoints.Select(x => x.Item1).ToList();
          List<double> flattenedPointList = pointListToFlatten.SelectMany(pt => new double[] { pt.x, pt.y, pt.z }).ToList();

          System.Collections.Generic.List<Int32> colors = new System.Collections.Generic.List<Int32>();
          System.Collections.Generic.List<System.Drawing.Color> systemColors = listOfTopSolidPoints.Select(x => (x.Item2).Argb).ToList();
          foreach (System.Drawing.Color systemColor in systemColors)
          {
            System.Drawing.Color opacityMaxColor = System.Drawing.Color.FromArgb(255, systemColor);
            Int32 colorAsArgb = opacityMaxColor.ToArgb();
            colors.Add(colorAsArgb);
          }

          Geometry.Pointcloud speckePointCloud = new Pointcloud(flattenedPointList, colors);

          if (speckePointCloud.points.Any())
          {
            if (@base["@elements"] == null || !(@base["@elements"] is List<Base>))
              @base["@elements"] = new List<Base>();

            (@base["@elements"] as List<Base>).Add(speckePointCloud);
          }

          return;
        }

        else if (host is SetDefinitionEntity set)
        {
          Base obj = null;
          obj = ConvertToSpeckle(set);

          if (obj != null)
          {
            if (@base["@elements"] == null || !(@base["@elements"] is List<Base>))
              @base["@elements"] = new List<Base>();

            (@base["@elements"] as List<Base>).Add(obj);
          }
        }
      }
    }

    public List<ApplicationObject> SetHostedElements(Base @base, Element host)
    {
      var placeholders = new List<ApplicationObject>();
      if (@base["@elements"] != null && @base["@elements"] is List<Base> elements)
      {
        CurrentHostElement = host;

        foreach (var obj in elements)
        {
          if (obj == null)
          {
            continue;
          }

          if (!CanConvertToNative(obj)) continue;

          try
          {
            var res = ConvertToNative(obj);
            if (res is ApplicationObject apl)
            {
              placeholders.Add(apl);
            }
            else if (res is ApplicationObject apls)
            {
              placeholders.Add(apls);
            }
          }
          catch (Exception e)
          {
            Report.ConversionErrors.Add(new Exception($"Failed to create hosted element {obj.speckle_type} in host ({host.Id}): \n{e.Message}"));
          }
        }

        CurrentHostElement = null; // unset the current host element.
      }
      return placeholders;
    }

    #endregion

    #region parameters

    /// <summary>
    /// </summary>
    /// <param name="topSolidElement"></param>
    /// <param name="speckleElement"></param>
    public void SetInstanceParameters(Base speckleElement, Element topSolidElement, Alias alias = null)
    {
      if (topSolidElement == null)
        return;

      var (topSolidParameters, isTopSolidAssembly) = getParameters(topSolidElement);
      Base paramBase = new Base();

      // TODO Optimize perf. (filtrer param by object type)
      foreach (var kv in topSolidParameters)
      {
        try
        {
          if (kv.Value != null && kv.Value.ToString() != "") paramBase[kv.Key] = kv.Value;
        }
        catch
        {
          //ignore
        }
      }

      if (paramBase.GetMembers().Any())
        speckleElement["parameters"] = paramBase;
      speckleElement["elementId"] = topSolidElement.Id.ToString();
      speckleElement.applicationId = topSolidElement.Id.ToString();
      speckleElement["units"] = ModelUnits;
      speckleElement["isTopSolidAssembly"] = isTopSolidAssembly;

      if (alias != null)
      {
        speckleElement["alias"] = JsonConvert.SerializeObject(alias);
      }

      var owner = (topSolidElement.Owner as Entity);
      if (owner != null)
      {
        speckleElement["renderMaterial"] = RenderMaterialToSpeckle(owner);
      }

    }

    /// <summary>
    /// </summary>
    /// <param name="topSolidElement"></param>
    /// <param name="speckleElement"></param>
    public void SetInstanceParameters(Base speckleElement, IGeometry topSolidElement, Alias alias = null)
    {
      if (topSolidElement == null)
        return;

      // TODO : Replace Owner by Geometry properties ? or not ? => Dependent on usage feedback in the Speckle viewer !
      Element owner = topSolidElement.Owner as Element;

      var (topSolidParameters, isTopSolidAssembly) = (topSolidElement.Owner != null) ? getParameters(owner) : (new List<KeyValuePair<string, object>>(), false);
      Base paramBase = new Base();

      // TODO Optimize perf. (filtrer param by type)
      foreach (var kv in topSolidParameters)
      {
        try
        {
          if (kv.Value != null && kv.Value.ToString() != "") paramBase[kv.Key] = kv.Value;
        }
        catch
        {
          //ignore
        }
      }

      if (speckleElement != null)
      {
        if (paramBase.GetMembers().Any())
          speckleElement["parameters"] = paramBase;
        speckleElement["units"] = ModelUnits;
        speckleElement["isTopSolidAssembly"] = isTopSolidAssembly;
        speckleElement["elementId"] = topSolidElement.Owner != null ? (topSolidElement.Owner as Element).Id.ToString() : "-";

        if (alias != null)
        {
          speckleElement["alias"] = JsonConvert.SerializeObject(alias);
        }

        if (owner != null)
        {
          speckleElement["renderMaterial"] = RenderMaterialToSpeckle(owner);
        }
        else
        {
          Console.WriteLine("No color and no owner");
        }
      }

    }


    public Alias GetAlias(Base speckleElement)
    {
      Alias alias = null;

      foreach (var p in speckleElement.GetMembers(DynamicBaseMemberType.Dynamic))
      {
        if (p.Key == "alias")
        {
          string aliasStr = p.Value.ToString();
          alias = JsonConvert.DeserializeObject<Alias>(aliasStr);
          break;
        }
      }

      return alias;

    }

    public static RenderMaterial RenderMaterialToSpeckle(Element element)
    {
      if (element == null)
        return null;

      string typeElt = element.GetType().ToString();
      if (element is not PointCloudEntity && element is not PointCloudsFolderEntity)
      {
        System.Drawing.Color color = element.Color;
        RenderMaterial material = new RenderMaterial()
        {
          name = element.Color.GetKnownName(),
          opacity = (double)element.Transparency.Opacity,
          //metalness = revitMaterial.Shininess / 128d, //Looks like these are not valid conversions
          //roughness = 1 - (revitMaterial.Smoothness / 100d),
          diffuse = color.ToArgb()
        };
        return material;
      }

      return null;
    }


    public static (IEnumerable<KeyValuePair<string, object>>, bool) getParameters(Element element)
    {

      List<KeyValuePair<string, object>> speckleParameters = new List<KeyValuePair<string, object>>();
      List<string> checkTypes = new List<string>();
      IEnumerable<ParameterEntity> paramElements = null;
      PartEntity ownerDoc = element.Owner as PartEntity;
      bool isTopSolidAssembly = false;

      if (ownerDoc is null)
      {
        AssemblyEntity ownerAss = element.Owner as AssemblyEntity;
        if (ownerAss != null && ownerAss.DefinitionDocument.ParametersFolderEntity != null)
        {
          isTopSolidAssembly = true;
          paramElements = ownerAss.DefinitionDocument.ParametersFolderEntity.DeepParameters;
        }
      }
      else
      {
        paramElements = ownerDoc.DefinitionDocument.ParametersFolderEntity.DeepParameters;
      }

      if (paramElements != null)
      {
        foreach (ParameterEntity param in paramElements)
        {
          //TextParameterEntity name = doc.ParametersFolderEntity.SearchDeepEntity("") as TextParameterEntity;

          if (param is TextParameterEntity textParam)
          {
            var temp = Regex.Replace(Convert.ToString(textParam.Value), "[^0-9a-zA-Z ]+", "");
            KeyValuePair<string, object> sp = new KeyValuePair<string, object>(textParam.GetFriendlyName(), temp);
            speckleParameters.Add(sp);
          }
          else if (param is DateTimeParameterEntity dateParam)
          {
            KeyValuePair<string, object> sp = new KeyValuePair<string, object>(dateParam.GetFriendlyName(), dateParam.Value);
            speckleParameters.Add(sp);
          }
          else
          {
            checkTypes.Add(param.GetType().ToString());
            KeyValuePair<string, object> sp = new KeyValuePair<string, object>(param.GetFriendlyName(), "");
            speckleParameters.Add(sp);
          }
        }
      }

      return (speckleParameters, isTopSolidAssembly);

    }


    public void GetInstanceParameters(Base speckleElement)
    {
      if (Doc == null)
        return;

      ParametersFolderEntity paramFolderRoot = Doc.ParametersFolderEntity;
      ParametersFolderEntity speckleParamFolderEntity = null;
      ElementList parameterFolderConstituents = new ElementList();
      paramFolderRoot.GetConstituents(parameterFolderConstituents);
      foreach (Element constituent in parameterFolderConstituents)
      {
        if (constituent is ParametersFolderEntity)
        {
          if (constituent.Name == "Speckle Parameters")
          {
            speckleParamFolderEntity = (ParametersFolderEntity)constituent;
          }
        }
      }
      if (speckleParamFolderEntity == null)
      {
        speckleParamFolderEntity = new ParametersFolderEntity(Doc, 0);
        speckleParamFolderEntity.Name = "Speckle Parameters";
        speckleParamFolderEntity.Create(Doc.ParametersFolderEntity);
      }

      //if parameter exists, upddate
      ElementList parametersInsideSpeckleFolder = new ElementList();
      speckleParamFolderEntity.GetConstituents(parametersInsideSpeckleFolder);

      foreach (var p in speckleElement.GetMembers(DynamicBaseMemberType.Dynamic))
      {

        Element element = Doc.Elements[p.Key];
        if (element != null && element is TextParameterEntity parameter)
        {
          parameter.Value = p.Value.ToString();
        }
        else
        {
          TextParameterEntity newParam = new TextParameterEntity(Doc, 0);
          newParam.Name = p.Key;
          newParam.Value = p.Value.ToString();
          newParam.Create();
          speckleParamFolderEntity.AddEntity(newParam);
        }

      }





    }

    #endregion

    #region Display and Attributes
    public (SX.Drawing.Color, Transparency) DiplayToNative(Base styleBase)
    {
      var color = new System.Drawing.Color();
      RenderMaterial mat = styleBase["renderMaterial"] as RenderMaterial;
      if (styleBase["displayStyle"] != null && styleBase["displayStyle"] is DisplayStyle style)
      {
        color = System.Drawing.Color.FromArgb(style.color);
      }
      else
      if (styleBase["renderMaterial"] != null && styleBase["renderMaterial"] is RenderMaterial material) // this is the fallback value if a rendermaterial is passed instead
      {
        color = System.Drawing.Color.FromArgb(material.diffuse);
      }
      else return (SX.Drawing.Color.Blue, Transparency.SemiTransparent);
      return (new SX.Drawing.Color(color.R, color.G, color.B), Transparency.FromByte((byte)(byte.MaxValue - color.A)));
    }
    

    public GeometryAliasLinked GetHashVertex(Vertex vertex, int index)
    {

      // TODO : Check if no surface and edges => can't force moniker
      string fHach = string.Join("-", vertex.Faces.ToList().Select(f => f.Moniker).OrderBy(s => s));
      string eHach = string.Join("-", vertex.Edges.ToList().Select(f => f.Moniker).OrderBy(s => s));
      string vHash = GetHash(fHach + "+" + eHach);

      return new GeometryAliasLinked
      {
        Index = index,
        Moniker = vertex.Moniker.ToString(),
        Hash = vHash
      };

    }

    public string GetHash(string s)
    {
      //using (MD5 md5 = MD5.Create())
      //{
      //  return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(s)))
      //              .Replace("-", "");
      //}
      return s.GetHashCode().ToString();
    }


    //TODO Lineweight
    //private static LineWeight GetLineWeight(double weight)
    //{
    //    double hundredthMM = weight * 100;
    //    var weights = Enum.GetValues(typeof(LineWeight)).Cast<int>().ToList();
    //    int closest = weights.Aggregate((x, y) => Math.Abs(x - hundredthMM) < Math.Abs(y - hundredthMM) ? x : y);
    //    return (LineWeight)closest;
    //}

    #endregion









  }
}
