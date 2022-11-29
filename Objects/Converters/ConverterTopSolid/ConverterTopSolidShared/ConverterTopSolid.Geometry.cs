﻿using Objects.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

//using Converters.TopSolid;
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

using TSPoint = TopSolid.Kernel.G.D3.Point;
using TsBox = TopSolid.Kernel.G.D3.Box;
using TsBSplineSurface = TopSolid.Kernel.G.D3.Surfaces.BSplineSurface;
//using TsLine = TopSolid.Kernel.DB.D2.L;
using TsInterval = TopSolid.Kernel.G.D1.Generic.Interval<double>;
using TsLineCurve = TopSolid.Kernel.G.D3.Curves.LineCurve;
using TsPlane = TopSolid.Kernel.G.D3.Plane;
using TsPoint = TopSolid.Kernel.G.D3.Point;
using TsPointList = TopSolid.Kernel.G.D3.PointList;
using TsPolylineCurve = TopSolid.Kernel.G.D3.Curves.PolylineCurve;
using TsUnitVector = TopSolid.Kernel.G.D3.UnitVector;
using TsVector = TopSolid.Kernel.G.D3.Vector;
using TsBsplineCurve = TopSolid.Kernel.G.D3.Curves.BSplineCurve;

using TX = TopSolid.Kernel.TX;
using SX = TopSolid.Kernel.SX;
using TopSolid.Kernel.G.D3.Shapes;
//using TopSolid.Kernel.SX.Collections;
using TSX = TopSolid.Kernel.SX.Collections.Generic;
using TKGD3 = TopSolid.Kernel.G.D3;
using TKGD2 = TopSolid.Kernel.G.D2;
using TopSolid.Kernel.G.D3.Surfaces;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Curves;
using Speckle.Core.Kits;
using TopSolid.Kernel.G.D1.Generic;
using TopSolid.Kernel.G.D3.Shapes.FacetShapes;
using TopSolid.Kernel.TX.Items;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.TX.Undo;
using TopSolid.Kernel.G.D3.Shapes.Creations;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.DB.D3.Shapes.Sew;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.TX.Units;
using TopSolid.Kernel.G.D1;
using TopSolid.Kernel.G.D3.Shapes.Polyhedrons;
using Speckle.Core.Models;
using TopSolid.Kernel.GR.Attributes;
using TopSolid.Kernel.DB.D3.Surfaces;
using TopSolid.Kernel.G;
//using TopSolid.Kernel.SX.Collections.Generic;

namespace Objects.Converter.TopSolid
{
    public partial class ConverterTopSolid
    {
        // tolerance for geometry:
        public double tolerance = 0.00001;//modified from 0.000 by AHW

        // Convenience methods:
        #region ConvenienceMethods
        // TODO: Deprecate once these have been added to Objects.sln
        public static double[] PointToArray(TsPoint pt)
        {
            return new double[] { pt.X, pt.Y, pt.Z };
        }

        public static double[] Point2dToArray(TKGD2.Point pt)
        {
            return new double[] { pt.X, pt.Y, 0 };
        }
        public TsPoint[] PointListToNative(IEnumerable<double> arr, string units)
        {

            var enumerable = arr.ToList();
            if (enumerable.Count % 3 != 0) throw new Speckle.Core.Logging.SpeckleException("Array malformed: length%3 != 0.");

            TsPoint[] points = new TsPoint[enumerable.Count / 3];
            var asArray = enumerable.ToArray();
            for (int i = 2, k = 0; i < enumerable.Count; i += 3)
                points[k++] = new TsPoint(
                  ScaleToNative(asArray[i - 2], units),
                  ScaleToNative(asArray[i - 1], units),
                  ScaleToNative(asArray[i], units));

            return points;
        }
        public static double[] PointsToFlatArray(IEnumerable<TsPoint> points)
        {
            return points.SelectMany(pt => PointToArray(pt)).ToArray();
        }
        public static List<double> PointsToFlatList(IEnumerable<TsPoint> points)
        {
            return points.SelectMany(pt => PointToArray(pt)).ToList();
        }

        public static double[] Points2dToFlatArray(IEnumerable<TKGD2.Point> points)
        {
            return points.SelectMany(pt => Point2dToArray(pt)).ToArray();
        }

        public static List<double> Points2dToFlatList(IEnumerable<TKGD2.Point> points)
        {
            return points.SelectMany(pt => Point2dToArray(pt)).ToList();
        }

