using Objects.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

#region Speckle Objects using
using Box = Objects.Geometry.Box;
using ControlPoint = Objects.Geometry.ControlPoint;
using Interval = Objects.Primitive.Interval;
using Line = Objects.Geometry.Line;
using Plane = Objects.Geometry.Plane;
using Point = Objects.Geometry.Point;
using Polyline = Objects.Geometry.Polyline;
using Surface = Objects.Geometry.Surface;
using Vector = Objects.Geometry.Vector;
using Curve = Objects.Geometry.Curve;
#endregion

#region TopSolid objects using
//TopSolid.Kernel.G.D3 Objects
using D3Point = TopSolid.Kernel.G.D3.Point;
using D3Box = TopSolid.Kernel.G.D3.Box;
using D3BSplineSurface = TopSolid.Kernel.G.D3.Surfaces.BSplineSurface;
using D3LineCurve = TopSolid.Kernel.G.D3.Curves.LineCurve;
using TsPlane = TopSolid.Kernel.G.D3.Plane;
using D3PointList = TopSolid.Kernel.G.D3.PointList;
using D3PolylineCurve = TopSolid.Kernel.G.D3.Curves.PolylineCurve;
using D3UnitVector = TopSolid.Kernel.G.D3.UnitVector;
using D3Vector = TopSolid.Kernel.G.D3.Vector;
using D3BsplineCurve = TopSolid.Kernel.G.D3.Curves.BSplineCurve;

//TopSolid.Kernel.G.D2 Objects
using D2Point = TopSolid.Kernel.G.D2.Point;
using D2LineCurve = TopSolid.Kernel.G.D2.Curves.LineCurve;
using D2PointList = TopSolid.Kernel.G.D2.PointList;
using D2PolylineCurve = TopSolid.Kernel.G.D2.Curves.PolylineCurve;
using D2Vector = TopSolid.Kernel.G.D2.Vector;

//Others
using D1Interval = TopSolid.Kernel.G.D1.Generic.Interval<double>;
using G = TopSolid.Kernel.G;
using TX = TopSolid.Kernel.TX;
using SX = TopSolid.Kernel.SX;
using TSX = TopSolid.Kernel.SX.Collections.Generic;
#endregion


using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.G.D3.Surfaces;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.TX.Items;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.G.D3.Shapes.Creations;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.G.D1;
using TopSolid.Kernel.G.D3.Shapes.Polyhedrons;
using Speckle.Core.Models;
using TopSolid.Kernel.G.D3.Shapes.Sew;
using TK = TopSolid.Kernel;
using DynamicData;
using TopSolid.Kernel.SX.Collections;
using ItemType = TopSolid.Kernel.TX.Items.ItemType;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Kernel.TX.Attributes;
using TopSolid.Kernel.DB.D3.Axes;
using Objects.Other;
using TopSolid.Kernel.G.D2.Curves.Attributes;
using Speckle.Core.Api;
using TopSolid.Kernel.G.D3.Curves;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.DB.D3.Planes;
using TopSolid.Kernel.DB.D2.Axes;
using TopSolid.Kernel.SX.Collections.Generic;
using TopSolid.Kernel.DB.Layers;
using Avalonia.Controls.Shapes;

using Arc = Objects.Geometry.Arc;
using Ellipse = Objects.Geometry.Ellipse;
using Shape = TopSolid.Kernel.G.D3.Shapes.Shape;
using TopSolid.Kernel.DB.D2.Frames;
using TopSolid.Kernel.DB.D2.Dimensions;
using TopSolid.Kernel.DB.D2;


namespace Objects.Converter.TopSolid
{
  public partial class ConverterTopSolid
  {
    // tolerance for geometry:
    public double tolerance = 0.00001;//modified from 0.000 by AHW

    // Convenience methods:
    #region ConvenienceMethods
    // TODO: Deprecate once these have been added to Objects.sln
    public static double[] D2PointToArray(G.D2.Point pt)
    {
      return new double[] { pt.X, pt.Y, 0 };
    }
    public static double[] D2PointToArray(G.D2.Point pt, G.D3.Plane plane)
    {
      var pt3d = plane.ToAbsolute(pt);
      return new double[] { pt3d.X, pt3d.Y, pt3d.Z };
    }

    public static double[] D3PointToArray(D3Point pt)
    {
      return new double[] { pt.X, pt.Y, pt.Z };
    }

    public static double[] Point2dToArray(G.D2.Point pt)
    {
      return new double[] { pt.X, pt.Y, 0 };
    }
    public D3Point[] PointListToNative(IEnumerable<double> arr, string units)
    {

      var enumerable = arr.ToList();
      if (enumerable.Count % 3 != 0) throw new Speckle.Core.Logging.SpeckleException("Array malformed: length%3 != 0.");

      D3Point[] points = new D3Point[enumerable.Count / 3];
      var asArray = enumerable.ToArray();
      for (int i = 2, k = 0; i < enumerable.Count; i += 3)
        points[k++] = new D3Point(
          ScaleToNative(asArray[i - 2], units),
          ScaleToNative(asArray[i - 1], units),
          ScaleToNative(asArray[i], units));

      return points;
    }
    public static double[] D2PointsToFlatArray(IEnumerable<G.D2.Point> points)
    {
      return points.SelectMany(pt => D2PointToArray(pt)).ToArray();
    }
    public static double[] D2PointsToFlatArray(IEnumerable<G.D2.Point> points, G.D3.Plane plane)
    {
      return (double[])points.SelectMany(pt => D2PointToArray(pt, plane).ToArray());
    }

    public static double[] D3PointsToFlatArray(IEnumerable<D3Point> points)
    {
      return points.SelectMany(pt => D3PointToArray(pt)).ToArray();
    }

    public static System.Collections.Generic.List<double> D2PointsToFlatList(IEnumerable<G.D2.Point> points)
    {
      return points.SelectMany(pt => D2PointToArray(pt)).ToList();
    }
    public static System.Collections.Generic.List<double> D2PointsToFlatList(IEnumerable<G.D2.Point> points, G.D3.Plane plane)
    {
      return points.SelectMany(pt => D2PointToArray(pt, plane)).ToList();
    }

    public static System.Collections.Generic.List<double> D3PointsToFlatList(IEnumerable<D3Point> points)
    {
      return points.SelectMany(pt => D3PointToArray(pt)).ToList();
    }

    public static double[] Points2dToFlatArray(IEnumerable<G.D2.Point> points)
    {
      return points.SelectMany(pt => Point2dToArray(pt)).ToArray();
    }

    public static System.Collections.Generic.List<double> Points2dToFlatList(IEnumerable<G.D2.Point> points)
    {
      return points.SelectMany(pt => Point2dToArray(pt)).ToList();
    }

    private System.Collections.Generic.List<double> GetCorrectKnots(System.Collections.Generic.List<double> knots, int controlPointCount, int degree)
    {
      var correctKnots = knots;
      if (knots.Count == controlPointCount + degree + 1)
      {
        correctKnots.RemoveAt(0);
        correctKnots.RemoveAt(correctKnots.Count - 1);
      }

      return correctKnots;

    }

