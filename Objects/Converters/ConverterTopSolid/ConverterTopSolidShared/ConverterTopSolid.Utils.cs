
using Objects.BuiltElements;
using Objects.BuiltElements.Revit;
using Objects.Geometry;
using Objects.Other;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Speckle.Core.Kits;
using Speckle.Newtonsoft.Json;

using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.UI;
using TopSolid.Kernel.DB.OptionSets;
using TopSolid.Kernel.TX.Units;
using TopSolid.Kernel.GR.Transforms;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Cad.Design.DB;
using TopSolid.Kernel.G.D3.Shapes;

using TsApp = TopSolid.Kernel.UI.Application;
using TX = TopSolid.Kernel.TX;
using TopSolid.Kernel.SX.Collections;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D1;
using TKG = TopSolid.Kernel.G;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.G;
using TopSolid.Kernel.SX.Drawing;
using Speckle.Core.Api;
using DesktopUI2.Models;
using System.Text;
using System.Security.Cryptography;

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

        static BSpline ToBSpline(List<double> list)
        {
            //var count = list.CountU * list.CountV;
            BSpline w = new BSpline(); // = new DoubleList(count);
            //foreach (ControlPoint p in list)
            //{
            //    var weight = p.Weight;
            //    w.Add(weight);
            //}
            return w;
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

        private bool ShouldConvertHostedElement(Element element, Element host)
        {
            //doesn't have a host, go ahead and convert
            if (host == null)
                return true;

            // has been converted before (from a parent host), skip it
            if (ConvertedObjectsList.IndexOf(element.Id) != -1)
            {
                return false;
            }

            // the parent is in our selection list,skip it, as this element will be converted by the host element
            if (ContextObjects.FindIndex(obj => obj.applicationId == host.Id.ToString()) != -1)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Gets the hosted element of a host and adds the to a Base object
        /// </summary>
        /// <param name="host"></param>
        /// <param name="base"></param>
        public void GetHostedElements(Base @base, Element host)
        {
            if (host.HasGeometry)
            {

                // TODO check if I have to add a code for multiple geometries ?
                var hostedGeometry = host.Geometry;
                IList<TKG.IGeometry> hostedGeometries = new List<TKG.IGeometry>();
                hostedGeometries.Add(hostedGeometry);

                var convertedHostedElements = new List<Base>();

                foreach (TKG.IGeometry geometry in hostedGeometries)
                {


                    if (CanConvertToSpeckle(geometry))
                    {
                        var obj = ConvertToSpeckle(geometry);

                        if (obj != null)
                        {
                            convertedHostedElements.Add(obj);
                            ConvertedObjectsList.Add(Convert.ToInt32(obj.applicationId));
                        }
                    }
                }

                if (convertedHostedElements.Any())
                {
                    if (@base["@elements"] == null || !(@base["@elements"] is List<Base>))
                        @base["@elements"] = new List<Base>();

                    (@base["@elements"] as List<Base>).AddRange(convertedHostedElements);
                }



            }
            else
            {
                return;

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
            } else
            {
              Console.WriteLine("No color and no owner");
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
            
          ParametersFolderEntity paramfolderEntity = new ParametersFolderEntity(Doc, 0);
          paramfolderEntity.Name = "Speckle Parameters";

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
              paramfolderEntity.AddEntity(newParam);
            }

          }

          paramfolderEntity.Create(Doc.ParametersFolderEntity);



        }

        #endregion

        #region Display and Attributes
        public (Color, Transparency) DiplayToNative(Base styleBase)
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
            else return (Color.Blue, Transparency.SemiTransparent);
            return (new Color(color.R, color.G, color.B), Transparency.FromByte((byte)(byte.MaxValue - color.A)));
        }


     public string GetHash(string s)
    {
        using (MD5 md5 = MD5.Create())
        {
        return s;
          return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(s)))
                      .Replace("-", "");
        }
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