        private List<double> GetCorrectKnots(List<double> knots, int controlPointCount, int degree)
        {
            var correctKnots = knots;
            if (knots.Count == controlPointCount + degree + 1)
            {
                correctKnots.RemoveAt(0);
                correctKnots.RemoveAt(correctKnots.Count - 1);
            }

            return correctKnots;

        }

        public List<List<ControlPoint>> ControlPointsToSpeckle(TsBSplineSurface surface, string units = null)
        {
            var u = units ?? ModelUnits;

            var points = new List<List<ControlPoint>>();
            int count = 0;
            for (var i = 0; i < surface.UCptsCount; i++)
            {
                var row = new List<ControlPoint>();
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
        public Point PointToSpeckle(TsPoint topSolidpoint, string units = null)
        {
            var u = units ?? ModelUnits;
            Point specklePoint = new Point(topSolidpoint.X, topSolidpoint.Y, topSolidpoint.Z, u);
            SetInstanceParameters(specklePoint, topSolidpoint);
            return specklePoint;
        }
        public TsPoint PointToNative(Point point, string units = null)
        {
            var _point = new TsPoint(ScaleToNative(point.x, point.units),
              ScaleToNative(point.y, point.units),
              ScaleToNative(point.z, point.units));
            return _point;
        }
        #endregion

        // Vectors
        #region Vector
        public Vector VectorToSpeckle(TsVector topSolidVector, string units = null)
        {
            var u = units ?? ModelUnits;
            Vector speckleVector = new Vector(topSolidVector.X, topSolidVector.Y, topSolidVector.Z, u);
            return speckleVector;
        }
        public TsUnitVector UnitVectorToNative(Vector vector)
        {
            return new TsUnitVector(
              ScaleToNative(vector.x, vector.units),
              ScaleToNative(vector.y, vector.units),
              ScaleToNative(vector.z, vector.units));
        }
        public TsVector VectorToNative(Vector vector)
        {
            return new TsUnitVector(
              ScaleToNative(vector.x, vector.units),
              ScaleToNative(vector.y, vector.units),
              ScaleToNative(vector.z, vector.units));
        }
        #endregion

        // Interval
        #region Interval
        public Interval IntervalToSpeckle(TsInterval interval)
        {
            return new Interval(interval.Start, interval.End);
        }
        public TsInterval IntervalToNative(Interval interval)
        {
            return new TsInterval((double)interval.start, (double)interval.end);
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
        public Line LineToSpeckle(TsLineCurve topSolidline, string units = null)
        {
            var u = units ?? ModelUnits;
            Line speckleLine = new Line(PointToSpeckle(topSolidline.Ps), PointToSpeckle(topSolidline.Pe), u);
            SetInstanceParameters(speckleLine, topSolidline);
            return speckleLine;
        }
        public TsLineCurve LineToNative(Line line, string units = null)
        {
            return new TsLineCurve(PointToNative(line.start), PointToNative(line.end));
        }
        #endregion

        // PolylineCurve
        #region Polyline
        public Polyline PolyLineToSpeckle(TsPolylineCurve topSolidPolyline, string units = null)
        {

            var u = units ?? ModelUnits;
            List<double> _coordinates = new List<double>();

            TsPointList pts = topSolidPolyline.CPts;

            foreach (TsPoint p in pts)
            {
                Point _point = PointToSpeckle(p);
                _coordinates.Add(_point.x);
            }

            Polyline specklePolyline = new Polyline(_coordinates, u);
            SetInstanceParameters(specklePolyline, topSolidPolyline);
            return specklePolyline;

        }
        public TsPolylineCurve PolyLineToNative(Polyline polyLine, string units = null)
        {

            TsPointList _pointsList = new TsPointList();

            foreach (Point p in polyLine.GetPoints())
            {
                TsPoint _point = PointToNative(p, units);
                _pointsList.Add(_point);
            }

            return new TsPolylineCurve(polyLine.closed, _pointsList);

        }
        #endregion

        //Curve 2D & 3D
        #region Curve


        public Polycurve ProfileToSpeckle(TKGD3.Curves.GeometricProfile profile, string units = null)
        {
            var u = units ?? ModelUnits;
            Polycurve polyCurve = new Polycurve();
            polyCurve.segments = profile.Segments.Select(x => CurveToSpeckle(x.GetOrientedCurve().Curve)).ToList();
            polyCurve.units = u;
            return polyCurve;
        }

        //Arc      
        public CircleCurve ArcToNative(Arc arc, string units = null)
        {
            //var plane = PlaneToNative(arc.plane);
            CircleCurve circleCurve = new CircleCurve(PlaneToNative(arc.plane), ScaleToNative((double)arc.radius, arc.units));
            CircleMaker maker = new CircleMaker(SX.Version.Current, tolerance, global::TopSolid.Kernel.G.Precision.AngularPrecision);
            maker.SetByCenterAndTwoPoints(
                PointToNative(arc.plane.origin),
                PointToNative(arc.startPoint),
                PointToNative(arc.endPoint),
                false,
                UnitVectorToNative(arc.plane.normal.Unit()),
                circleCurve);

            return circleCurve;

        }

        public ICurve CurveToSpeckle(TKGD3.Curves.Curve curve, string units = null)
        {
            var u = units ?? ModelUnits;
            switch (curve)
            {
                case BSplineCurve bspline:
                    return BSplineCurveToSpeckle(bspline, u);
                case CircleCurve circle:
                    if (circle.IsClosed())
                        return CircleToSpeckle(circle, u);
                    else
                        return ArcToSpeckle(circle, u);
                case LineCurve line:
                    return LineToSpeckle(line, u);
                case PolylineCurve poly:
                    return PolyLineToSpeckle(poly, u);
                default:
                    return BSplineCurveToSpeckle(curve.GetBSplineCurve(false, false));




            }

        }

        public Circle CircleToSpeckle(CircleCurve circ, string units = null)
        {
            var u = units ?? ModelUnits;
            var circle = new Circle(PlaneToSpeckle(circ.Plane, u), circ.Radius, u);
            circle.domain = new Interval(0, 1);
            circle.length = 2 * Math.PI * circ.Radius;
            circle.area = Math.PI * circ.Radius * circ.Radius;
            return circle;
        }

        public Arc ArcToSpeckle(CircleCurve a, string units = null)
        {
            var u = units ?? ModelUnits;

            double angle = (new TsVector(a.Center, a.Ps)).GetAngle(new TsVector(a.Center, a.Pe));
            Arc arc = new Arc(PlaneToSpeckle(a.Plane), PointToSpeckle(a.Ps), PointToSpeckle(a.Pe), angle);

            arc.midPoint = PointToSpeckle(a.Pm, u);
            arc.domain = new Interval(0, 1);
            arc.length = a.GetLength();
            //arc.bbox = BoxToSpeckle(new RH.Box(a.BoundingBox()), u);
            return arc;
        }



        public Objects.Geometry.Curve BSplineCurveToSpeckle(BSplineCurve topSolidCurve, string units = null)
        {
            Curve speckleCurve = new Curve();
            var u = units ?? ModelUnits;


            //Weights
            List<double> ptWeights = new List<double>();
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
                TsPointList polyPoints = new TsPointList();
                for (int i = 0; i < 100; i++)
                {
                    polyPoints.Add(topSolidCurve.GetPoint((range / 100) * i));
                }
                Polyline displayValue = new Polyline();
                displayValue.value = PointsToFlatList(polyPoints);
                displayValue.units = u;
                displayValue.closed = false;

                speckleCurve.displayValue = displayValue;
            }
            catch { }

            //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
            List<double> knots = new List<double>();

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
            speckleCurve.points = PointsToFlatArray(topSolidCurve.CPts).ToList();
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

        public Objects.Geometry.Curve Curve2dToSpeckle(TKGD2.Curves.BSplineCurve topSolidCurve, string units = null)
        {
            Curve speckleCurve = new Curve();
            var u = units ?? ModelUnits; //TODO investigate this


            List<TKGD2.Point> tsPoints = topSolidCurve.CPts.ToList();

            //Weights
            List<double> ptWeights = new List<double>();
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
                TKGD2.PointList polyPoints = new TKGD2.PointList();
                for (int i = 0; i < 100; i++)
                {
                    polyPoints.Add(topSolidCurve.GetPoint((range / 100) * i));
                }
                TKGD2.Curves.PolylineCurve tspoly = new TKGD2.Curves.PolylineCurve(false, polyPoints);
                Polyline displayValue = new Polyline();
                displayValue.value = Points2dToFlatList(polyPoints);
                displayValue.units = u;
                displayValue.closed = false;


                speckleCurve.displayValue = displayValue;
            }
            catch { }

            //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
            List<double> knots = new List<double>();

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

        private TKGD3.Curves.Curve CircleToNative(Circle circle)
        {

            CircleCurve circleCurve = new CircleCurve(PlaneToNative(circle.plane), ScaleToNative((double)circle.radius, circle.units));

            return circleCurve;

        }
        public TKGD3.Curves.Curve CurveToNative(ICurve curve)
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
                    return LineToNative(line);

                //case Polycurve polycurve:
                //    return PolycurveToNative(polycurve);

                default:
                    return null;
            }
        }

        public EllipseCurve EllipseToNative(Ellipse ellipse)
        {
            return new EllipseCurve(
                PlaneToNative(ellipse.plane),
                ScaleToNative((double)ellipse.firstRadius, ellipse.units),
                ScaleToNative((double)ellipse.secondRadius, ellipse.units));

        }


        public PolylineCurve PolylineToNative(Polyline polyline)
        {
            return new TsPolylineCurve(polyline.closed, ToNativePointList(polyline.points));

        }

        public GeometricProfile PolycurveToNative(Polycurve polycurve)
        {
            GeometricProfile profile = new GeometricProfile();
            foreach (ICurve segment in polycurve.segments)
            {
                profile.Add(CurveToNative(segment));
            }
            return profile;
        }

        public TsBsplineCurve CurveToNative(Curve curve, string units = null)
        {
            //var u = units ?? ModelUnits;
            bool isRational = curve.rational;
            bool isPeriodic = curve.periodic;
            int degree = curve.degree;

            SX.Collections.DoubleList nativeKnot = ToNativeDoubleList(curve.knots);
            var ptsList = curve.GetPoints();
            PointList nativePts = ToNativePointList(ptsList);
            SX.Collections.DoubleList nativeWeights = ToNativeDoubleList(curve.weights.ToList());
            BSpline bspline = new BSpline(isPeriodic, degree, nativeKnot);
            if (isRational)
            {
                //var w = c.Points.ConvertAll(x => x.Weight);
                BSplineCurve bsplineCurve = new BSplineCurve(bspline, nativePts, nativeWeights);
                bsplineCurve.SetRange((double)curve.domain.start, (double)curve.domain.end);
                return bsplineCurve;
            }
            else
            {
                BSplineCurve bsplineCurve = new BSplineCurve(bspline, nativePts);
                bsplineCurve.SetRange((double)curve.domain.start, (double)curve.domain.end);
                return bsplineCurve;
            }

        }
        #endregion


        // Box
        #region Box
        public Box BoxToSpeckle(TsBox topSolidBox, string units = null)
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
        public TsBox BoxToNative(Box box)
        {
            // TODO: BOX To Topsolid
            return new TsBox();
        }
        #endregion

        // Surface
        #region Surface

        public Surface SurfaceToSpeckle(TsBSplineSurface topSolidSurface, string units = null)
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
        public TsBSplineSurface SurfaceToNative(Surface surface, string units = null)
        {
            //var u = units ?? ModelUnits;
            // Create TopSolid surface
            //AHW incorrect cause scaling twice ?
            //List<List<ControlPoint>> surfPts = surface.GetControlPoints().Select(l => l.Select(p =>
            //  new ControlPoint(
            //    ScaleToNative(p.x, p.units),
            //    ScaleToNative(p.y, p.units),
            //    ScaleToNative(p.z, p.units),
            //    p.weight,
            //    p.units)).ToList()).ToList();

            List<List<ControlPoint>> surfPts = surface.GetControlPoints().Select(l => l.Select(p =>
              new ControlPoint(
                p.x,
                p.y,
                p.z,
                p.weight,
                p.units)).ToList()).ToList();


            var uKnots = SurfaceKnotsToNative(surface.knotsU);
            var vKnots = SurfaceKnotsToNative(surface.knotsV);
            var ctPts = ControlPointsToNative(surfPts);

            BSpline vBspline = new BSpline(surface.closedV, surface.degreeV, ToDoubleList(vKnots));
            //vBspline.SetRange(new global::TopSolid.Kernel.G.D1.Extent((double)surface.domainV.start, (double)surface.domainV.end));
            BSpline uBspline = new BSpline(surface.closedU, surface.degreeU, ToDoubleList(uKnots));
            //uBspline.SetRange(new global::TopSolid.Kernel.G.D1.Extent((double)surface.domainU.start, (double)surface.domainU.end));

            //for (int u = 0; u < points.Count; u++)
            //{
            //    for (int v = 0; v < points[u].Count; v++)
            //    {
            //        _surface.SetCPt(u, v, PointToNative(points[u][v]));
            //        _surface.SetCWt(u, v, points[u][v].weight);
            //        _surface.UBs[u] = surface.knotsU[u];
            //        _surface.VBs[v] = surface.knotsV[v];
            //    }
            //}

            // TODO : Rational option
            if (surface.rational)
            {
                TsBSplineSurface bs = new TsBSplineSurface(uBspline, vBspline, ctPts, ToDoubleList(surfPts.SelectMany(x => x).Select(x => x.weight)));
                return bs;
            }
            else
            {
                TsBSplineSurface bs = new TsBSplineSurface(uBspline, vBspline, ctPts);
                var gtype = bs.GeometryType;



                //bs.SetRangeFull();

                return bs;
            }



        }

        private PointList ControlPointsToNative(List<List<ControlPoint>> controlPoints)
        {
            var uCount = controlPoints.Count;
            var vCount = controlPoints[0].Count;
            var count = uCount * vCount;

            var points = new PointList(count);
            int p = 0;

            foreach (var row in controlPoints)
            {
                foreach (var pt in row)
                {
                    var point = new Point(pt.x, pt.y, pt.z, pt.units);
                    points.Add(PointToNative(point));
                }
            }


            //controlPoints.ForEach(row =>
            //  row.ForEach(pt =>
            //  {
            //      var point = new Point(pt.x, pt.y, pt.z, pt.units);
            //      points[p++] = PointToNative(point);
            //  }));

            return points;
        }

        public double[] SurfaceKnotsToNative(List<double> list)
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

            //Variables and global counters (not to be reinitialized for each face)
            double tol = global::TopSolid.Kernel.G.Precision.LinearPrecision;
            var u = units ?? ModelUnits;
            int faceindex = 0;
            int loopIndex = 0;
            spcklBrep.units = u;
            int startVertInd = 0;
            int endVertInd = 0;
            int facecount = _shape.FaceCount;

            //Lists to get Curves and Edges for each face
            List<TSX.List<TKGD2.Curves.IGeometricProfile>> global2dList = new List<TSX.List<TKGD2.Curves.IGeometricProfile>>(facecount);
            List<TSX.List<TKGD3.Curves.IGeometricProfile>> global3dList = new List<TSX.List<TKGD3.Curves.IGeometricProfile>>(facecount);
            List<TSX.List<EdgeList>> globalEdgeList = new List<TSX.List<EdgeList>>(facecount);
            List<SX.Collections.BoolList> globalBoolList = new List<SX.Collections.BoolList>(facecount);

            //uv curves, 3d curves and surfaces, per face
            foreach (Face face in _shape.Faces)
            {
                global2dList.Add(new TSX.List<TKGD2.Curves.IGeometricProfile>());
                global3dList.Add(new TSX.List<IGeometricProfile>());
                globalEdgeList.Add(new TSX.List<EdgeList>());
                globalBoolList.Add(new SX.Collections.BoolList());

                var loop2d = global2dList[faceindex];
                var loop3d = global3dList[faceindex];
                var tsEgdes = globalEdgeList[faceindex];
                var boolList = globalBoolList[faceindex];

                //GetTopological info of face
                OrientedSurface surf = face.GetOrientedBsplineTrimmedGeometry(tol, false, false, false, boolList, loop2d, loop3d, tsEgdes);

                //Surface
                spcklBrep.Surfaces.Add(SurfaceToSpeckle(surf.Surface as BSplineSurface, u));

                faceindex++;
            }

            //Flatten lists
            var crv2d = global2dList.SelectMany(x => x.SelectMany(y => y.Segments));
            var crv3d = global3dList.SelectMany(x => x.SelectMany(y => y.Segments));
            var edges = globalEdgeList.SelectMany(x => x.SelectMany(y => y));
            var tupList = new List<(Edge Edge, IGeometricSegment Crv3d, TKGD2.Curves.IGeometricSegment Crv2d)>();


            //Vertices
            List<Vertex> tsVerticesList = _shape.Vertices.ToList();
            spcklBrep.Vertices = tsVerticesList
              .Select(vertex => PointToSpeckle(vertex.GetGeometry(), u)).ToList();

            int counter = 0;

            //Create a list of tuple linking Edges, crv3d and crv2d ===> some edges are thus repeated
            foreach (var edge in edges)
            {
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
            int j = 0;
            int K = 0;
            var tupwithfaces = new List<(Edge Edge, IGeometricSegment Crv3d, TKGD2.Curves.IGeometricSegment Crv2d, int Findex, int Counter, int LoopIndex, int EdgeIndex)>();
            foreach (var lst in global2dList) //loop through faces
            {
                //i = 0;
                foreach (var profile in lst) //loop through loops
                {
                    j = 0;
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

                            var mytup = (Edge: edge, crv3d: tupList.ElementAt(counter).Crv3d, crv2d: tupList.ElementAt(counter).Crv2d, faceindex: K, Counter: counter, LoopIndex: i, EdgeIndex: EdgeIndex);
                            //tupwithfaces.Add(new Tuple<Edge, IGeometricSegment, TKGD2.Curves.IGeometricSegment, int>(tup.ElementAt(counter).Item1, tup.ElementAt(counter).Item2, tup.ElementAt(counter).Item3, K));
                            tupwithfaces.Add(mytup);
                            j++;
                            counter++;
                        }


                    }
                    i++;
                }
                K++;
            }