    public System.Collections.Generic.List<System.Collections.Generic.List<ControlPoint>> ControlPointsToSpeckle(D3BSplineSurface surface, string units = null)
    {
      var u = units ?? ModelUnits;

      var points = new System.Collections.Generic.List<System.Collections.Generic.List<ControlPoint>>();
      int count = 0;
      for (var i = 0; i < surface.UCptsCount; i++)
      {
        var row = new System.Collections.Generic.List<ControlPoint>();
        for (var j = 0; j < surface.VCptsCount; j++)
        {
          var point = surface.CPts[count];
          double weight = 1;
          try
          {
            if (surface.CWts.Count() != 0)
            {
              weight = surface.CWts[count];
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
          }

          row.Add(new ControlPoint(point.X, point.Y, point.Z, weight, u));
          count++;
        }
        points.Add(row);
      }
      return points;
    }

    #endregion

    // Points
    #region Points
    public Point PointToSpeckle(D3Point topSolidpoint, string units = null)
    {
      var u = units ?? ModelUnits;
      Point specklePoint = new Point(topSolidpoint.X, topSolidpoint.Y, topSolidpoint.Z, u);
      SetInstanceParameters(specklePoint, topSolidpoint);
      return specklePoint;
    }


    public Point PointToSpeckleWithTransformation(D3Point topSolidpoint, TK.G.D3.Transform inTransform, string units = null)
    {
      var u = units ?? ModelUnits;
      Point specklePointBefore = new Point(topSolidpoint.X, topSolidpoint.Y, topSolidpoint.Z, u);

      Other.Transform speckleTransform = new Other.Transform(
        new Vector(inTransform.R00, inTransform.R01, inTransform.R02, units),
        new Vector(inTransform.R10, inTransform.R11, inTransform.R12, units),
        new Vector(inTransform.R20, inTransform.R21, inTransform.R22, units), new Vector(inTransform.Tx, inTransform.Ty, inTransform.Tz, units));
      Point specklePoint = new Point();
      specklePointBefore.TransformTo(speckleTransform, out specklePoint);

      SetInstanceParameters(specklePoint, topSolidpoint);
      return specklePoint;
    }

    public Point PointToSpeckle(D2Point topSolidpoint, string units = null)
    {
      var u = units ?? ModelUnits;
      Point specklePoint = new Point(topSolidpoint.X, topSolidpoint.Y, 0, u);
      SetInstanceParameters(specklePoint, topSolidpoint);
      return specklePoint;
    }

    public Point PointToSpeckle(D2Point topSolidpoint, G.D3.Plane plane, string units = null)
    {
      var u = units ?? ModelUnits;
      var absolutepoint = plane.ToAbsolute(topSolidpoint);
      Point specklePoint = new Point(absolutepoint.X, absolutepoint.Y, absolutepoint.Z, u);
      SetInstanceParameters(specklePoint, topSolidpoint);
      return specklePoint;
    }
    public D3Point PointToNative(Point point, string units = null)
    {
      var _point = new D3Point(ScaleToNative(point.x, point.units),
        ScaleToNative(point.y, point.units),
        ScaleToNative(point.z, point.units));
      return _point;
    }
    #endregion

    // Vectors
    #region Vector
    public Vector VectorToSpeckle(D3Vector topSolidVector, string units = null)
    {
      var u = units ?? ModelUnits;
      Vector speckleVector = new Vector(topSolidVector.X, topSolidVector.Y, topSolidVector.Z, u);
      return speckleVector;
    }
    public D3UnitVector UnitVectorToNative(Vector vector)
    {
      return new D3UnitVector(
        ScaleToNative(vector.x, vector.units),
        ScaleToNative(vector.y, vector.units),
        ScaleToNative(vector.z, vector.units));
    }
    public D3Vector VectorToNative(Vector vector)
    {
      return new D3UnitVector(
        ScaleToNative(vector.x, vector.units),
        ScaleToNative(vector.y, vector.units),
        ScaleToNative(vector.z, vector.units));
    }
    #endregion

    // Interval
    #region Interval
    public Interval IntervalToSpeckle(D1Interval interval)
    {
      return new Interval(interval.Start, interval.End);
    }
    public D1Interval IntervalToNative(Interval interval)
    {
      return new D1Interval((double)interval.start, (double)interval.end);
    }
    #endregion

    // Plane
    #region Plane
    public Plane PlaneToSpeckle(TsPlane topSolidPlane, string units = null)
    {
      var u = units ?? ModelUnits;
      Plane specklePlane = new Plane(PointToSpeckle(topSolidPlane.Po, u), VectorToSpeckle(topSolidPlane.Vz, u), VectorToSpeckle(topSolidPlane.Vx, u), VectorToSpeckle(topSolidPlane.Vy, u), u);
      SetInstanceParameters(specklePlane, topSolidPlane);
      return specklePlane;
    }



    public TsPlane PlaneToNative(Plane plane)
    {
      return new TsPlane(PointToNative(plane.origin), UnitVectorToNative(plane.xdir.Unit()), UnitVectorToNative(plane.ydir.Unit()));
    }





    #endregion

    // LineCurve
    #region Line
    public Line LineToSpeckle(D3LineCurve topSolidline, string units = null)
    {
      var u = units ?? ModelUnits;
      Line speckleLine = new Line(PointToSpeckle(topSolidline.Ps), PointToSpeckle(topSolidline.Pe), u);
      SetInstanceParameters(speckleLine, topSolidline);
      return speckleLine;
    }

    public Line LineToSpeckle(D2LineCurve topSolidline, string units = null)
    {
      var u = units ?? ModelUnits;
      Line speckleLine = new Line(PointToSpeckle(topSolidline.Ps), PointToSpeckle(topSolidline.Pe), u);
      SetInstanceParameters(speckleLine, topSolidline);
      return speckleLine;
    }

    public Line LineToSpeckle(D2LineCurve topSolidline, G.D3.Plane plane, string units = null)
    {
      var u = units ?? ModelUnits;
      Line speckleLine = new Line(PointToSpeckle(topSolidline.Ps, plane, u), PointToSpeckle(topSolidline.Pe, plane, u), u);
      SetInstanceParameters(speckleLine, topSolidline);
      return speckleLine;
    }
    public D3LineCurve LineToNative(Line line, bool isReversed = false, string units = null)
    {
      if (isReversed)
        return new D3LineCurve(PointToNative(line.end), PointToNative(line.start));

      return new D3LineCurve(PointToNative(line.start), PointToNative(line.end));
    }
    #endregion


    // Sketch
    #region Sketch
    public Base PlanarSketchToSpeckle(G.D3.Sketches.Planar.PlanarSketch topSolidSketch, string units = null)
    {
      var u = units ?? ModelUnits;

      Base speckleSketch = new Base();

      bool is3d = topSolidSketch.Is3D;
      var localPlane = topSolidSketch.Plane;
      //AHW test to add profiles dynmaically
      System.Collections.Generic.List<Base> list = new System.Collections.Generic.List<Base>();
      TK.DB.D3.Sketches.Planar.PlanarSketchEntity sketchEntity = (TK.DB.D3.Sketches.Planar.PlanarSketchEntity)(topSolidSketch.Owner);
      //TESTAFS
      foreach (var profile in topSolidSketch.Profiles)
      {
        LineStyle defaultLineStyle = new LineStyle();
        SX.Drawing.Color defaultColor = SX.Drawing.Color.Empty;

        if (sketchEntity != null && sketchEntity.HasStyle)
        {
          var styleForSketc = sketchEntity.Style;
          defaultLineStyle = styleForSketc.LineStyle.IsEmpty ? LineStyle.Empty : styleForSketc.LineStyle;
        }
        if (sketchEntity != null)
        {
          defaultColor = sketchEntity.Color;
          if (defaultLineStyle == LineStyle.Empty)
          {
            defaultLineStyle = sketchEntity.LineStyle;
          }
        }

        foreach (var segment in profile.Segments)
        {
          var geoSegm = segment.MakeGeometricProfile(G.D2.Curves.Attributes.AttributeType.Color);

          DisplayStyle displayStyle = new DisplayStyle();
          //ici, si le lineStyleWith est égale à None alors c'est qu'il y a un style
          SX.Drawing.Color colorToUse = (segment.Color.IsEmpty ? defaultColor : segment.Color);
          displayStyle.color = ((System.Drawing.Color)colorToUse).ToArgb();
          LineStyle lineStyleToUse = (segment.LineStyle != LineStyle.Empty) ? segment.LineStyle : defaultLineStyle;

          Tuple<double, string> lineData = GetLineStyleInfos(lineStyleToUse);
          displayStyle.lineweight = lineData.Item1;
          displayStyle.linetype = lineData.Item2;//for now
          displayStyle.units = "mm";

          var obj = ObjectToSpeckle(geoSegm, localPlane);
          //nominations
          if (segment.HasSegmentNameAttribute)//si existe
          {
            obj["segmentName"] = segment.SegmentName;
            obj["segmentNameColor"] = ((System.Drawing.Color)segment.NameColor).ToArgb();
            obj["segmentNameFont"] = segment.NameFontName;

            obj["segmentNameDirectionInverted"] = segment.IsNameLocationReversed;

            this.GetNamePos(segment, out var outPosition, out var outDirection);
            obj["namePosDirSegment"] = new Vector(outDirection.X, outDirection.Y);
            obj["namePosPointSegment"] = PointToSpeckle(outPosition);
          }

          obj["IsInternal"] = segment.IsInternal ? "true" : "false";
          obj["IsSketch"] = "yes";
          obj["displayStyle"] = displayStyle;
          obj["units"] = u;
          list.Add(obj);
        }
      }

      //deal with dimensions, testing for linear only
      //EDIT 13-05-2025: DOES NOT WORK => REMOVED

      foreach (Entity entity in sketchEntity.Entities)
      {
        if (entity is TK.DB.D2.Dimensions.LinearDimensionEntity dimension)
        {
          DistanceDimension dim = new DistanceDimension();
          dim.position = PointToSpeckle(dimension.FirstTopPoint, localPlane);
          dim.richText = @"{\rtf1\deff0{\fonttbl{\f0 Arial;}}\f0 \fs11{\f0 " + dimension.TextString + "mm}}";
          dim.measurement = dimension.MeasuredValue;
          dim.units = "mm";
          dim.isOrdinate = false;
          dim.value = dimension.TextString + " mm";
          dim.textPosition = PointToSpeckle(new D2Point((dimension.FirstTopPoint.X + dimension.SecondTopPoint.X) * 0.5, (dimension.FirstTopPoint.Y + dimension.SecondTopPoint.Y) * 0.5), localPlane);
          var firstPoint = PointToSpeckle(dimension.SecondTopPoint, localPlane);
          var secondPoint = PointToSpeckle(dimension.FirstTopPoint, localPlane);
          dim.direction = new Vector(secondPoint.x - firstPoint.x, secondPoint.y - firstPoint.y, secondPoint.z - firstPoint.z);
          DisplayStyle dimDisplayStyle = new DisplayStyle { linetype = "Continuous", units = "mm", lineweight = 0, color = System.Drawing.Color.Red.ToArgb() };
          dim["displayStyle"] = dimDisplayStyle;
          System.Collections.Generic.List<ICurve> displayValue = new System.Collections.Generic.List<ICurve>();
          System.Collections.Generic.List<D2LineCurve> curves = GetDisplayLines(dimension);
          dim.displayValue = curves.Select(l => LineToSpeckle(l, localPlane) as ICurve).ToList();
          dim.measured = new System.Collections.Generic.List<Point> { PointToSpeckle(dimension.FirstTopPoint, localPlane), PointToSpeckle(dimension.SecondTopPoint, localPlane) };
          dim["renderMaterial"] = RenderMaterialToSpeckle(dimension);
          dim["height"] = "11";//ne marche pas
          dim["IsInternal"] = dimension.IsInternal;

          #region ajout du texte -- commenté le 09-06-2025
          //Text speckleText = new Text();
          //speckleText.height = 0.05;
          //speckleText.richText = @"{\rtf1\deff0{\fonttbl{\f0 Arial;}}\f0 \fs11{\f0 " + dimension.TextString + "mm}}";
          //var planetouse= PlaneToSpeckle(dimension.Plane);    
          //speckleText.value = dimension.TextString + " mm";
          //speckleText.units = "mm";
          //speckleText["renderMaterial"] = RenderMaterialToSpeckle(dimension);
          //dimension.FindPointsOnLineAxis(out D2Point firstPoint, out D2Point secondPoint);
          //planetouse.origin = /*PointToSpeckle(firstPoint);*/PointToSpeckle(new D2Point((firstPoint.X + secondPoint.X) * 0.5, (firstPoint.Y + secondPoint.Y) * 0.5));
          ////il faudrait sans doute translaterl'origine
          //double rotation = G.D2.Vector.VX.GetAngle(new D2Vector(secondPoint, firstPoint), true);//angle en radians
          //planetouse.xdir = new Vector(planetouse.xdir.Length * Math.Cos(rotation), planetouse.xdir.Length*Math.Sin(rotation));
          //planetouse.ydir = new Vector(planetouse.ydir.Length * Math.Cos(Math.PI/2+rotation), planetouse.ydir.Length * Math.Sin(Math.PI/2+rotation));
          //speckleText.plane = planetouse;
          //speckleText.rotation = 0;
          //speckleText["displayStyle"] = dimDisplayStyle;
          //SetInstanceParameters(speckleText, dimension);
          //list.Add(speckleText);
          #endregion

          list.Add(dim);
        }
      }


      var vertices = topSolidSketch.Vertices.Where(y => !y.IsInternal).Select(x => ObjectToSpeckle(x)).ToList();
      speckleSketch["Profiles"] = list;
      speckleSketch["Vertices"] = vertices;
      speckleSketch["isSketch"] = true;

      return speckleSketch;

    }


    private System.Collections.Generic.List<D2LineCurve> GetDisplayLines(TK.DB.D2.Dimensions.LinearDimensionEntity dimension)
    {
      //D2LineCurve lineCurve = GetDimensionLine(dimension,dimension.GetCalloutTextPosition();
      System.Collections.Generic.List<D2LineCurve> curves = new System.Collections.Generic.List<D2LineCurve>();
      foreach (var f in dimension.Display.Items)
      {
        if (f.IsCurveItem)
        {
          G.D2.CurveGeometryType geometryType = G.D2.CurveGeometryType.Line;
          var dimensionLine = dimension.SearchItemCurve(f.Label, geometryType);
          if (dimensionLine is D2LineCurve dimensionLineCurve)
          {
            curves.Add(dimensionLineCurve);
          }
        }
      }
      return curves;
    }

    /// <summary>
    /// Gets the segment name position.
    /// </summary>
    /// <param name="outPosition">Position.</param>
    /// <param name="outDirection">Direction.</param>
    /// <remarks>This method does not take <see cref="IsNameLocationReversed"/> property into account.</remarks>
    internal bool GetNamePos(G.D2.Sketches.Segment inSegment, out D2Point outPosition, out G.D2.UnitVector outDirection)
    {
      if (inSegment.Geometry == null)
      {
        outPosition = G.D2.Point.P0;
        outDirection = G.D2.UnitVector.VX;
        return false;
      }

      double t = inSegment.NamePosParam;
      if (inSegment.Geometry.Range.IsFinite)
        t = inSegment.Geometry.GetDenormalized(t);

      outPosition = inSegment.Geometry.GetPoint(t);
      outDirection = inSegment.Geometry.GetTangent(t);
      return true;
    }

    /// <summary>
		/// Gets the segment name position.
		/// </summary>
		/// <param name="outPosition">Position.</param>
		/// <param name="outDirection">Direction.</param>
		/// <remarks>This method does not take <see cref="IsNameLocationReversed"/> property into account.</remarks>
		internal bool GetNamePos(G.D3.Sketches.Segment inSegment, out D3Point outPosition, out G.D3.UnitVector outDirection)
    {
      if (inSegment.Geometry == null)
      {
        outPosition = G.D3.Point.P0;
        outDirection = G.D3.UnitVector.VX;
        return false;
      }

      double t = inSegment.NamePosParam;
      if (inSegment.Geometry.Range.IsFinite)
        t = inSegment.Geometry.GetDenormalized(t);

      outPosition = inSegment.Geometry.GetPoint(t);
      outDirection = inSegment.Geometry.GetTangent(t);
      return true;
    }

    #region Plane
    public Polycurve BoundedPlaneToSpeckle(BoundedPlane planeGeometry, string units = null)
    {
      var u = units ?? ModelUnits;

      D2PointList corners = new D2PointList();
      planeGeometry.Extent.GetCorners(true, corners);

      Polycurve polyCurve = new Polycurve();

      for (int i = 0; i <= corners.Count - 1; i++)
      {
        D3Point pS = planeGeometry.Plane.ToAbsolute(corners[i]);
        D3Point pE = planeGeometry.Plane.ToAbsolute((i != 3 ? corners[i + 1] : corners[0]));

        D3LineCurve d3Line = new D3LineCurve(pS, pE);
        Line speckleLine = LineToSpeckle(d3Line);
        polyCurve.segments.Add(LineToSpeckle(d3Line));
      }
      polyCurve.units = u;

      return polyCurve;
    }

    public Polycurve PlaneGeometryToSpeckle(PlaneEntity planeEntity, string units = null)
    {
      var u = units ?? ModelUnits;

      var planeGeometry = planeEntity.BoundedGeometry;
      Polycurve polyCurve = BoundedPlaneToSpeckle(planeGeometry, units);

      DisplayStyle displayStyle = new DisplayStyle();
      displayStyle.color = ((System.Drawing.Color)planeEntity.Color).ToArgb();
      displayStyle.lineweight = 0;
      displayStyle.linetype = "Continuous";
      displayStyle.units = u;

      SetInstanceParameters(polyCurve, planeEntity);
      polyCurve["displayStyle"] = displayStyle;

      return polyCurve;

    }


    #endregion

    /// <summary>
    /// Gets LineStyleInfos
    /// </summary>
    /// <param name="inLineStyle"></param>
    /// <returns>A tuple holding linewidth and linetype</returns>
    private static Tuple<double, string> GetLineStyleInfos(LineStyle inLineStyle)
    {
      double lineweight = 0;
      switch (inLineStyle.Width)
      {
        case LineWidth.Custom:
        case LineWidth.None:
          lineweight = 0;
          break;
        case LineWidth.Thin:
          lineweight = 1;
          break;
        case LineWidth.Medium:
          lineweight = 3;
          break;
        case LineWidth.Thick:
          lineweight = 4;
          break;
        case LineWidth.ExtraThick:
          lineweight = 5;
          break;
        case LineWidth.MediumThin:
          lineweight = 2;
          break;
        default:
          break;
      }

      string linetype = "Continuous";
      switch (inLineStyle.Type)
      {
        case LineType.None:
          linetype = "Continuous";
          break;
        case LineType.Custom:
          linetype = "Custom";
          break;
        case LineType.Solid:
          linetype = "Solid";
          break;
        case LineType.Dash:
          linetype = "Dash";
          break;
        case LineType.Dot:
          linetype = "Dot";
          break;
        case LineType.DashDot:
          linetype = "DashDot";
          break;
        case LineType.DashDotDot:
          linetype = "DashDotDot";
          break;
        case LineType.ShortDash:
        default:
          linetype = "Continuous";
          break;
      }

      return Tuple.Create(lineweight, linetype);
    }

    public G.D3.Sketches.Planar.PlanarSketch PlanarSketchToNative(Line line, string units = null)
    {
      return null;
    }

    #region Frame

    public Base FrameToSpeckle(TK.DB.D3.Frames.FrameEntity frameEntity, string units = null)
    {
      var u = units ?? ModelUnits;

      Base speckleFrame = new Base();

      var frameBoundedGeometry = frameEntity.BoundedGeometry;
      var XY = BoundedPlaneToSpeckle(frameBoundedGeometry.Pxy);
      var YZ = BoundedPlaneToSpeckle(frameBoundedGeometry.Pyz);
      var XZ = BoundedPlaneToSpeckle(frameBoundedGeometry.Pxz);

      DisplayStyle displayStyle = new DisplayStyle();
      displayStyle.lineweight = 0;
      displayStyle.units = "mm";
      displayStyle.linetype = "Continuous";
      displayStyle.color = (System.Drawing.Color.Red).ToArgb();
      XY["displayStyle"] = displayStyle;
      XY["FrameDir"] = "XY";
      XY["FrameId"] = frameEntity.Id.ToString();
      XY["FrameVX"] = frameBoundedGeometry.Ax.ToString();

      DisplayStyle displayStyle2 = new DisplayStyle();
      displayStyle2.lineweight = 0;
      displayStyle2.units = "mm";
      displayStyle2.linetype = "Continuous";
      displayStyle2.color = (System.Drawing.Color.Green).ToArgb();
      YZ["displayStyle"] = displayStyle2;
      YZ["FrameDir"] = "YZ";
      YZ["FrameId"] = frameEntity.Id.ToString();
      YZ["FrameVY"] = frameBoundedGeometry.Ay.ToString();

      DisplayStyle displayStyle3 = new DisplayStyle();
      displayStyle3.lineweight = 0;
      displayStyle3.units = "mm";
      displayStyle3.linetype = "Continuous";
      displayStyle3.color = (System.Drawing.Color.Blue).ToArgb();
      XZ["displayStyle"] = displayStyle3;
      XZ["FrameDir"] = "XZ";
      XZ["FrameId"] = frameEntity.Id.ToString();
      XZ["FrameVZ"] = frameBoundedGeometry.Az.ToString();

      System.Collections.Generic.List<Base> list = new System.Collections.Generic.List<Base> { XY, YZ, XZ };

      Point origoPointSpeckle = PointToSpeckle(frameBoundedGeometry.Center);
      origoPointSpeckle["FrameId"] = frameEntity.Id.ToString();
      list.Add(origoPointSpeckle);

      speckleFrame["Profiles"] = list;
      speckleFrame["IsFrame"] = true;
      speckleFrame["FrameId"] = frameEntity.Id.ToString();

      SetInstanceParameters(speckleFrame, frameEntity);
      return speckleFrame;

    }
    #endregion

    #region Axis
    public Base AxisToSpeckle(TK.DB.D3.Axes.AxisEntity axisEntity, string units = null)
    {
      var u = units ?? ModelUnits;

      var Pe = (axisEntity.Geometry.Po + axisEntity.Geometry.Vx);
      Line speckleLine = new Line(PointToSpeckle(axisEntity.Geometry.Po), PointToSpeckle(Pe), u);
      //Line speckleLine = new Line(PointToSpeckle(axisEntity.Display.GetExtent().Min), PointToSpeckle(axisEntity.Display.GetExtent().Max), u);
      speckleLine["IsAxis"] = true;
      speckleLine["renderMaterial"] = RenderMaterialToSpeckle(axisEntity);

      //style displau
      DisplayStyle displayStyle = new DisplayStyle();
      displayStyle.lineweight = 0;
      displayStyle.units = null;
      displayStyle.linetype = "DashDot";
      displayStyle.color = ((System.Drawing.Color)axisEntity.Color).ToArgb();


      speckleLine["displayStyle"] = displayStyle;
      SetInstanceParameters(speckleLine, axisEntity);
      return speckleLine;

    }

    public Base AxisToSpeckle(TK.DB.D2.Axes.AxisEntity axisEntity, string units = null)
    {
      var u = units ?? ModelUnits;

      var Pe = (axisEntity.Geometry.Po + axisEntity.Geometry.Vx);
      Line speckleLine = new Line(PointToSpeckle(axisEntity.Geometry.Po), PointToSpeckle(Pe), u);
      //Line speckleLine = new Line(PointToSpeckle(axisEntity.Display.GetExtent().Min), PointToSpeckle(axisEntity.Display.GetExtent().Max), u);
      speckleLine["IsAxis"] = true;
      speckleLine["renderMaterial"] = RenderMaterialToSpeckle(axisEntity);
      //style displau
      DisplayStyle displayStyle = new DisplayStyle();
      displayStyle.lineweight = 0;
      displayStyle.linetype = "DashDot";
      displayStyle.color = ((System.Drawing.Color)axisEntity.Color).ToArgb();


      speckleLine["displayStyle"] = displayStyle;
      SetInstanceParameters(speckleLine, axisEntity);
      return speckleLine;


    }
    #endregion

    #region Positioned Sketch
    public Base PositionedSketchToSpeckle(G.D3.Sketches.PositionedSketch topSolidSketch, string units = null)
    {
      var u = units ?? ModelUnits;

      Base speckleSketch = new Base();

      var haslocalPlane = topSolidSketch.HasPlanarGeometricSection(G.Precision.ModelingLinearTolerance, G.Precision.ModelingAngularTolerance, out TsPlane localPlane);

      if (haslocalPlane)
      {
        //AHW test to add profiles dynmaically
        System.Collections.Generic.List<Base> list = new System.Collections.Generic.List<Base>();

        //TESTAFS
        foreach (var profile in topSolidSketch.Profiles)
        {
          LineStyle defaultLineStyle = new LineStyle();
          SX.Drawing.Color defaultColor = SX.Drawing.Color.Empty;
          TK.DB.D3.Sketches.PositionedSketchEntity sketchEntity = (TK.DB.D3.Sketches.PositionedSketchEntity)(topSolidSketch.Owner);
          if (sketchEntity != null && sketchEntity.HasStyle)
          {
            var styleForSketc = sketchEntity.Style;
            defaultLineStyle = styleForSketc.LineStyle.IsEmpty ? LineStyle.Empty : styleForSketc.LineStyle;
          }
          if (sketchEntity != null)
          {
            defaultColor = sketchEntity.Color;
            if (defaultLineStyle == LineStyle.Empty)
            {
              defaultLineStyle = sketchEntity.LineStyle;
            }
          }

          foreach (var segment in profile.Segments)
          {
            var geoSegm = segment.MakeGeometricProfile(G.D2.Curves.Attributes.AttributeType.Color);


            DisplayStyle displayStyle = new DisplayStyle();
            //ici, si le lineStyleWith est égale à None alors c'est qu'il y a un style
            SX.Drawing.Color colorToUse = (segment.Color.IsEmpty ? defaultColor : segment.Color);
            displayStyle.color = ((System.Drawing.Color)colorToUse).ToArgb();
            LineStyle lineStyleToUse = (segment.LineStyle != LineStyle.Empty) ? segment.LineStyle : defaultLineStyle;

            Tuple<double, string> lineData = GetLineStyleInfos(lineStyleToUse);
            displayStyle.lineweight = lineData.Item1;
            displayStyle.linetype = lineData.Item2;//for now
            displayStyle.units = "mm";


            //localPlane.TransformByInverse(topSolidSketch.Frame.GetPositioningTransform());

            var obj = ObjectToSpeckle(geoSegm, topSolidSketch.Frame.Pxy /*localPlane*/);
            //nominations
            if (segment.HasSegmentNameAttribute)//si existe
            {
              obj["segmentName"] = segment.SegmentName;

              obj["segmentNameDirectionInverted"] = (!segment.NameIsFirstDirectionInverted) ? "No" : "Yes";
              if (segment.NameIsFirstDirectionInverted)
              {
                obj["segmentNameIsDirectionXPlus"] = (segment.NameIsSecondDirectionX && !segment.NameIsSecondDirectionInverted).ToString();
                obj["segmentNameIsDirectionXMoins"] = (segment.NameIsSecondDirectionX && segment.NameIsSecondDirectionInverted).ToString();
                obj["segmentNameIsDirectionYPlus"] = (segment.NameIsSecondDirectionY && !segment.NameIsSecondDirectionInverted).ToString();
                obj["segmentNameIsDirectionYMoins"] = (segment.NameIsSecondDirectionY && segment.NameIsSecondDirectionInverted).ToString();
                obj["segmentNameIsDirectionZPlus"] = (segment.NameIsSecondDirectionZ && !segment.NameIsSecondDirectionInverted).ToString();
                obj["segmentNameIsDirectionZMoins"] = (segment.NameIsSecondDirectionZ && segment.NameIsSecondDirectionInverted).ToString();
              }

              obj["segmentNameColor"] = ((System.Drawing.Color)segment.NameColor).ToArgb();
              obj["segmentNameFont"] = segment.NameFontName;
              this.GetNamePos(segment, out var outPosition, out var outDirection);
              obj["namePosDirSegment"] = new Vector(outDirection.X, outDirection.Y);
              obj["namePosPointSegment"] = PointToSpeckle(outPosition);
            }

            obj["IsInternal"] = segment.IsInternal ? "true" : "false";
            obj["IsSketch"] = "yes";
            obj["displayStyle"] = displayStyle;
            obj["units"] = u;
            list.Add(obj);
          }

          //deal with dimensions, testing for linear only
          //EDIT 13-05-2025: DOES NOT WORK => REMOVED


          foreach (Entity entity in sketchEntity.Entities)
          {
            if (entity is TK.DB.D2.Dimensions.LinearDimensionEntity dimension)
            {
              DistanceDimension dim = new DistanceDimension();
              TsPlane planeToUse = new TsPlane(dimension.Plane.Frame);
              dim.position = PointToSpeckle(dimension.FirstTopPoint, planeToUse);

              dim.richText = @"{\rtf1\deff0{\fonttbl{\f0 Arial;}}\f0 \fs11{\f0 " + dimension.TextString + "mm}}";
              dim.measurement = dimension.MeasuredValue;
              dim.units = "mm";
              dim.isOrdinate = false;
              dim.value = dimension.TextString + " mm";
              dim.textPosition = PointToSpeckle(new D2Point((dimension.FirstTopPoint.X + dimension.SecondTopPoint.X) * 0.5, (dimension.FirstTopPoint.Y + dimension.SecondTopPoint.Y) * 0.5), planeToUse);
              var firstPoint = PointToSpeckle(dimension.FirstTopPoint, planeToUse);
              var secondPoint = PointToSpeckle(dimension.SecondTopPoint, planeToUse);
              //dim.direction=
              dim.direction = new Vector(secondPoint.x - firstPoint.x, secondPoint.y - firstPoint.y, secondPoint.z - firstPoint.z);
              DisplayStyle dimDisplayStyle = new DisplayStyle { linetype = "Continuous", units = "mm", lineweight = 0, color = System.Drawing.Color.Red.ToArgb() };
              dim["displayStyle"] = dimDisplayStyle;
              System.Collections.Generic.List<ICurve> displayValue = new System.Collections.Generic.List<ICurve>();
              System.Collections.Generic.List<D2LineCurve> curves = GetDisplayLines(dimension);
              dim.displayValue = curves.Select(l => LineToSpeckle(l, planeToUse) as ICurve).ToList();
              dim.measured = new System.Collections.Generic.List<Point> { PointToSpeckle(dimension.FirstTopPoint, planeToUse), PointToSpeckle(dimension.SecondTopPoint, planeToUse) };
              dim["renderMaterial"] = RenderMaterialToSpeckle(dimension);
              dim["height"] = "11";//ne marche pas
              dim["IsInternal"] = dimension.IsInternal;


              #region ajout du texte -- commenté le 09-06-2025
              //Text speckleText = new Text();
              //speckleText.height = 0.05;
              //speckleText.richText = @"{\rtf1\deff0{\fonttbl{\f0 Arial;}}\f0 \fs11{\f0 " + dimension.TextString + "mm}}";
              //var planetouse= PlaneToSpeckle(dimension.Plane);    
              //speckleText.value = dimension.TextString + " mm";
              //speckleText.units = "mm";
              //speckleText["renderMaterial"] = RenderMaterialToSpeckle(dimension);
              //dimension.FindPointsOnLineAxis(out D2Point firstPoint, out D2Point secondPoint);
              //planetouse.origin = /*PointToSpeckle(firstPoint);*/PointToSpeckle(new D2Point((firstPoint.X + secondPoint.X) * 0.5, (firstPoint.Y + secondPoint.Y) * 0.5));
              ////il faudrait sans doute translaterl'origine
              //double rotation = G.D2.Vector.VX.GetAngle(new D2Vector(secondPoint, firstPoint), true);//angle en radians
              //planetouse.xdir = new Vector(planetouse.xdir.Length * Math.Cos(rotation), planetouse.xdir.Length*Math.Sin(rotation));
              //planetouse.ydir = new Vector(planetouse.ydir.Length * Math.Cos(Math.PI/2+rotation), planetouse.ydir.Length * Math.Sin(Math.PI/2+rotation));
              //speckleText.plane = planetouse;
              //speckleText.rotation = 0;
              //speckleText["displayStyle"] = dimDisplayStyle;
              //SetInstanceParameters(speckleText, dimension);
              //list.Add(speckleText);
              #endregion

              list.Add(dim);
            }
          }

        }

        var vertices = topSolidSketch.Vertices.Where(y => !y.IsInternal).Select(x => ObjectToSpeckle(x)).ToList();
        speckleSketch["Profiles"] = list;
        speckleSketch["Vertices"] = vertices;
        speckleSketch["isSketch"] = true;
      }

      return speckleSketch;
    }
    #endregion

    #endregion



    // PolylineCurve
    #region Polyline
    public Polyline PolyLineToSpeckle(D3PolylineCurve topSolidPolyline, string units = null)
    {

      var u = units ?? ModelUnits;
      System.Collections.Generic.List<double> _coordinates = new System.Collections.Generic.List<double>();

      D3PointList pts = topSolidPolyline.CPts;

      foreach (D3Point p in pts)
      {
        Point _point = PointToSpeckle(p);
        _coordinates.Add(_point.x);
      }

      Polyline specklePolyline = new Polyline(_coordinates, u);
      SetInstanceParameters(specklePolyline, topSolidPolyline);
      return specklePolyline;

    }
    public Polyline PolyLineToSpeckle(D2PolylineCurve topSolidPolyline, string units = null)
    {

      var u = units ?? ModelUnits;
      System.Collections.Generic.List<double> _coordinates = new System.Collections.Generic.List<double>();

      D2PointList pts = topSolidPolyline.CPts;

      foreach (D2Point p in pts)
      {
        Point _point = PointToSpeckle(p);
        _coordinates.Add(_point.x);
      }

      Polyline specklePolyline = new Polyline(_coordinates, u);
      SetInstanceParameters(specklePolyline, topSolidPolyline);
      return specklePolyline;

    }

    public Polyline PolyLineToSpeckle(D2PolylineCurve topSolidPolyline, G.D3.Plane plane
      , string units = null)
    {

      var u = units ?? ModelUnits;
      System.Collections.Generic.List<double> _coordinates = new System.Collections.Generic.List<double>();

      D2PointList pts = topSolidPolyline.CPts;

      foreach (D2Point p in pts)
      {
        Point _point = PointToSpeckle(p, plane, u);
        _coordinates.Add(_point.x);
      }

      Polyline specklePolyline = new Polyline(_coordinates, u);
      SetInstanceParameters(specklePolyline, topSolidPolyline);
      return specklePolyline;

    }

    public D3PolylineCurve PolyLineToNative(Polyline polyLine, string units = null)
    {

      D3PointList _pointsList = new D3PointList();

      foreach (Point p in polyLine.GetPoints())
      {
        D3Point _point = PointToNative(p, units);
        _pointsList.Add(_point);
      }

      return new D3PolylineCurve(polyLine.closed, _pointsList);

    }
    #endregion

    //Curve 2D & 3D
    #region Curve
    public Polycurve ProfileToSpeckle(G.D2.Curves.GeometricProfile profile, string units = null)
    {
      var u = units ?? ModelUnits;
      Polycurve polyCurve = new Polycurve();
      polyCurve.segments = profile.Segments.Select(x => CurveToSpeckle(x.GetOrientedCurve().Curve)).ToList();
      polyCurve.units = u;

      return polyCurve;
    }
    public Polycurve ProfileToSpeckle(G.D2.Curves.GeometricProfile profile, G.D3.Plane plane, string units = null)
    {
      var u = units ?? ModelUnits;
      Polycurve polyCurve = new Polycurve();

      polyCurve.segments = profile.Segments.Select(x => CurveToSpeckle(x.GetOrientedCurve().Curve, plane)).ToList();
      polyCurve.units = u;

      return polyCurve;
    }

    public Polycurve ProfileToSpeckle(G.D3.Curves.GeometricProfile profile, G.D3.Plane plane, string units = null)
    {
      var u = units ?? ModelUnits;

      Polycurve polyCurveBefore = new Polycurve();
      polyCurveBefore.segments = profile.Segments.Select(x => CurveToSpeckle(x.GetOrientedCurve().Curve.MakeTransformedCurve(SX.Version.Current, plane.GetTransform(), G.Precision.ModelingLinearTolerance))).ToList();
      polyCurveBefore.units = u;

      return polyCurveBefore;
    }

    public Polycurve ProfileToSpeckle(G.D3.Curves.GeometricProfile profile, string units = null)
    {
      var u = units ?? ModelUnits;
      Polycurve polyCurve = new Polycurve();
      polyCurve.segments = profile.Segments.Select(x => CurveToSpeckle(x.GetOrientedCurve().Curve)).ToList();
      polyCurve.units = u;
      return polyCurve;
    }

    //Arc      
    public G.D3.Curves.CircleCurve ArcToNative(Geometry.Arc arc, string units = null)
    {
      //var plane = PlaneToNative(arc.plane);
      G.D3.Curves.CircleCurve circleCurve = new G.D3.Curves.CircleCurve(PlaneToNative(arc.plane), ScaleToNative((double)arc.radius, arc.units));
      G.D3.Curves.CircleMaker maker = new G.D3.Curves.CircleMaker(SX.Version.Current, tolerance, global::TopSolid.Kernel.G.Precision.AngularPrecision);
      maker.SetByCenterAndTwoPoints(
          PointToNative(arc.plane.origin),
          PointToNative(arc.startPoint),
          PointToNative(arc.endPoint),
          false,
          UnitVectorToNative(arc.plane.normal.Unit()),
          circleCurve);

      return circleCurve;

    }

    public ICurve CurveToSpeckle(G.D2.Curves.Curve curve, string units = null)
    {
      var u = units ?? ModelUnits;
      switch (curve)
      {
        case G.D2.Curves.BSplineCurve bspline:
          return BSplineCurveToSpeckle(bspline, u);
        case G.D2.Curves.CircleCurve circle:
          if (circle.IsClosed())
            return CircleToSpeckle(circle, u);
          else
            return ArcToSpeckle(circle, u);
        case G.D2.Curves.LineCurve line:
          return LineToSpeckle(line, u);
        case G.D2.Curves.PolylineCurve poly:
          return PolyLineToSpeckle(poly, u);
        default:
          return BSplineCurveToSpeckle(curve.GetBSplineCurve(false, false));
      }

    }
    public ICurve CurveToSpeckle(G.D2.Curves.Curve curve, G.D3.Plane plane, string units = null)
    {

      var u = units ?? ModelUnits;
      switch (curve)
      {
        case G.D2.Curves.BSplineCurve bspline:
          return BSplineCurveToSpeckle(bspline, plane, u);
        case G.D2.Curves.CircleCurve circle:
          if (circle.IsClosed())
            return CircleToSpeckle(circle, plane, u);
          else
            return ArcToSpeckle(circle, plane, u);
        case G.D2.Curves.LineCurve line:
          return LineToSpeckle(line, plane, u);
        case G.D2.Curves.PolylineCurve poly:
          return PolyLineToSpeckle(poly, plane, u);
        default:
          return BSplineCurveToSpeckle(curve.GetBSplineCurve(false, false), plane, u);
      }

    }


    public ICurve CurveToSpeckle(G.D3.Curves.Curve curve, string units = null)
    {
      var u = units ?? ModelUnits;
      switch (curve)
      {
        case G.D3.Curves.BSplineCurve bspline:
          return D3BSplineCurveToSpeckle(bspline, u);
        case G.D3.Curves.CircleCurve circle:
          if (circle.IsClosed())
            return CircleToSpeckle(circle, u);
          else
            return ArcToSpeckle(circle, u);
        case G.D3.Curves.LineCurve line:
          return LineToSpeckle(line, u);
        case G.D3.Curves.PolylineCurve poly:
          return PolyLineToSpeckle(poly, u);
        default:
          return D3BSplineCurveToSpeckle(curve.GetBSplineCurve(false, false));
      }

    }

    public Circle CircleToSpeckle(G.D2.Curves.CircleCurve circ, string units = null)
    {
      var u = units ?? ModelUnits;
      var circle = new Circle(PlaneToSpeckle((TsPlane)circ.Frame, u), circ.Radius, u);
      circle.domain = new Interval(0, 1);
      circle.length = 2 * Math.PI * circ.Radius;
      circle.area = Math.PI * circ.Radius * circ.Radius;
      circle.plane.origin = PointToSpeckle(circ.Center);
      G.D3.Extent box = (G.D3.Extent)circ.GetBoundingBox();
      circle.bbox = new Box(circle.plane, new Interval(box.XMin, box.XMax), new Interval(box.YMin, box.YMax), new Interval(box.ZMin, box.ZMax));
      return circle;
    }
    public Circle CircleToSpeckle(G.D2.Curves.CircleCurve circ, G.D3.Plane plane, string units = null)
    {
      var u = units ?? ModelUnits;
      //var circAbsPlane = plane.ToAbsolute(circ.Frame.ToAbsolute(fr);

      var circle = new Circle(PlaneToSpeckle(plane, u), circ.Radius, u);
      circle.domain = new Interval(0, 1);
      circle.length = 2 * Math.PI * circ.Radius;
      circle.area = Math.PI * circ.Radius * circ.Radius;
      circle.plane.origin = PointToSpeckle(circ.Center, plane);
      G.D3.Extent box = (G.D3.Extent)circ.GetBoundingBox();
      circle.bbox = new Box(circle.plane, new Interval(box.XMin, box.XMax), new Interval(box.YMin, box.YMax), new Interval(box.ZMin, box.ZMax));
      return circle;
    }

    public Circle CircleToSpeckle(G.D3.Curves.CircleCurve circ, string units = null)
    {
      var u = units ?? ModelUnits;
      var circle = new Circle(PlaneToSpeckle(circ.Plane, u), circ.Radius, u);
      circle.domain = new Interval(0, 1);
      circle.length = 2 * Math.PI * circ.Radius;
      circle.area = Math.PI * circ.Radius * circ.Radius;

      return circle;
    }

    public Geometry.Arc ArcToSpeckle(G.D3.Curves.CircleCurve a, string units = null)
    {
      var u = units ?? ModelUnits;

      double angle = (new D3Vector(a.Center, a.Ps)).GetAngle(new D3Vector(a.Center, a.Pe));
      Geometry.Arc arc = new Geometry.Arc(PlaneToSpeckle(a.Plane), PointToSpeckle(a.Ps), PointToSpeckle(a.Pe), angle);

      arc.midPoint = PointToSpeckle(a.Pm, u);
      arc.domain = new Interval(0, 1);
      arc.length = a.GetLength();
      //arc.bbox = BoxToSpeckle(new RH.Box(a.BoundingBox()), u);
      return arc;
    }



    public Arc ArcToSpeckle(G.D2.Curves.CircleCurve a, string units = null)
    {

      var u = units ?? ModelUnits;

      double angle = (new D2Vector(a.Center, a.Ps)).GetAngle(new D2Vector(a.Center, a.Pe), false);
      Arc arc = new Arc(PlaneToSpeckle((TsPlane)a.Frame), PointToSpeckle(a.Ps), PointToSpeckle(a.Pe), angle);

      arc.midPoint = PointToSpeckle(a.Pm, u);
      arc.domain = new Interval(0, 1);
      arc.length = a.GetLength();
      //arc.bbox = BoxToSpeckle(new RH.Box(a.BoundingBox()), u);
      return arc;
    }
    public Arc ArcToSpeckle(G.D2.Curves.CircleCurve a, G.D3.Plane plane, string units = null)
    {

      var u = units ?? ModelUnits;
      var vec1 = new D2Vector(a.Center, a.Ps);
      var vec2 = new D2Vector(a.Center, a.Pe);

      double angle = vec1.GetAngle(vec2, false);

      Arc arc = new Arc(PlaneToSpeckle((plane)), PointToSpeckle(a.Ps, plane, u), PointToSpeckle(a.Pe, plane, u), angle);

      arc.midPoint = PointToSpeckle(a.Pm, plane, u);
      arc.domain = new Interval(0, 1);
      arc.length = a.GetLength();
      //arc.bbox = BoxToSpeckle(new RH.Box(a.BoundingBox()), u);
      return arc;
    }



    public Objects.Geometry.Curve BSplineCurveToSpeckle(G.D2.Curves.BSplineCurve topSolidCurve, string units = null)
    {
      Curve speckleCurve = new Curve();
      var u = units ?? ModelUnits;


      //Weights
      System.Collections.Generic.List<double> ptWeights = new System.Collections.Generic.List<double>();
      try
      {
        if (topSolidCurve.CWts.Count != 0)
        {
          foreach (double weight in topSolidCurve.CWts)
          {
            ptWeights.Add(weight);
          }
        }
      }
      catch { }

      try
      {
        double range = (topSolidCurve.Te - topSolidCurve.Ts);
        G.D2.PointList polyPoints = new G.D2.PointList();
        for (int i = 0; i < 100; i++)
        {
          polyPoints.Add(topSolidCurve.GetPoint((range / 100) * i));
        }
        Polyline displayValue = new Polyline();
        displayValue.value = D2PointsToFlatList(polyPoints);
        displayValue.units = u;
        displayValue.closed = false;

        speckleCurve.displayValue = displayValue;
      }
      catch { }

      //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
      System.Collections.Generic.List<double> knots = new System.Collections.Generic.List<double>();

      for (int i = 0; i < (topSolidCurve.Bs.Count); i++)
      {
        knots.Add(topSolidCurve.Bs.ElementAt(i));

      }

      //Prevent errors when weight list is empty
      if (topSolidCurve.CWts.Count == 0)
      {
        ptWeights.Clear();
        for (int i = 0; i < topSolidCurve.CPts.Count; i++)
        {
          ptWeights.Add(1.0);
        }
      }

      Interval interval = new Interval(topSolidCurve.Ts, topSolidCurve.Te);

      //set speckle curve info
      speckleCurve.points = D2PointsToFlatArray(topSolidCurve.CPts).ToList();
      speckleCurve.knots = knots;
      speckleCurve.weights = ptWeights;
      speckleCurve.degree = topSolidCurve.Degree;
      speckleCurve.periodic = topSolidCurve.IsPeriodic;
      speckleCurve.rational = topSolidCurve.IsRational;
      speckleCurve.closed = topSolidCurve.IsClosed();
      speckleCurve.length = topSolidCurve.GetLength();
      speckleCurve.domain = interval;
      //speckleCurve.bbox = BoxToSpeckle(spline.GeometricExtents, true);
      speckleCurve.units = u;

      SetInstanceParameters(speckleCurve, topSolidCurve);

      return speckleCurve;
    }

    public Objects.Geometry.Curve BSplineCurveToSpeckle(G.D2.Curves.BSplineCurve topSolidCurve, G.D3.Plane frame, string units = null)
    {
      Curve speckleCurve = new Curve();
      var u = units ?? ModelUnits;


      //Weights
      System.Collections.Generic.List<double> ptWeights = new System.Collections.Generic.List<double>();
      try
      {
        if (topSolidCurve.CWts.Count != 0)
        {
          foreach (double weight in topSolidCurve.CWts)
          {
            ptWeights.Add(weight);
          }
        }
      }
      catch { }

      try
      {

        double range = (topSolidCurve.Te - topSolidCurve.Ts);
        G.D2.PointList polyPoints = new G.D2.PointList();
        for (int i = 0; i < 100; i++)
        {
          polyPoints.Add(topSolidCurve.GetPoint((range / 100) * i));
        }
        Polyline displayValue = new Polyline();
        displayValue.value = D2PointsToFlatList(polyPoints, frame);
        displayValue.units = u;
        displayValue.closed = false;

        speckleCurve.displayValue = displayValue;
      }
      catch { }

      //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
      System.Collections.Generic.List<double> knots = new System.Collections.Generic.List<double>();

      for (int i = 0; i < (topSolidCurve.Bs.Count); i++)
      {
        knots.Add(topSolidCurve.Bs.ElementAt(i));

      }

      //Prevent errors when weight list is empty
      if (topSolidCurve.CWts.Count == 0)
      {
        ptWeights.Clear();
        for (int i = 0; i < topSolidCurve.CPts.Count; i++)
        {
          ptWeights.Add(1.0);
        }
      }

      Interval interval = new Interval(topSolidCurve.Ts, topSolidCurve.Te);

      //set speckle curve info
      speckleCurve.points = D2PointsToFlatArray(topSolidCurve.CPts).ToList();
      speckleCurve.knots = knots;
      speckleCurve.weights = ptWeights;
      speckleCurve.degree = topSolidCurve.Degree;
      speckleCurve.periodic = topSolidCurve.IsPeriodic;
      speckleCurve.rational = topSolidCurve.IsRational;
      speckleCurve.closed = topSolidCurve.IsClosed();
      speckleCurve.length = topSolidCurve.GetLength();
      speckleCurve.domain = interval;
      //speckleCurve.bbox = BoxToSpeckle(spline.GeometricExtents, true);
      speckleCurve.units = u;

      SetInstanceParameters(speckleCurve, topSolidCurve);

      return speckleCurve;
    }

    public Objects.Geometry.Curve D3BSplineCurveToSpeckle(G.D3.Curves.BSplineCurve topSolidCurve, string units = null)
    {
      Curve speckleCurve = new Curve();
      var u = units ?? ModelUnits;


      //Weights
      System.Collections.Generic.List<double> ptWeights = new System.Collections.Generic.List<double>();
      try
      {
        if (topSolidCurve.CWts.Count != 0)
        {
          foreach (double weight in topSolidCurve.CWts)
          {
            ptWeights.Add(weight);
          }
        }
      }
      catch { }

      try
      {
        double range = (topSolidCurve.Te - topSolidCurve.Ts);
        D3PointList polyPoints = new D3PointList();
        for (int i = 0; i < 100; i++)
        {
          polyPoints.Add(topSolidCurve.GetPoint((range / 100) * i));
        }
        Polyline displayValue = new Polyline();
        displayValue.value = D3PointsToFlatList(polyPoints);
        displayValue.units = u;
        displayValue.closed = false;

        speckleCurve.displayValue = displayValue;
      }
      catch { }

      //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
      System.Collections.Generic.List<double> knots = new System.Collections.Generic.List<double>();

      for (int i = 0; i < (topSolidCurve.Bs.Count); i++)
      {
        knots.Add(topSolidCurve.Bs.ElementAt(i));

      }

      //Prevent errors when weight list is empty
      if (topSolidCurve.CWts.Count == 0)
      {
        ptWeights.Clear();
        for (int i = 0; i < topSolidCurve.CPts.Count; i++)
        {
          ptWeights.Add(1.0);
        }
      }

      Interval interval = new Interval(topSolidCurve.Ts, topSolidCurve.Te);

      //set speckle curve info
      speckleCurve.points = D3PointsToFlatArray(topSolidCurve.CPts).ToList();
      speckleCurve.knots = knots;
      speckleCurve.weights = ptWeights;
      speckleCurve.degree = topSolidCurve.Degree;
      speckleCurve.periodic = topSolidCurve.IsPeriodic;
      speckleCurve.rational = topSolidCurve.IsRational;
      speckleCurve.closed = topSolidCurve.IsClosed();
      speckleCurve.length = topSolidCurve.GetLength();
      speckleCurve.domain = interval;
      //speckleCurve.bbox = BoxToSpeckle(spline.GeometricExtents, true);
      speckleCurve.units = u;

      SetInstanceParameters(speckleCurve, topSolidCurve);

      return speckleCurve;
    }
    public Objects.Geometry.Curve D3BSplineCurveToSpeckleWithTransformation(G.D3.Curves.BSplineCurve topSolidCurve, G.D3.Transform inTransform, string units = null)
    {
      Curve speckleCurveBefore = new Curve();
      var u = units ?? ModelUnits;


      //Weights
      System.Collections.Generic.List<double> ptWeights = new System.Collections.Generic.List<double>();
      try
      {
        if (topSolidCurve.CWts.Count != 0)
        {
          foreach (double weight in topSolidCurve.CWts)
          {
            ptWeights.Add(weight);
          }
        }
      }
      catch { }

      try
      {
        double range = (topSolidCurve.Te - topSolidCurve.Ts);
        D3PointList polyPoints = new D3PointList();
        for (int i = 0; i < 100; i++)
        {
          polyPoints.Add(topSolidCurve.GetPoint((range / 100) * i));
        }
        Polyline displayValue = new Polyline();
        displayValue.value = D3PointsToFlatList(polyPoints);
        displayValue.units = u;
        displayValue.closed = false;

        speckleCurveBefore.displayValue = displayValue;
      }
      catch { }

      //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
      System.Collections.Generic.List<double> knots = new System.Collections.Generic.List<double>();

      for (int i = 0; i < (topSolidCurve.Bs.Count); i++)
      {
        knots.Add(topSolidCurve.Bs.ElementAt(i));

      }

      //Prevent errors when weight list is empty
      if (topSolidCurve.CWts.Count == 0)
      {
        ptWeights.Clear();
        for (int i = 0; i < topSolidCurve.CPts.Count; i++)
        {
          ptWeights.Add(1.0);
        }
      }

      Interval interval = new Interval(topSolidCurve.Ts, topSolidCurve.Te);

      //set speckle curve info
      speckleCurveBefore.points = D3PointsToFlatArray(topSolidCurve.CPts).ToList();
      speckleCurveBefore.knots = knots;
      speckleCurveBefore.weights = ptWeights;
      speckleCurveBefore.degree = topSolidCurve.Degree;
      speckleCurveBefore.periodic = topSolidCurve.IsPeriodic;
      speckleCurveBefore.rational = topSolidCurve.IsRational;
      speckleCurveBefore.closed = topSolidCurve.IsClosed();
      speckleCurveBefore.length = topSolidCurve.GetLength();
      speckleCurveBefore.domain = interval;
      //speckleCurve.bbox = BoxToSpeckle(spline.GeometricExtents, true);
      speckleCurveBefore.units = u;

      Curve speckleCurve = new Curve();
      Other.Transform speckleTransform = new Other.Transform(
       new Vector(inTransform.R00, inTransform.R01, inTransform.R02, units),
       new Vector(inTransform.R10, inTransform.R11, inTransform.R12, units),
       new Vector(inTransform.R20, inTransform.R21, inTransform.R22, units), new Vector(inTransform.Tx, inTransform.Ty, inTransform.Tz, units));
      speckleCurveBefore.TransformTo(speckleTransform, out speckleCurve);

      SetInstanceParameters(speckleCurve, topSolidCurve);

      return speckleCurve;
    }

    public Objects.Geometry.Curve Curve2dToSpeckle(G.D2.Curves.BSplineCurve topSolidCurve, string units = null)
    {
      Curve speckleCurve = new Curve();
      var u = units ?? ModelUnits; //TODO investigate this


      System.Collections.Generic.List<G.D2.Point> tsPoints = topSolidCurve.CPts.ToList();

      //Weights
      System.Collections.Generic.List<double> ptWeights = new System.Collections.Generic.List<double>();
      try
      {
        if (topSolidCurve.CWts.Count != 0)
        {
          foreach (double weight in topSolidCurve.CWts)
          {
            ptWeights.Add(weight);
          }
        }
      }
      catch { }

      try
      {
        double range = (topSolidCurve.Te - topSolidCurve.Ts);
        G.D2.PointList polyPoints = new G.D2.PointList();
        for (int i = 0; i < 100; i++)
        {
          polyPoints.Add(topSolidCurve.GetPoint((range / 100) * i));
        }
        G.D2.Curves.PolylineCurve tspoly = new G.D2.Curves.PolylineCurve(false, polyPoints);
        Polyline displayValue = new Polyline();
        displayValue.value = Points2dToFlatList(polyPoints);
        displayValue.units = u;
        displayValue.closed = false;


        speckleCurve.displayValue = displayValue;
      }
      catch { }

      //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
      System.Collections.Generic.List<double> knots = new System.Collections.Generic.List<double>();

      for (int i = 0; i < (topSolidCurve.Bs.Count); i++)
      {
        knots.Add(topSolidCurve.Bs.ElementAt(i));
      }

      //Prevent errors when weight list is empty
      if (topSolidCurve.CWts.Count == 0)
      {
        ptWeights.Clear();
        for (int i = 0; i < topSolidCurve.CPts.Count; i++)
        {
          ptWeights.Add(1.0);
        }
      }

      Interval interval = new Interval(topSolidCurve.Ts, topSolidCurve.Te);

      //set speckle curve info
      speckleCurve.points = Points2dToFlatArray(topSolidCurve.CPts).ToList();
      speckleCurve.knots = knots;
      speckleCurve.weights = ptWeights;
      speckleCurve.degree = topSolidCurve.Degree;
      speckleCurve.periodic = topSolidCurve.IsPeriodic;
      speckleCurve.rational = topSolidCurve.IsRational;
      speckleCurve.closed = topSolidCurve.IsClosed();
      speckleCurve.length = topSolidCurve.GetLength();
      speckleCurve.domain = interval;
      //speckleCurve.bbox = BoxToSpeckle(spline.GeometricExtents, true);
      speckleCurve.units = u;

      SetInstanceParameters(speckleCurve, topSolidCurve);

      return speckleCurve;
    }
    public Objects.Geometry.Curve Curve2dToSpeckleWithTransformation(G.D2.Curves.BSplineCurve topSolidCurve, TK.G.D3.Transform inTransform, string units = null)
    {
      Curve speckleCurveBefore = new Curve();
      var u = units ?? ModelUnits; //TODO investigate this


      System.Collections.Generic.List<G.D2.Point> tsPoints = topSolidCurve.CPts.ToList();

      //Weights
      System.Collections.Generic.List<double> ptWeights = new System.Collections.Generic.List<double>();
      try
      {
        if (topSolidCurve.CWts.Count != 0)
        {
          foreach (double weight in topSolidCurve.CWts)
          {
            ptWeights.Add(weight);
          }
        }
      }
      catch { }

      try
      {
        double range = (topSolidCurve.Te - topSolidCurve.Ts);
        G.D2.PointList polyPoints = new G.D2.PointList();
        for (int i = 0; i < 100; i++)
        {
          polyPoints.Add(topSolidCurve.GetPoint((range / 100) * i));
        }
        G.D2.Curves.PolylineCurve tspoly = new G.D2.Curves.PolylineCurve(false, polyPoints);
        Polyline displayValue = new Polyline();
        displayValue.value = Points2dToFlatList(polyPoints);
        displayValue.units = u;
        displayValue.closed = false;


        speckleCurveBefore.displayValue = displayValue;
      }
      catch { }

      //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
      System.Collections.Generic.List<double> knots = new System.Collections.Generic.List<double>();

      for (int i = 0; i < (topSolidCurve.Bs.Count); i++)
      {
        knots.Add(topSolidCurve.Bs.ElementAt(i));
      }

      //Prevent errors when weight list is empty
      if (topSolidCurve.CWts.Count == 0)
      {
        ptWeights.Clear();
        for (int i = 0; i < topSolidCurve.CPts.Count; i++)
        {
          ptWeights.Add(1.0);
        }
      }

      Interval interval = new Interval(topSolidCurve.Ts, topSolidCurve.Te);

      //set speckle curve info
      speckleCurveBefore.points = Points2dToFlatArray(topSolidCurve.CPts).ToList();
      speckleCurveBefore.knots = knots;
      speckleCurveBefore.weights = ptWeights;
      speckleCurveBefore.degree = topSolidCurve.Degree;
      speckleCurveBefore.periodic = topSolidCurve.IsPeriodic;
      speckleCurveBefore.rational = topSolidCurve.IsRational;
      speckleCurveBefore.closed = topSolidCurve.IsClosed();
      speckleCurveBefore.length = topSolidCurve.GetLength();
      speckleCurveBefore.domain = interval;
      //speckleCurve.bbox = BoxToSpeckle(spline.GeometricExtents, true);
      speckleCurveBefore.units = u;

      Curve speckleCurve = new Curve();
      Other.Transform speckleTransform = new Other.Transform(
      new Vector(inTransform.R00, inTransform.R01, inTransform.R02, units),
      new Vector(inTransform.R10, inTransform.R11, inTransform.R12, units),
      new Vector(inTransform.R20, inTransform.R21, inTransform.R22, units), new Vector(inTransform.Tx, inTransform.Ty, inTransform.Tz, units));
      speckleCurveBefore.TransformTo(speckleTransform, out speckleCurve);

      SetInstanceParameters(speckleCurve, topSolidCurve);

      return speckleCurveBefore;
    }


    private G.D3.Curves.Curve CircleToNative(Circle circle)
    {

      G.D3.Curves.CircleCurve circleCurve = new G.D3.Curves.CircleCurve(PlaneToNative(circle.plane), ScaleToNative((double)circle.radius, circle.units));

      //modif
      try
      {
        G.D3.Curves.CircleMaker maker = new G.D3.Curves.CircleMaker(SX.Version.Current, tolerance, global::TopSolid.Kernel.G.Precision.AngularPrecision);
        maker.SetByCenterAndTwoPoints(
            PointToNative(circle.plane.origin),
            circleCurve.Ps,
           circleCurve.Pe,
            false,
            UnitVectorToNative(circle.plane.normal.Unit()),
            circleCurve);
      }
      catch (Exception e)
      { }


      return circleCurve;

    }
    public G.D3.Curves.Curve CurveToNative(ICurve curve, bool isReversed = false, string units = null)
    {
      switch (curve)
      {
        case Circle circle:
          return CircleToNative(circle);

        case Arc arc:
          return ArcToNative(arc);

        case Ellipse ellipse:
          return EllipseToNative(ellipse);

        //case Spiral spiral:
        //    return SpiralToNative(spiral);

        case Curve crv:
          return CurveToNative(crv);

        case Polyline polyline:
          return PolylineToNative(polyline);

        case Line line:
          return LineToNative(line, isReversed);

        //case Polycurve polycurve:
        //    return PolycurveToNative(polycurve);

        default:
          return null;
      }
    }

    public G.D3.Curves.EllipseCurve EllipseToNative(Ellipse ellipse)
    {
      return new G.D3.Curves.EllipseCurve(
          PlaneToNative(ellipse.plane),
          ScaleToNative((double)ellipse.firstRadius, ellipse.units),
          ScaleToNative((double)ellipse.secondRadius, ellipse.units));

    }


    public D3PolylineCurve PolylineToNative(Polyline polyline)
    {
      return new D3PolylineCurve(polyline.closed, ToNativePointList(polyline.points));

    }

    public G.D3.Curves.GeometricProfile PolycurveToNative(Polycurve polycurve)
    {
      G.D3.Curves.GeometricProfile profile = new G.D3.Curves.GeometricProfile();
      foreach (ICurve segment in polycurve.segments)
      {
        profile.Add(CurveToNative(segment));
      }
      return profile;
    }

    public D3BsplineCurve CurveToNative(Curve curve, string units = null)
    {
      //var u = units ?? ModelUnits;
      bool isRational = curve.rational;
      bool isPeriodic = curve.periodic;
      int degree = curve.degree;

      SX.Collections.DoubleList nativeKnot = ToNativeDoubleList(curve.knots);
      var ptsList = curve.GetPoints();
      G.D3.PointList nativePts = ToNativePointList(ptsList);
      SX.Collections.DoubleList nativeWeights = ToNativeDoubleList(curve.weights.ToList());
      BSpline bspline = new BSpline(isPeriodic, degree, nativeKnot);
      if (isRational)
      {
        //var w = c.Points.ConvertAll(x => x.Weight);
        G.D3.Curves.BSplineCurve bsplineCurve = new G.D3.Curves.BSplineCurve(bspline, nativePts, nativeWeights);
        bsplineCurve.SetRange((double)curve.domain.start, (double)curve.domain.end);
        return bsplineCurve;
      }
      else
      {
        G.D3.Curves.BSplineCurve bsplineCurve = new G.D3.Curves.BSplineCurve(bspline, nativePts);
        bsplineCurve.SetRange((double)curve.domain.start, (double)curve.domain.end);
        return bsplineCurve;
      }

    }
    #endregion


    // Box
    #region Box
    public Box BoxToSpeckle(D3Box topSolidBox, string units = null)
    {
      try
      {

        var u = units ?? ModelUnits;

        Box speckleBox = null;


        Frame tsFrame = topSolidBox.Frame;
        Plane spcklPlane = new Plane(new Point(tsFrame.Po.X, tsFrame.Po.Y, tsFrame.Po.Z, u), VectorToSpeckle(tsFrame.Vz, u), VectorToSpeckle(tsFrame.Vx, u), VectorToSpeckle(tsFrame.Vy, u), u);

        speckleBox = new Box(spcklPlane, new Interval(-topSolidBox.Hx, topSolidBox.Hx), new Interval(-topSolidBox.Hy, topSolidBox.Hy), new Interval(-topSolidBox.Hz, topSolidBox.Hz), u);
        //_box.area = (box.Hx * 2 * box.Hy * 2 * 2) + (box.Hx * 2 * box.Hz * 2 * 2) + (box.Hz * 2 * box.Hy * 2 * 2);
        speckleBox.volume = topSolidBox.Volume;
        speckleBox.units = u;

        SetInstanceParameters(speckleBox, topSolidBox);

        return speckleBox;

      }
      catch
      {
        return null;
      }
    }
    public D3Box BoxToNative(Box box)
    {
      // TODO: BOX To Topsolid
      return new D3Box();
    }
    #endregion

    // Surface
    #region Surface

    public Surface SurfaceToSpeckle(D3BSplineSurface topSolidSurface, string units = null)
    {
      var u = units ?? ModelUnits;
      var speckleSurface = new Geometry.Surface
      {
        degreeU = topSolidSurface.UDegree,
        degreeV = topSolidSurface.VDegree,
        rational = topSolidSurface.IsRational,
        closedU = topSolidSurface.IsUClosed,
        closedV = topSolidSurface.IsVClosed,
        domainU = new Interval(topSolidSurface.Us, topSolidSurface.Ue),
        domainV = new Interval(topSolidSurface.Vs, topSolidSurface.Ve),
        knotsU = GetCorrectKnots(topSolidSurface.UBs.ToList(), topSolidSurface.UCptsCount, topSolidSurface.UDegree),
        knotsV = GetCorrectKnots(topSolidSurface.VBs.ToList(), topSolidSurface.VCptsCount, topSolidSurface.VDegree)
      };

      speckleSurface.SetControlPoints(ControlPointsToSpeckle(topSolidSurface));

      speckleSurface.units = u;
      SetInstanceParameters(speckleSurface, topSolidSurface);

      return speckleSurface;
    }

    public D3BSplineSurface SurfaceToNative_AFS(Surface surface, int index = 0, string units = null)
    {

      bool isRational = surface.rational;
      bool isPeriodicU = surface.closedU;
      bool isPeriodicV = surface.closedV;
      var degreeU = surface.degreeU;
      var degreeV = surface.degreeV;
      DoubleList knotsU = ToDoubleList(surface.knotsU);
      DoubleList knotsV = ToDoubleList(surface.knotsV);
      System.Collections.Generic.List<System.Collections.Generic.List<ControlPoint>> surfPts = surface.GetControlPoints().Select(l => l.Select(p =>
       new ControlPoint(
         p.x,
         p.y,
         p.z,
         p.weight,
         p.units)).ToList()).ToList();
      var controlPts = surface.GetControlPoints();
      TSX.List<G.D3.PointList> meshPoints = new TSX.List<G.D3.PointList>();
      var controlPoints = ControlPointsToNative(surfPts, out meshPoints);


      int offKnot, nbCPu, nbCPv;
      DoubleList topKnots = new DoubleList();

      bool isPeriodic = isPeriodicU;
      if (isPeriodic) // According to v4_WishBone.3dm.
      {
        /*
        offKnot = degreeU - 1;       
        nbCPu = controlPts.Count - degreeU - 1;//afs
        //nbCPu = controlPts.Count - degreeU;
        */
        offKnot = 1;
        nbCPu = controlPts.Count - 2;
      }
      else
      {
        offKnot = 0;
        nbCPu = controlPts.Count;

        topKnots.Add(knotsU[0]);
      }


      for (int i = offKnot; i < surface.knotsU.Count - offKnot; i++)
        topKnots.Add(knotsU[i]);


      if (isPeriodic == false)
        topKnots.Add(knotsU.Last());


      BSpline bsplineU = new BSpline(isPeriodic, degreeU, topKnots);

      topKnots = new DoubleList();


      isPeriodic = isPeriodicV;
      if (isPeriodic)
      {
        offKnot = degreeV - 1;
        nbCPv = controlPts[0].Count - degreeV;
      }
      else
      {
        offKnot = 0;
        nbCPv = controlPts[0].Count;

        topKnots.Add(knotsV[0]);
      }

      for (int i = offKnot; i < surface.knotsV.Count - offKnot; i++)
        topKnots.Add(knotsV[i]);

      if (isPeriodic == false)
        topKnots.Add(knotsV.Last());

      BSpline bsplV = new BSpline(isPeriodic, degreeV, topKnots);

      DoubleList topWeights = new DoubleList();
      G.D3.PointList topPnts = new G.D3.PointList();


      for (int i = 0; i < nbCPu; i++)
      {
        for (int j = 0; j < nbCPv; j++)
        {
          D3Point pointToAdd = PointToNative(surfPts[i][j]);
          topPnts.Add(pointToAdd);
        }
      }


      for (int k = nbCPu * nbCPv; k < topPnts.Count; k++)
      {
        topPnts.RemoveAt(k);
      }

      if (surface.rational)
        return new BSplineSurface(bsplineU, bsplV, topPnts, ToDoubleList(surfPts.SelectMany(x => x).Select(x => x.weight)));
      else
        return new BSplineSurface(bsplineU, bsplV, topPnts);
    }

    public D3BSplineSurface SurfaceToNative(Surface surface, int index = 0, string units = null)
    {
      System.Collections.Generic.List<System.Collections.Generic.List<ControlPoint>> surfPts = surface.GetControlPoints().Select(l => l.Select(p =>
       new ControlPoint(
         p.x,
         p.y,
         p.z,
         p.weight,
         p.units)).ToList()).ToList();


      var uKnots = SurfaceKnotsToNative(surface.knotsU);
      var vKnots = SurfaceKnotsToNative(surface.knotsV);
      TSX.List<G.D3.PointList> meshPoints = new TSX.List<G.D3.PointList>();
      var ctPts = ControlPointsToNative(surfPts, out meshPoints);

      BSpline vBspline = new BSpline(surface.closedV, surface.degreeV, ToDoubleList(vKnots));

      BSpline uBspline = new BSpline(surface.closedU, surface.degreeU, ToDoubleList(uKnots));

      // TODO : Rational option
      if (surface.rational)
      {
        D3BSplineSurface bs = new D3BSplineSurface(uBspline, vBspline, ctPts, ToDoubleList(surfPts.SelectMany(x => x).Select(x => x.weight)));
        return bs;
      }
      else
      {

        D3BSplineSurface bs = new D3BSplineSurface(uBspline, vBspline, ctPts);
        var gtype = bs.GeometryType;

        return bs;
      }
    }



    private G.D3.PointList ControlPointsToNative(System.Collections.Generic.List<System.Collections.Generic.List<ControlPoint>> controlPoints, out TSX.List<G.D3.PointList> pts)
    {
      var uCount = controlPoints.Count;
      var vCount = controlPoints[0].Count;
      var count = uCount * vCount;

      var points = new G.D3.PointList(count);
      int p = 0;

      pts = new TSX.List<G.D3.PointList>();
      foreach (var row in controlPoints)
      {
        G.D3.PointList ptListToAdd = new G.D3.PointList();
        foreach (var pt in row)
        {
          var point = new Point(pt.x, pt.y, pt.z, pt.units);
          points.Add(PointToNative(point));
          ptListToAdd.Add(PointToNative(point));
        }
        pts.Add(ptListToAdd);
      }


      return points;
    }

    public double[] SurfaceKnotsToNative(System.Collections.Generic.List<double> list)
    {
      var count = list.Count;
      var knots = new double[count + 2];

      int j = 0, k = 0;
      while (j < count)
        knots[++k] = list[j++];

      knots[0] = knots[1];
      knots[count + 1] = knots[count];

      return knots;
    }


    #endregion

    //Breps & Shapes
    #region Brep

    private Brep BrepToSpeckle(Shape shape, string units = null)
    {
      Shape _shape = shape;
      Brep spcklBrep = new Brep();
      Alias alias = new Alias();
      alias.Faces = new System.Collections.Generic.List<GeometryAlias>();
      alias.Edges = new System.Collections.Generic.List<GeometryAlias>();
      alias.Vertices = new System.Collections.Generic.List<GeometryAliasLinked>();

      //Variables and global counters (not to be reinitialized for each face)
      //double tol = global::TopSolid.Kernel.G.Precision.LinearPrecision;
      double tol = global::TopSolid.Kernel.G.Precision.ModelingLinearTolerance;
      var u = units ?? ModelUnits;
      int faceindex = 0;
      int loopIndex = 0;
      spcklBrep.units = u;
      int startVertInd = 0;
      int endVertInd = 0;
      int facecount = _shape.FaceCount;

      //Lists to get Curves and Edges for each face
      System.Collections.Generic.List<TSX.List<G.D2.Curves.IGeometricProfile>> global2dList = new System.Collections.Generic.List<TSX.List<G.D2.Curves.IGeometricProfile>>(facecount);
      System.Collections.Generic.List<TSX.List<G.D3.Curves.IGeometricProfile>> global3dList = new System.Collections.Generic.List<TSX.List<G.D3.Curves.IGeometricProfile>>(facecount);
      System.Collections.Generic.List<TSX.List<EdgeList>> globalEdgeList = new System.Collections.Generic.List<TSX.List<EdgeList>>(facecount);
      System.Collections.Generic.List<SX.Collections.BoolList> globalBoolList = new System.Collections.Generic.List<SX.Collections.BoolList>(facecount);

      Dictionary<int, bool> periodicityDictionary = new Dictionary<int, bool>();
      //uv curves, 3d curves and surfaces, per face
      foreach (G.D3.Shapes.Face face in _shape.Faces)
      {
        SurfaceGeometryType typeOfFace = face.GeometryType;


        global2dList.Add(new TSX.List<G.D2.Curves.IGeometricProfile>());
        global3dList.Add(new TSX.List<G.D3.Curves.IGeometricProfile>());
        globalEdgeList.Add(new TSX.List<EdgeList>());
        globalBoolList.Add(new SX.Collections.BoolList());

        var loop2d = global2dList[faceindex];
        var loop3d = global3dList[faceindex];
        var tsEgdes = globalEdgeList[faceindex];
        var boolList = globalBoolList[faceindex];

        alias.Faces.Add(new GeometryAlias
        {
          Index = faceindex,
          Moniker = face.Moniker.ToString()
        });
        if (typeOfFace != SurfaceGeometryType.Sphere)
        {
          //GetTopological info of face
          OrientedSurface surf = face.GetOrientedBsplineTrimmedGeometry(tol, true, true, false, FaceTrimmingLoopsConfine.No, boolList, loop2d, loop3d, tsEgdes/*,false*/);

          bool periodicity = surf.Surface.IsUPeriodic || surf.Surface.IsVPeriodic;
          periodicityDictionary.Add(faceindex, periodicity);

          //Surface
          spcklBrep.Surfaces.Add(SurfaceToSpeckle(surf.Surface as BSplineSurface, u));
        }
        else
        {
          var surfs = face.GetBsplineGeometry(tol, true, true, false);
          var bSplineSurf = surfs.GetBsplineGeometry(tol, true, true, false);
          EdgeList edList = new EdgeList();
          face.GetEdges(edList);
          tsEgdes.Add(edList);
          LoopList loops = new LoopList();
          face.GetLoops(loops);
          int indLoop = 0;
          foreach (var loop in loops)
          {
            TX.Items.ItemMonikerKey key = new TX.Items.ItemMonikerKey(TX.Items.ItemOperationKey.BasicKey);
            var crv3dCurve = loop.MakeGeometricProfile(new ItemMoniker(false, (byte)ItemType.ShapeFace, key, new int[] { faceindex, indLoop }));
            loop3d.Add(crv3dCurve);
            var crv2dCurve = crv3dCurve.MakeD2GeometricProfile(new TsPlane(Frame.OXYZ));
            loop2d.Add(crv2dCurve);
            indLoop++;
          }
          spcklBrep.Surfaces.Add(SurfaceToSpeckle(bSplineSurf, u));
        }

        faceindex++;
      }

      //Flatten lists
      var crv2d = global2dList.SelectMany(x => x.SelectMany(y => y.Segments));
      var crv3d = global3dList.SelectMany(x => x.SelectMany(y => y.Segments));
      var edges = globalEdgeList.SelectMany(x => x.SelectMany(y => y));
      var tupList = new System.Collections.Generic.List<(Edge Edge, G.D3.Curves.IGeometricSegment Crv3d, G.D2.Curves.IGeometricSegment Crv2d)>();
      var edC = edges.Count();
      var crv3dC = crv3d.Count();
      var crv2dC = crv2d.Count();
      //Vertices
      System.Collections.Generic.List<G.D3.Shapes.Vertex> tsVerticesList = _shape.Vertices.ToList();

      spcklBrep.Vertices = tsVerticesList
        .Select(vertex => PointToSpeckle(vertex.GetGeometry(), u)).ToList();

      int counter = 0;

      //Create a list of tuple linking Edges, crv3d and crv2d ===> some edges are thus repeated
      foreach (var edge in edges)
      {
        var curve3D = crv3d.ElementAt(counter);
        var myTup = (Edge: edge, Crv3d: crv3d.ElementAt(counter), Crv2d: crv2d.ElementAt(counter));
        tupList.Add(myTup);
        counter++;
      }

      //Add faceindex to tuples
      EdgeList listDistinct = new EdgeList();
      int EdgeCounter = 0;
      int EdgeIndex = 0;
      counter = 0;
      int i = 0; // global Loop index
      int K = 0;
      var tupwithfaces = new System.Collections.Generic.List<(Edge Edge, G.D3.Curves.IGeometricSegment Crv3d, G.D2.Curves.IGeometricSegment Crv2d, int Findex, int Counter, int LoopIndex, int EdgeIndex)>();


      FaceList facesList = new FaceList();
      shape.GetFaces(facesList);

      foreach (var lst in global2dList/*global3dList*/) //loop through faces
      {
        bool isPeriodicSurface = false;
        periodicityDictionary.TryGetValue(K, out isPeriodicSurface);

        var loopCount = facesList[K].LoopCount;
        LoopList loops = new LoopList();
        facesList[K].GetLoops(loops);

        foreach (var profile in lst) //loop through loops
        {
          foreach (var seg in profile.Segments) //loop through crvs
          {
            if (counter < tupList.Count) //Added to prevent an error when number of edges != number of curves
            {

              var edge = tupList.ElementAt(counter).Edge;

              if (!listDistinct.Contains(edge))
              {
                listDistinct.Add(edge);
                EdgeIndex = EdgeCounter++;
              }
              else
              {
                EdgeIndex = listDistinct.IndexOf(edge);
              }

              //ici l'index du loop envoyé n'est pas bon
              //certaines faces ne sont pas décrites à cause de la limite COUNTER
              var mytup = (Edge: edge, crv3d: tupList.ElementAt(counter).Crv3d, crv2d: tupList.ElementAt(counter).Crv2d, faceindex: K, Counter: counter, LoopIndex: i, EdgeIndex: EdgeIndex);
              //tupwithfaces.Add(new Tuple<Edge, IGeometricSegment, G.D2.Curves.IGeometricSegment, int>(tup.ElementAt(counter).Item1, tup.ElementAt(counter).Item2, tup.ElementAt(counter).Item3, K));
              tupwithfaces.Add(mytup);

              counter++;
            }
          }
          i++;//loop index
        }

        K++;
      }

      //Create a list of Tuple which associates each edge to a 3d curve and a list of 2D trims
      var tupforTrims = new System.Collections.Generic.List<(Edge Edge, G.D3.Curves.IGeometricSegment Crv3d, System.Collections.Generic.List<G.D2.Curves.IGeometricSegment> TrimCrvs, System.Collections.Generic.List<int> Crv2dindices)>();
      foreach (var ed in _shape.Edges.OrderBy(x => edges.ToList().IndexOf(x)))
      {
        var localTups = tupList.Where(x => x.Edge == ed); //Get all the tuple with this same edge
        var crv2dIndices = new System.Collections.Generic.List<int>(localTups.Count());
        foreach (var tup in localTups) //get the indices of the 2d crvs
        {
          crv2dIndices.Add(tupList.IndexOf(tup));
        }

        G.D3.Curves.IGeometricSegment curve3D = tupList.Where(x => x.Edge == ed).First().Crv3d;

        var mytup = (Edge: ed, Crv3d: curve3D, TrimCrvs: tupList.Where(x => x.Edge == ed).Select(x => x.Crv2d).ToList(), Crv2dindices: crv2dIndices);
        tupforTrims.Add(mytup);
      }

      //Loop list needed for face definition and later for loop def
      var tsLoopList = _shape.Loops.ToList();

      int faceind = 0;
      int outerindex = 0;

      //Add Faces with correct loops
      foreach (G.D3.Shapes.Face face in _shape.Faces)
      {
        var typeOfFace = face.GeometryType;
        System.Collections.Generic.List<int> faceLoopIndices = new System.Collections.Generic.List<int>(face.LoopCount);
        var list = face.Loops;
        foreach (var loop in list)
        {
          LoopType lp = loop.Type;
          var ind = tsLoopList.IndexOf(loop);
          faceLoopIndices.Add(ind);
          bool isPeriodicU, isPeriodicV = false;
          face.IsPeriodic(out isPeriodicU, out isPeriodicV);
          if (loop.IsOuter || (lp == LoopType.Winding && (isPeriodicU || isPeriodicV)))
            outerindex = ind;
        }
        var brepFace = new BrepFace(spcklBrep, faceind, faceLoopIndices, outerindex, face.IsReversed());
        brepFace["faceMoniker"] = face.Moniker.ToString();
        brepFace["faceId"] = face.Id.ToString();
        spcklBrep.Faces.Add(brepFace);
        faceind++;
      }

      //Add 3d Curves non repeated
      G.D3.Curves.BSplineCurve bsCrv3d;
      foreach (var t in tupforTrims)
      {
        bsCrv3d = t.Crv3d.GetOrientedCurve().Curve.GetBSplineCurve(false, false);

        Plane planeToKeep = new Plane();
        if (bsCrv3d.IsCircular(out G.D3.Curves.CircleCurve circleCurve))
        {
          if (circleCurve.IsClosed())
          {
            spcklBrep.Curve3D.Add(CircleToSpeckle(circleCurve));
          }
          else
          {
            D3Point Ps, Pm, Pe, Center;
            Ps = new D3Point(circleCurve.Ps.X, circleCurve.Ps.Y, 0);
            Pm = new D3Point(circleCurve.Pm.X, circleCurve.Pm.Y, 0);
            Pe = new D3Point(circleCurve.Pe.X, circleCurve.Pe.Y, 0);
            Center = new D3Point(circleCurve.Center.X, circleCurve.Center.Y, 0);

            D3Vector vectorS = new D3Vector(Ps, Center);
            D3Vector vectorE = new D3Vector(Pe, Center);

            Geometry.Arc arc = new Geometry.Arc();
            arc.startPoint = PointToSpeckle(circleCurve.Ps);
            arc.midPoint = PointToSpeckle(circleCurve.Pm);
            arc.endPoint = PointToSpeckle(circleCurve.Pe);
            arc.plane = PlaneToSpeckle(circleCurve.Plane);
            arc.radius = circleCurve.Radius;
            arc.length = circleCurve.GetLength();
            arc.domain = new Interval(0, 1);

            spcklBrep.Curve3D.Add(arc);
          }
        }
        else
        {
          spcklBrep.Curve3D.Add(D3BSplineCurveToSpeckle(bsCrv3d, u));
        }
      }

      //Add 2D curves
      G.D2.Curves.BSplineCurve bsCrv2d;
      foreach (var t in tupList)
      {
        bsCrv2d = t.Crv2d.GetOrientedCurve().Curve.GetBSplineCurve(false, false);
        //spcklBrep.Curve2D.Add(BSplineCurveToSpeckle(bsCrv2d));
      }

      // Add Tags.vertices
      var iV = 0;
      foreach (var vertex in _shape.Vertices)
      {
        alias.Vertices.Add(GetHashVertex(vertex, iV));
        iV++;
      }


      //Add Edges with correct trims
      counter = 0;
      foreach (var tuple in tupforTrims)
      {
        var localEdge = tuple.Edge;
        EdgeType typeOfEdge = tuple.Edge.Type;

        startVertInd = tsVerticesList.IndexOf(localEdge.StartVertex);
        endVertInd = tsVerticesList.IndexOf(localEdge.EndVertex);

        var brepEdge = new BrepEdge();
        brepEdge.Brep = spcklBrep;
        brepEdge.Curve3dIndex = counter;
        brepEdge.TrimIndices = tuple.Crv2dindices.ToArray();

        brepEdge.StartIndex = startVertInd;
        brepEdge.EndIndex = endVertInd;

        brepEdge.ProxyCurveIsReversed = tuple.Edge.IsReversed();
        brepEdge.Domain = new Interval(0, 1);

        alias.Edges.Add(new GeometryAlias
        {
          Index = listDistinct.IndexOf(localEdge),
          Moniker = localEdge.Moniker.ToString()
        });


        // MOVE UPPER

        //// Update Edge in all vertices
        //foreach (var item in localEdge.Vertices) 

        //{
        //  var findex = _shape.Vertices.ToList().FindIndex(x => x.Moniker == item.Moniker);

        //  // TODO : Check if no surface and edges => can't force moniker
        //  string eHach = string.Join("-", item.Edges.ToList().Select(f => f.Moniker).OrderBy(s => s));
        //  string vHash = GetHash(alias.Vertices[findex].Hash + "+" + eHach);

        //  alias.Vertices[findex].Hash = (vHash);
        //}

        //brepEdge.Domain = new Interval(localEdge.GetRange().Min, localEdge.GetRange().Max);//This caused problems because the bspline is always [0,1]
        spcklBrep.Edges.Add(brepEdge);
        counter++;
      }

      //Loops + Trims
      var tsFaceList = _shape.Faces.ToList();
      faceind = 0;
      counter = 0;

      loopIndex = 0;
      int trimcounter = 0;

      foreach (var l in tsLoopList)
      {
        System.Collections.Generic.List<int> triminds = new System.Collections.Generic.List<int>();
        var localFace = l.GetFace();
        faceind = tsFaceList.IndexOf(localFace);

        BrepLoopType type;
        if (l.IsInner)
          type = BrepLoopType.Inner;
        else if (l.IsOuter)
          type = BrepLoopType.Outer;
        else
        { type = BrepLoopType.Unknown; }

        var loop = new BrepLoop();
        loop.Brep = spcklBrep;
        loop.FaceIndex = tsFaceList.IndexOf(l.GetFace());
        loop.Type = type;


        //Better ordered than getting the Edges via Loop.Edges
        foreach (var tup in tupwithfaces)
        {
          if (tup.LoopIndex != loopIndex)
            continue;
          else
          {
            var trim = new BrepTrim();
            trim.Brep = spcklBrep;
            trim.EdgeIndex = tup.EdgeIndex;
            trim.FaceIndex = tup.Findex;
            trim.LoopIndex = loopIndex;
            trim.CurveIndex = tup.Counter;
            trim.IsoStatus = 0;
            if (tup.Edge.Type == EdgeType.Boundary)
              trim.TrimType = BrepTrimType.Boundary;
            else
              trim.TrimType = BrepTrimType.Unknown;
            var c2d = tup.Crv2d;
            trim.IsReversed = c2d.IsReversed;
            trim.Domain = new Interval(c2d.Range.Min, c2d.Range.Max);
            spcklBrep.Trims.Add(trim);

            triminds.Add(trimcounter++);
          }
        }

        loop.TrimIndices = triminds;
        spcklBrep.Loops.Add(loop);
        loopIndex++;
        counter++;
      }


      //necessary in order to have Trims counted per Loop 
      loopIndex = 0;
      foreach (var l in spcklBrep.Loops)
      {
        foreach (var t in l.Trims)
        {
          t.LoopIndex = loopIndex;
        }
        loopIndex++;
      }

      spcklBrep.bbox = BoxToSpeckle(shape.FindBox(), u);
      //Find display values in geometries
      System.Collections.Generic.List<Mesh> displayValue = new System.Collections.Generic.List<Mesh>();
      displayValue.Add(ShapeDisplayToMesh(shape, u));
      spcklBrep.displayValue = displayValue;
      SetInstanceParameters(spcklBrep, shape, alias);


      return spcklBrep;
    }


    OperationList operationsList = new OperationList();
    private Shape BrepToNative(Brep brep, string units = null)
    {

      var u = units ?? ModelUnits;
      ModelingDocument doc = Doc;

      Alias alias = GetAlias(brep);

      if (alias != null)
      {
        Console.WriteLine(alias.ToString());
      }

      // Brep rs = null;
      double tol = 0;
      tol = (global::TopSolid.Kernel.G.Precision.ModelingLinearTolerance);
      ShapeList shapeList = BrepToShapeList(brep, alias, tol);


      if (shapeList != null && shapeList.Count > 1)
      {
        SheetsSewer sheetsSewer = new SheetsSewer(SX.Version.Current, shapeList.First());
        //sheetsSewer.GapWidth = G.Precision.ModelingLinearTolerance;
        sheetsSewer.GapWidth = tol/*0.1*/;//afs modif
        sheetsSewer.NbIterations = 5;
        sheetsSewer.CreateNewBodies = true;
        sheetsSewer.ResetEdgesPrecision = true;
        sheetsSewer.Merges = true;//afs comment
        Shape currentShape;
        currentShape = shapeList[0];

        for (int i = 1; i < shapeList.Count; i++)
        {
          currentShape = shapeList[i];

          foreach (var face in currentShape.Faces)
          {
            Console.WriteLine(face.Edges.Select(e => e.Moniker).ToString());
          }
          sheetsSewer.AddTool(currentShape, i);
        }


        try
        {
          sheetsSewer.Sew(ItemOperationKey.BasicKey);
          sheetsSewer.ResetEdgesPrecision = true;
          EdgeList edgeErrors = sheetsSewer.ErrorEdges;
        }
        catch (Exception ex)
        {
          ex.ToString();
        }

        //var iF = 0;
        //foreach (var face in sheetsSewer.Shape.Faces)
        //{
        //  Console.WriteLine(face.Moniker.ToString() + face.Edges.Select(e => e.Moniker.ToString()));

        //  iF++;
        //}

        // TODO : Define all Moniker (saved in Speckle)
        // Edge Moniker   : E1(s1(2))
        //var iE = 0;
        //foreach (var edge in sheetsSewer.Shape.Edges)
        //{
        //  //edge.SetMoniker(new ItemMoniker(new SX.CString(alias.Edges[iE].Moniker)));

        //  iE++;
        //}

        //var ttt=   sheetsSewer.Shape.Edges.Select(e => e.Moniker.ToString()).ToList(); 
        // Vertex Moniker : V1(1)
        foreach (var vertex in sheetsSewer.Shape.Vertices)
        {

          string vHash = GetHashVertex(vertex, -1).Hash;

          string newMoniker = null;
          //AFS commented
          //foreach (var va in alias.Vertices)
          //{
          //  if (va.Hash == vHash)
          //  {
          //    newMoniker = va.Moniker;
          //  }
          //}

          //if (newMoniker != null) vertex.SetMoniker(new ItemMoniker(new SX.CString(newMoniker)));
        }

        // TODO : Controle is modified compared hash of brep

        sheetsSewer.Shape.AddRollbackMark(true, false, false, true, out _);

        return sheetsSewer.Shape;

      }

      return shapeList[0];


    }

    public ShapeList BrepToShapeList(Brep brep, Alias alias, double tol = global::TopSolid.Kernel.G.Precision.ModelingLinearTolerance, string units = null)
    {
      //var u = units ?? ModelUnits;
      double tol_TS = tol;
      Shape shape = null;
      ShapeList ioShapes = new ShapeList(brep.Faces.Count);
      int faceind = 0;
      foreach (BrepFace bface in brep.Faces)
      {
        shape = null;

        shape = MakeSheetFrom3d(brep, bface, tol_TS, faceind++, alias);


        if (shape == null || shape.IsEmpty)
        {
        }
        else
          ioShapes.Add(shape);
      }


      return ioShapes;
    }
    private Shape MakeSheetFrom3d(Brep inBRep, BrepFace inFace, double inLinearPrecision, int faceindex, Alias alias, string units = null)
    {
      Shape shape = new Shape(null);

      TrimmedSheetMaker sheetMaker = new TrimmedSheetMaker(SX.Version.Current);
      sheetMaker.LinearTolerance = inLinearPrecision;
      sheetMaker.UsesBRepMethod = false;

      // TODO : Remplacer Moniker de Speckle : 
      // 1) String to Moniker
      // 2) Set
      TX.Items.ItemMonikerKey key = new TX.Items.ItemMonikerKey(TX.Items.ItemOperationKey.BasicKey);

      // Get surface and set to maker.

      Surface surface = inBRep.Surfaces[inFace.SurfaceIndex];


      // Closed BSpline surfaces must not be periodic for parasolid with 3d curves (according to wishbone.3dm and dinnermug.3dm).
      // If new problems come, see about the periodicity of the curves.

      //TODO check if planar to simplify            
      BSplineSurface bsSurface = SurfaceToNative_AFS(surface);

      bool isSurfPeriodic = false;
      if (bsSurface != null && (bsSurface.IsUPeriodic || bsSurface.IsVPeriodic))
      {
        isSurfPeriodic = true;
        bsSurface = (BSplineSurface)bsSurface.Clone();

        if (bsSurface.IsUPeriodic)
        {
          bsSurface.MakeUNonPeriodic();
        }

        if (bsSurface.IsVPeriodic)
        {
          bsSurface.MakeVNonPeriodic();
        }
      }


      // Recupérer la valeur de sheetMaker (list, etc)

      sheetMaker.Surface = new OrientedSurface(bsSurface, /*inFace.OrientationReversed*/false);//afs modif
      sheetMaker.SurfaceMoniker = new ItemMoniker(false, (byte)ItemType.ShapeFace, key, faceindex/*1*/);


      #region AFS modified
      // Get spatial curves and set to maker.
      TK.SX.Collections.Generic.List<G.D3.Curves.CurveList> loops3d = new TK.SX.Collections.Generic.List<G.D3.Curves.CurveList>();
      TSX.List<ItemMonikerList> listItemMok = new TSX.List<ItemMonikerList>();
      TSX.List<ItemMonikerList> listOfmonikersForVertices = new TSX.List<ItemMonikerList>();

      int loopIndex = 0;
      int indexMoniker = 0;
      int indexVertices = 0;

      System.Collections.Generic.List<ICurve> curvesInFace = inFace.Brep.Curve3D;


      foreach (BrepLoop loop in inFace.Loops)
      {
        loops3d.Add(new G.D3.Curves.CurveList());
        listItemMok.Add(new ItemMonikerList());

        ItemMonikerList monikersForCurves = new ItemMonikerList();
        ItemMonikerList verticesMonikers = new ItemMonikerList();

        G.D3.Curves.CurveList curvesToAdd = new G.D3.Curves.CurveList();
        ItemMonikerList monikersCurves = new ItemMonikerList();
        int indexInLoop = 0;
        foreach (var trim in loop.Trims)
        {
          //if (loops3d.Count < loopIndex - 1 || listItemMok.Count < loopIndex - 1) break;

          if (trim.Edge != null) //trim.Edge can be null for singular Trims
          {
            if (trim.Edge.Curve is not Circle)
            {
              G.D3.Curves.Curve curveToAdd = CurveToNative(trim.Edge.Curve);
              curvesToAdd.Add(curveToAdd);
            }
            else
            {
              G.D3.Curves.Curve curveToAdd = CircleToNative(trim.Edge.Curve as Circle);
              curvesToAdd.Add(curveToAdd);
            }
            monikersCurves.Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, new int[] { faceindex, indexMoniker }));
            indexMoniker++;

            // Make vertices monikers.
            D3Point verticeStart;
            if (trim.Edge.Curve is not Circle)
            {
              verticeStart = PointToNative(trim.Edge.StartVertex);
              verticesMonikers.Add(new ItemMoniker(false, (byte)ItemType.SketchVertex, key, new int[] { faceindex, indexMoniker, indexVertices }));
            }

            indexVertices++;
            indexInLoop++;

          }
        }

        if (curvesToAdd.Count > 0)
        {
          G.D3.Curves.CurveList curvesToAdd_Ordered = new G.D3.Curves.CurveList();
          curvesToAdd.MakeOrdered(inLinearPrecision, curvesToAdd_Ordered);

          loops3d[loopIndex].Add(curvesToAdd_Ordered);
          listItemMok[loopIndex].Add(monikersCurves);

          loopIndex++;

          if (verticesMonikers.Count > 0)
            listOfmonikersForVertices.Add(verticesMonikers);
        }


      }
      #endregion



