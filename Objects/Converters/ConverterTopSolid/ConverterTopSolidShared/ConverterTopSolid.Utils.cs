
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

using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.UI;
using TopSolid.Kernel.DB.OptionSets;
using TopSolid.Kernel.TX.Units;
using TopSolid.Kernel.GR.Transforms;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Cad.Design.DB;

using TsApp = TopSolid.Kernel.UI.Application;
using TopSolid.Kernel.SX.Collections;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D1;
using TKG = TopSolid.Kernel.G;


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
                    _modelUnits = UnitToSpeckle(Doc.LengthUnit);
                return _modelUnits;
            }
        }
        private void SetUnits(Base geom)
        {
            //geom.units = ModelUnits;//TODO check this
        }

        private double ScaleToNative(double value, string units)
        {
            var f = Units.GetConversionFactor(units, ModelUnits);
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
        public static PointList ToPointList(IEnumerable<Geometry.Point> list)
        {
            //    var count = list.CountU * list.CountV;
            var points = new PointList();

            foreach (var p in list)
            {
                var pt = new TKG.D3.Point(p.x, p.y, p.z);
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

        static DoubleList ToDoubleList(List<double> list)
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
                    if (@base["elements"] == null || !(@base["elements"] is List<Base>))
                        @base["elements"] = new List<Base>();

                    (@base["elements"] as List<Base>).AddRange(convertedHostedElements);
                }


            
            } else
            {
                return;

            }


        }

        public List<ApplicationPlaceholderObject> SetHostedElements(Base @base, Element host)
        {
            var placeholders = new List<ApplicationPlaceholderObject>();
            if (@base["elements"] != null && @base["elements"] is List<Base> elements)
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
                        if (res is ApplicationPlaceholderObject apl)
                        {
                            placeholders.Add(apl);
                        }
                        else if (res is List<ApplicationPlaceholderObject> apls)
                        {
                            placeholders.AddRange(apls);
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

        //#region ToSpeckle
        ///// <summary>
        ///// Adds Instance and Type parameters, ElementId, ApplicationInternalName and Units.
        ///// </summary>
        ///// <param name="speckleElement"></param>
        ///// <param name="revitElement"></param>
        ///// <param name="exclusions">List of BuiltInParameters or GUIDs used to indicate what parameters NOT to get,
        ///// we exclude all params already defined on the top level object to avoid duplication and 
        ///// potential conflicts when setting them back on the element</param>
        //public void GetAllRevitParamsAndIds(Base speckleElement, DB.Element revitElement, List<string> exclusions = null)
        //{
        //    var instParams = GetInstanceParams(revitElement, exclusions);
        //    var typeParams = speckleElement is Level ? null : GetTypeParams(revitElement);  //ignore type props of levels..!
        //    var allParams = new Dictionary<string, Parameter>();

        //    if (instParams != null)
        //        instParams.ToList().ForEach(x => { if (!allParams.ContainsKey(x.Key)) allParams.Add(x.Key, x.Value); });

        //    if (typeParams != null)
        //        typeParams.ToList().ForEach(x => { if (!allParams.ContainsKey(x.Key)) allParams.Add(x.Key, x.Value); });

        //    //sort by key
        //    allParams = allParams.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        //    Base paramBase = new Base();

        //    foreach (var kv in allParams)
        //    {
        //        try
        //        {
        //            paramBase[kv.Key] = kv.Value;
        //        }
        //        catch
        //        {
        //            //ignore
        //        }
        //    }

        //    if (paramBase.GetDynamicMembers().Any())
        //        speckleElement["parameters"] = paramBase;
        //    speckleElement["elementId"] = revitElement.Id.ToString();
        //    speckleElement.applicationId = revitElement.UniqueId;
        //    speckleElement["units"] = ModelUnits;
        //    speckleElement["isRevitLinkedModel"] = revitElement.Document.IsLinked;
        //    speckleElement["revitLinkedModelPath"] = revitElement.Document.PathName;
        //}

        ////private List<string> alltimeExclusions = new List<string> { 
        ////  "ELEM_CATEGORY_PARAM" };
        //private Dictionary<string, Parameter> GetInstanceParams(DB.Element element, List<string> exclusions)
        //{
        //    return GetElementParams(element, false, exclusions);
        //}
        //private Dictionary<string, Parameter> GetTypeParams(DB.Element element)
        //{
        //    var elementType = element.Document.GetElement(element.GetTypeId());

        //    if (elementType == null || elementType.Parameters == null)
        //    {
        //        return new Dictionary<string, Parameter>();
        //    }
        //    return GetElementParams(elementType, true);

        //}

        //private Dictionary<string, Parameter> GetElementParams(DB.Element element, bool isTypeParameter = false, List<string> exclusions = null)
        //{
        //    exclusions = (exclusions != null) ? exclusions : new List<string>();

        //    //exclude parameters that don't have a value and those pointing to other elements as we don't support them
        //    var revitParameters = element.Parameters.Cast<DB.Parameter>()
        //      .Where(x => x.HasValue && x.StorageType != StorageType.ElementId && !exclusions.Contains(GetParamInternalName(x))).ToList();

        //    //exclude parameters that failed to convert
        //    var speckleParameters = revitParameters.Select(x => ParameterToSpeckle(x, isTypeParameter))
        //      .Where(x => x != null);

        //    return speckleParameters.GroupBy(x => x.applicationInternalName).Select(x => x.First()).ToDictionary(x => x.applicationInternalName, x => x);
        //}

        ///// <summary>
        ///// Returns the value of a Revit Built-In <see cref="DB.Parameter"/> given a target <see cref="DB.Element"/> and <see cref="BuiltInParameter"/>
        ///// </summary>
        ///// <param name="elem">The <see cref="DB.Element"/> containing the Built-In <see cref="DB.Parameter"/></param>
        ///// <param name="bip">The <see cref="BuiltInParameter"/> enum name of the target parameter</param>
        ///// <param name="unitsOverride">The units in which to return the value in the case where you want to override the Built-In <see cref="DB.Parameter"/>'s units</param>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //private T GetParamValue<T>(DB.Element elem, BuiltInParameter bip, string unitsOverride = null)
        //{
        //    var rp = elem.get_Parameter(bip);

        //    if (rp == null || !rp.HasValue)
        //        return default;

        //    var value = ParameterToSpeckle(rp, unitsOverride: unitsOverride).value;
        //    if (typeof(T) == typeof(int) && value.GetType() == typeof(bool))
        //        return (T)Convert.ChangeType(value, typeof(int));

        //    return (T)ParameterToSpeckle(rp, unitsOverride: unitsOverride).value;
        //}

        ///// <summary>
        ///// Converts a Revit Built-In <see cref="DB.Parameter"/> to a Speckle <see cref="Parameter"/>.
        ///// </summary>
        ///// <param name="rp">The Revit Built-In <see cref="DB.Parameter"/> to convert</param>
        ///// <param name="isTypeParameter">Defaults to false. True if this is a type parameter</param>
        ///// <param name="unitsOverride">The units in which to return the value in the case where you want to override the Built-In <see cref="DB.Parameter"/>'s units</param>
        ///// <returns></returns>
        ///// <remarks>The <see cref="rp"/> must have a value (<see cref="DB.Parameter.HasValue"/></remarks>
        //private Parameter ParameterToSpeckle(DB.Parameter rp, bool isTypeParameter = false, string unitsOverride = null)
        //{
        //    var sp = new Parameter
        //    {
        //        name = rp.Definition.Name,
        //        applicationInternalName = GetParamInternalName(rp),
        //        isShared = rp.IsShared,
        //        isReadOnly = rp.IsReadOnly,
        //        isTypeParameter = isTypeParameter,
        //        applicationUnitType = rp.GetUnityTypeString() //eg UT_Length
        //    };

        //    switch (rp.StorageType)
        //    {
        //        case StorageType.Double:
        //            // NOTE: do not use p.AsDouble() as direct input for unit utils conversion, it doesn't work.  ¯\_(ツ)_/¯
        //            var val = rp.AsDouble();
        //            try
        //            {
        //                sp.applicationUnit = rp.GetDisplayUnityTypeString(); //eg DUT_MILLIMITERS, this can throw!
        //                sp.value = unitsOverride == null ? RevitVersionHelper.ConvertFromInternalUnits(val, rp) : ScaleToSpeckle(val, unitsOverride);
        //            }
        //            catch
        //            {
        //                sp.value = val;
        //            }
        //            break;
        //        case StorageType.Integer:

        //            switch (rp.Definition.ParameterType)
        //            {
        //                case ParameterType.YesNo:
        //                    sp.value = Convert.ToBoolean(rp.AsInteger());
        //                    break;
        //                default:
        //                    sp.value = rp.AsInteger();
        //                    break;
        //            }

        //            break;
        //        case StorageType.String:
        //            sp.value = rp.AsString();
        //            if (sp.value == null)
        //                sp.value = rp.AsValueString();
        //            break;

        //        default:
        //            return null;
        //    }
        //    return sp;
        //}

        #endregion

        /// <summary>
        /// </summary>
        /// <param name="topSolidElement"></param>
        /// <param name="speckleElement"></param>
        public void SetInstanceParameters(Base speckleElement, Element topSolidElement)
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


            if (paramBase.GetDynamicMembers().Any())
                speckleElement["parameters"] = paramBase;
            speckleElement["elementId"] = topSolidElement.Id.ToString();
            speckleElement.applicationId = topSolidElement.Id.ToString();
            speckleElement["units"] = ModelUnits;
            speckleElement["isTopSolidAssembly"] = isTopSolidAssembly;

        }


        public static (IEnumerable<KeyValuePair<string, object>>,bool) getParameters(Element element)
        {

            List<KeyValuePair<string, object>> speckleParameters = new List<KeyValuePair<string, object>>();
            List<string> checkTypes = new List<string>();
            IEnumerable<ParameterEntity> paramElements = null;
            PartEntity ownerDoc = element.Owner as PartEntity;
            bool isTopSolidAssembly = false;

            if (ownerDoc is null)
            {
                AssemblyEntity ownerAss = element.Owner as AssemblyEntity;
                paramElements = ownerAss.DefinitionDocument.ParametersFolderEntity.DeepParameters;
                isTopSolidAssembly = true;
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


        //#endregion








    }
}