            //Create a list of Tuple which associates each edge to a 3d curve and a list of 2D trims
            var tupforTrims = new List<(Edge Edge, IGeometricSegment Crv3d, List<TKGD2.Curves.IGeometricSegment> TrimCrvs, List<int> Crv2dindices)>();
            foreach (var ed in _shape.Edges.OrderBy(x => edges.ToList().IndexOf(x)))
            {
                var localTups = tupList.Where(x => x.Edge == ed); //Get all the tuple with this same edge
                var crv2dIndices = new List<int>(localTups.Count());
                foreach (var tup in localTups) //get the indices of the 2d crvs
                {
                    crv2dIndices.Add(tupList.IndexOf(tup));
                }

                var mytup = (Edge: ed, Crv3d: tupList.Where(x => x.Edge == ed).First().Crv3d, TrimCrvs: tupList.Where(x => x.Edge == ed).Select(x => x.Crv2d).ToList(), Crv2dindices: crv2dIndices);
                tupforTrims.Add(mytup);
            }

            //Loop list needed for face definition and later for loop def
            var tsLoopList = _shape.Loops.ToList();

            int faceind = 0;
            int outerindex = 0;

            //Add Faces with correct loops
            foreach (Face face in _shape.Faces)
            {
                List<int> faceLoopIndices = new List<int>(face.LoopCount);
                var list = face.Loops;
                foreach (var loop in list)
                {
                    var ind = tsLoopList.IndexOf(loop);
                    faceLoopIndices.Add(ind);
                    if (loop.IsOuter)
                        outerindex = ind;
                }
                spcklBrep.Faces.Add(new BrepFace(spcklBrep, faceind, faceLoopIndices, outerindex, face.IsReversed()));
                faceind++;
            }