      if (loops3d != null && loops3d.Count != 0)
      {
        {

          sheetMaker.SetCurves(loops3d, null/*listItemMok*/);
          if (isSurfPeriodic)
          {
            var simplifiedSurface = bsSurface.Simplify(SX.Version.Current, inLinearPrecision);
            if (simplifiedSurface != null)//same nb of points
            {
              sheetMaker.Surface = new OrientedSurface(simplifiedSurface, false);//afs modif
            }

          }


          bool valid = sheetMaker.IsValid;
          try
          {
            shape = sheetMaker.Make(null, TK.TX.Items.ItemOperationKey.BasicKey);

            shape.SetDefaultMonikers(new ItemMonikerKey(TK.TX.Items.ItemOperationKey.BasicKey));
            //shape.CreateDebugEntity(SX.Drawing.Color.White, null, null);

          }
          catch (Exception e)
          {

            foreach (var curveList in loops3d)
            {
              foreach (var curve in curveList)
              {
                curve.CreateDebugEntity(SX.Drawing.Color.Red, null, null);
              }
            }
            Console.WriteLine(e);
          }
        }
      }

      return shape;
    }


    //Preview Mesh for the Web (or else replacement in case conversion fails)
    public Mesh ShapeDisplayToMesh(Shape shape, string units = null)
    {
      var u = units ?? ModelUnits;

      var verts = new System.Collections.Generic.List<double>();
      System.Collections.Generic.List<int> vertIndices = new System.Collections.Generic.List<int>();
      int ind = 0;
      var faces = new System.Collections.Generic.List<int>();
      foreach (var f in shape.Display.Items.FaceItems)
      {

        var mainface = f as FaceItem;
        foreach (var face in mainface.Facets)
        {

          verts.Add(face.P0.X);
          verts.Add(face.P0.Y);
          verts.Add(face.P0.Z);
          vertIndices.Add(ind++);

          verts.Add(face.P1.X);
          verts.Add(face.P1.Y);
          verts.Add(face.P1.Z);
          vertIndices.Add(ind++);

          verts.Add(face.P2.X);
          verts.Add(face.P2.Y);
          verts.Add(face.P2.Z);
          vertIndices.Add(ind++);


          faces.Add(0);
          faces.AddRange(new int[] { ind - 3, ind - 2, ind - 1 });
        }


      }


      Mesh speckleMesh = new Mesh();
      speckleMesh.faces = faces;
      speckleMesh.vertices = verts;
      speckleMesh.units = u;
      //speckleMesh["renderMaterial"] = RenderMaterialToSpeckle(shape.Owner as Element);

      return speckleMesh;
    }
    #endregion


