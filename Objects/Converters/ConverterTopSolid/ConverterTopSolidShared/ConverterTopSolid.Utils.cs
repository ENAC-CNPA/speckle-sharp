using Speckle.Core.Kits;
using Speckle.Core.Models;

using System.Drawing;
using System.Text.RegularExpressions;
using TsUnits = TopSolid.Kernel.TX.Units.UnitFormat;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.UI;
using TopSolid.Kernel.DB.OptionSets;
using TopSolid.Kernel.TX.Units;
using TopSolid.Kernel.GR.Transforms;
using TsApp = TopSolid.Kernel.UI.Application;
using TopSolid.Kernel.SX.Collections;
using TopSolid.Kernel.G.D3;
using Objects.Geometry;
using System.Collections.Generic;
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
            geom.units = ModelUnits;
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


        //public static ToDoubleList ToDoubleList(List<List<ControlPoint>> list)
        //{
        //    var count = list.Count;
        //    var knots = new double[count + 2];

        //    int j = 0, k = 0;
        //    while (j < count)
        //        knots[++k] = list[j++];

        //    knots[0] = knots[1];
        //    knots[count + 1] = knots[count];
        //    var kDl = new DoubleList();
        //    foreach (double d in knots)
        //    {
        //        kDl.Add(d);
        //    }
        //    return kDl;
        //}

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
    }
}