            //Add 3d Curves non repeated
            BSplineCurve bsCrv3d;
            foreach (var t in tupforTrims)
            {
                bsCrv3d = t.Crv3d.GetOrientedCurve().Curve.GetBSplineCurve(false, false);
                spcklBrep.Curve3D.Add(BSplineCurveToSpeckle(bsCrv3d, u));
            }

            //Add 2D curves
            TKGD2.Curves.BSplineCurve bsCrv2d;
            foreach (var t in tupList)
            {
                bsCrv2d = t.Crv2d.GetOrientedCurve().Curve.GetBSplineCurve(false, false);
                spcklBrep.Curve2D.Add(Curve2dToSpeckle(bsCrv2d, Units.None));
            }

            //Add Edges with correct trims
            counter = 0;
            foreach (var tuple in tupforTrims)
            {
                var localEdge = tuple.Edge;
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
                List<int> triminds = new List<int>();
                var localFace = l.GetFace();
                faceind = tsFaceList.IndexOf(localFace);

                BrepLoopType type;
                if (l.IsInner)
                    type = BrepLoopType.Inner;
                else if (l.IsOuter)
                    type = BrepLoopType.Outer;
                else type = BrepLoopType.Unknown;

                var loop = new BrepLoop();
                loop.Brep = spcklBrep;
                loop.FaceIndex = tsFaceList.IndexOf(l.GetFace());
                //loop.TrimIndices = trimIndices;
                loop.Type = type;

                //Better ordered than getting the Edges via Loop.Edges
                foreach (var tup in tupwithfaces)
                {
                    if (tup.LoopIndex != loopIndex)
                        continue;
                    else
                    {
                        var trim = new BrepTrim();


                        var ind = tup.EdgeIndex;
                        var in2d = tup.Counter;

                        trim.Brep = spcklBrep;
                        trim.EdgeIndex = ind;
                        trim.FaceIndex = tup.Findex;
                        trim.LoopIndex = loopIndex;
                        trim.CurveIndex = in2d;
                        trim.IsoStatus = 0;
                        if (tup.Edge.Type == EdgeType.Boundary)
                            trim.TrimType = BrepTrimType.Boundary;
                        else
                            trim.TrimType = BrepTrimType.Unknown;
                        var c2d = tup.Crv2d;
                        //trim.IsReversed = localtup.Edge.IsReversedWithFin(localFace);
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
            List<Mesh> displayValue = new List<Mesh>();
            displayValue.Add(ShapeDisplayToMesh(shape, u));
            spcklBrep.displayValue = displayValue;
            SetInstanceParameters(spcklBrep, shape);
            return spcklBrep;
        }

        OperationList operationsList = new OperationList();
        private Shape BrepToNative(Brep brep, string units = null)
        {
            var u = units ?? ModelUnits;
            ModelingDocument doc = Doc;



            //Brep rs = null;
            double tol = 0;
            tol = (global::TopSolid.Kernel.G.Precision.ModelingLinearTolerance);
            ShapeList shape = BrepToShapeList(brep, tol);

            FolderOperation folderOperation = new FolderOperation(doc, 0);
            //folderOperation.Name = $"Speckle creation : {brep.GetId()}";
            folderOperation.Create();

            EntitiesCreation shapesCreation = new EntitiesCreation(doc, 0);

            SewOperation sewOperation = new SewOperation(doc, 0);
            //sewOperation.Name = $"brep : {brep.GetId()}";

            foreach (var ts in shape)
            {
                ShapeEntity se = new ShapeEntity(doc, 0);
                //se.Name = $"brep : {brep.GetId()}";
                se.Geometry = ts;
                //se.Parent = shapesCreation;
                //se.Create(doc.ShapesFolderEntity);
                shapesCreation.AddChildEntity(se);
                shapesCreation.CanDeleteFromChild(se);
            }

            if (shape.Count == 1)
            {
                shapesCreation.Create(folderOperation);
           
                return shape[0];
            }


            //shapesCreation.Owner = sewOperation;
            //shapesCreation.Create(folderOperation);
            //shapesCreation.Create();

            if (shapesCreation.ChildrenEntities.Count() != 0)
            {
                sewOperation.ModifiedEntity = shapesCreation.ChildrenEntities.First() as ShapeEntity; // TODO : Question : Why ChildrenEntities is empty ???
            }
            for (int i = 1; i < shapesCreation.ChildEntityCount; i++)
            {
                //shapesCreation.ChildrenEntities.ElementAt(i).IsGhost = true;
                sewOperation.AddTool(new ProvidedSmartShape(sewOperation, shapesCreation.ChildrenEntities.ElementAt(i)));
            }

            if (tol != 0)
                sewOperation.GapWidth = new BasicSmartReal(sewOperation, tol, UnitType.Length, doc);
            else
                sewOperation.GapWidth = new BasicSmartReal(sewOperation, global::TopSolid.Kernel.G.Precision.ModelingLinearTolerance, UnitType.Length, doc);

            sewOperation.NbIterations = new BasicSmartInteger(sewOperation, 5);
            sewOperation.AddOperation(shapesCreation);
            // var op = Doc.RootOperation.DeepConstituents.Where(x => x.Name == "SpeckleCreation").FirstOrDefault() as FolderOperation;
            //if (op != null)
            //{
            //    sewOperation.Owner = op;
            //}
            sewOperation.Create(folderOperation);
            doc.Update(true, true);

            //Hides other shapes when successfull, otherwise keep them shown
            bool isInvalid = sewOperation.IsInvalid;
            if (!isInvalid)
            {
                for (int i = 1; i < shapesCreation.ChildEntityCount; i++)
                {
                    //shapesCreation.ChildrenEntities.ElementAt(i).Hide();
                    shapesCreation.ChildrenEntities.ElementAt(i).IsGhost = true;
                }

            }

            //TODO Move the Shape creation in specific function


            //ShapeEntity se = new ShapeEntity(doc, 0);
            //se.Geometry = shape;
            //se.Create(doc.ShapesFolderEntity);

            var ent = sewOperation.ModifiedShapeEntity;
            if (ent != null)
            {
                var display = DiplayToNative(brep);
                ent.ExplicitColor = display.Item1;
                ent.ExplicitTransparency = display.Item2;
                //ent.Name = $"brep : {brep.GetId()}";
                doc.ShapesFolderEntity.AddEntity(ent);
            }


            //operationsList.Add(sewOperation);           



            //return sewOperation.ShapeEntities.First().Geometry as Shape;
            return ent.Geometry as Shape;
        }

        public ShapeList BrepToShapeList(Brep brep, double tol = global::TopSolid.Kernel.G.Precision.ModelingLinearTolerance, string units = null)
        {
            //var u = units ?? ModelUnits;
            double tol_TS = tol;
            Shape shape = null;
            ShapeList ioShapes = new ShapeList();
            int faceind = 0;
            foreach (BrepFace bface in brep.Faces)
            {
                shape = null;
                shape = MakeSheetFrom3d(brep, bface, tol_TS, faceind++);

                if (shape == null || shape.IsEmpty)
                {
                }
                else
                    ioShapes.Add(shape);
            }


            return ioShapes;
        }
        private Shape MakeSheetFrom3d(Brep inBRep, BrepFace inFace, double inLinearPrecision, int faceindex, string units = null)
        {
            Shape shape = new Shape(null);

            TrimmedSheetMaker sheetMaker = new TrimmedSheetMaker(SX.Version.Current);
            sheetMaker.LinearTolerance = inLinearPrecision;
            sheetMaker.UsesBRepMethod = false;

            TX.Items.ItemMonikerKey key = new TX.Items.ItemMonikerKey(TX.Items.ItemOperationKey.BasicKey);

            // Get surface and set to maker.

            Surface surface = inBRep.Surfaces[inFace.SurfaceIndex];

            // Reverse surface and curves in 3d mode(according to the drilled cylinder crossed by cube in v5_example.3dm).
            //if (inFace.rev)
            //    surface = ImporterHelper.MakeReversed(surface); // Useless.

            // Closed BSpline surfaces must not be periodic for parasolid with 3d curves (according to wishbone.3dm and dinnermug.3dm).
            // If new problems come, see about the periodicity of the curves.

            //TODO check if planar to simplify            

            BSplineSurface bsSurface = SurfaceToNative(surface);

            if (bsSurface != null && (bsSurface.IsUPeriodic || bsSurface.IsVPeriodic))
            {
                bsSurface = (BSplineSurface)bsSurface.Clone();

                if (bsSurface.IsUPeriodic)
                    bsSurface.MakeUNonPeriodic();

                if (bsSurface.IsVPeriodic)
                    bsSurface.MakeVNonPeriodic();
                //bsSurface.MakeVNonPeriodic();

            }

            sheetMaker.Surface = new OrientedSurface(bsSurface, false);
            sheetMaker.SurfaceMoniker = new ItemMoniker(false, (byte)ItemType.ShapeFace, key, 1);

            // Get spatial curves and set to maker.
            int loopCount = inBRep.Faces.ElementAt(faceindex).Loops.Count;
            TSX.List<TKGD3.Curves.CurveList> loops3d = new TSX.List<TKGD3.Curves.CurveList>(loopCount);
            TSX.List<ItemMonikerList> listItemMok = new TSX.List<ItemMonikerList>();
            for (int k = 0; k < loopCount; k++)
            {
                loops3d.Add(new TKGD3.Curves.CurveList());
                listItemMok.Add(new ItemMonikerList());
            }



            int i = 0;

            //List<int> indices = new List<int>();


            int loopindex = 0;
            int counter = 0;
            foreach (BrepLoop loop in inBRep.Faces.ElementAt(faceindex).Loops)
            {
                foreach (BrepTrim trim in loop.Trims)
                {
                    TKGD3.Curves.Curve nativeCurve = CurveToNative(inBRep.Curve3D.ElementAt(trim.Edge.Curve3dIndex)); //TODO check a more general way to cast ICurve to Curve even for lines

                    if (nativeCurve != null)
                    {
                        var convertedCrv = nativeCurve;
                        listItemMok.ElementAt(loopindex).Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, i++));
                        loops3d.ElementAt(loopindex).Add(convertedCrv);
                    }

                    else if (inBRep.Curve3D.ElementAt(trim.Edge.Curve3dIndex) is Polycurve polyCurve)
                    {
                        GeometricProfile profile = PolycurveToNative(polyCurve);
                        if (profile != null)
                        {
                            foreach (var seg in profile.Segments)
                            {
                                listItemMok.ElementAt(loopindex).Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, i++));
                                loops3d.ElementAt(loopindex).Add(seg.GetOrientedCurve().Curve);
                            }
                        }
                    }
                    counter++;
                }
                loopindex++;
            }
            if (loops3d != null && loops3d.Count != 0)
            {
                // if (inFace.rev == false || ImporterHelper.MakeReversed(loops3d)) // Useless
                {
                    sheetMaker.SetCurves(loops3d, listItemMok);
                    try
                    {
                        shape = sheetMaker.Make(null, ItemOperationKey.BasicKey);
                    }
                    catch (Exception e)
                    {
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

            var verts = new List<double>();
            List<int> vertIndices = new List<int>();
            int ind = 0;
            var faces = new List<int>();
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

            return speckleMesh;
        }
        #endregion


        //PolyHedron
        public Mesh PolyhedronToSpeckle(Polyhedron polyhedron, string units = null)
        {
            var u = units ?? ModelUnits;

            var verts = new List<double>();
            List<int> vertIndices = new List<int>();
            int ind = 0;
            var faces = new List<int>();


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
    }
}