    //PolyHedron
    public Mesh PolyhedronToSpeckle(Polyhedron polyhedron, string units = null)
    {
      var u = units ?? ModelUnits;

      var verts = new System.Collections.Generic.List<double>();
      System.Collections.Generic.List<int> vertIndices = new System.Collections.Generic.List<int>();
      int ind = 0;
      var faces = new System.Collections.Generic.List<int>();


      foreach (var f in polyhedron.Display.Items.FaceItems)
      {

        var mainface = f as FaceItem;
        foreach (var face in mainface.Facets)
        {


          verts.Add(face.P0.X);
          verts.Add(face.P0.Y);
          verts.Add(face.P0.Z);
          vertIndices.Add(ind++);

          verts.Add(face.P1.X);
          verts.Add(face.P1.Y);
          verts.Add(face.P1.Z);
          vertIndices.Add(ind++);

          verts.Add(face.P2.X);
          verts.Add(face.P2.Y);
          verts.Add(face.P2.Z);
          vertIndices.Add(ind++);


          faces.Add(0);
          faces.AddRange(new int[] { ind - 3, ind - 2, ind - 1 });
        }


      }

      Mesh speckleMesh = new Mesh();
      speckleMesh.faces = faces;
      speckleMesh.vertices = verts;
      speckleMesh.units = u;
      SetInstanceParameters(speckleMesh, polyhedron);

      return speckleMesh;

    }

    public Point VertexToSpeckle(G.D2.Sketches.Vertex vertex)
    {
      Point specklepoint = PointToSpeckle(vertex.Geometry);
      specklepoint["vertexName"] = vertex.VertexName;
      specklepoint["vertexColor"] = ((System.Drawing.Color)vertex.NameColor).ToArgb();
      specklepoint["vertexFont"] = vertex.NameFontName;
      specklepoint["namePosVector"] = new Vector(vertex.NamePosVector.X, vertex.NamePosVector.Y);


      SX.Drawing.Color defaultColor = SX.Drawing.Color.Empty;
      var sketchEntity = (TK.DB.D3.Sketches.Planar.PlanarSketchEntity)(vertex.Sketch.Owner);
      if (sketchEntity != null && sketchEntity.HasStyle)
      {
        var styleForSketc = sketchEntity.Style;
      }
      if (sketchEntity != null)
      {
        defaultColor = sketchEntity.Color;
      }
      DisplayStyle displayStyle = new DisplayStyle();
      SX.Drawing.Color colorToUse = (vertex.Color.IsEmpty ? defaultColor : vertex.Color);
      displayStyle.color = ((System.Drawing.Color)colorToUse).ToArgb();
      displayStyle.lineweight = 0;
      displayStyle.linetype = "Continuous";
      specklepoint["displayStyle"] = displayStyle;

      return specklepoint;
    }

    public Point VertexToSpeckle(G.D3.Sketches.Vertex vertex)
    {
      Point specklepoint = PointToSpeckle(vertex.Geometry);
      specklepoint["vertexName"] = vertex.VertexName;
      specklepoint["vertexColor"] = ((System.Drawing.Color)vertex.NameColor).ToArgb();
      specklepoint["vertexFont"] = vertex.NameFontName;
      specklepoint["namePosVector"] = new Vector(vertex.NamePosVector.X, vertex.NamePosVector.Y);


      SX.Drawing.Color defaultColor = SX.Drawing.Color.Empty;
      if (vertex.Sketch.Owner is TK.DB.D3.Sketches.Planar.PlanarSketchEntity sketchEntity)
      {
        //var sketchEntity = (TK.DB.D3.Sketches.Planar.PlanarSketchEntity)(vertex.Sketch.Owner);
        if (sketchEntity != null && sketchEntity.HasStyle)
        {
          var styleForSketc = sketchEntity.Style;
        }
        if (sketchEntity != null)
        {
          defaultColor = sketchEntity.Color;
        }
      }
      else if (vertex.Sketch.Owner is TK.DB.D3.Sketches.PositionedSketchEntity sketchEntityAgain)
      {
        //var sketchEntity = (TK.DB.D3.Sketches.Planar.PlanarSketchEntity)(vertex.Sketch.Owner);
        if (sketchEntityAgain != null && sketchEntityAgain.HasStyle)
        {
          var styleForSketc = sketchEntityAgain.Style;
        }
        if (sketchEntityAgain != null)
        {
          defaultColor = sketchEntityAgain.Color;
        }
      }
      DisplayStyle displayStyle = new DisplayStyle();
      SX.Drawing.Color colorToUse = (vertex.Color.IsEmpty ? defaultColor : vertex.Color);
      displayStyle.color = ((System.Drawing.Color)colorToUse).ToArgb();
      displayStyle.lineweight = 0;
      displayStyle.linetype = "Continuous";
      specklepoint["displayStyle"] = displayStyle;

      return specklepoint;
    }
  }
}


