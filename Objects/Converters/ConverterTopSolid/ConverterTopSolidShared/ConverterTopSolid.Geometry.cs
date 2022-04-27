using Objects.Geometry;
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
using TsUVector = TopSolid.Kernel.G.D3.UnitVector;
using TsVector = TopSolid.Kernel.G.D3.Vector;
using TsBsplineCurve = TopSolid.Kernel.G.D3.Curves.BSplineCurve;

using TX = TopSolid.Kernel.TX;
using SX = TopSolid.Kernel.SX;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.SX.Collections;
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

        public static double[] Points2dToFlatArray(IEnumerable<TKGD2.Point> points)
        {
            return points.SelectMany(pt => Point2dToArray(pt)).ToArray();
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
        public Point PointToSpeckle(TsPoint point, string units = null)
        {
            var u = units ?? ModelUnits;
            return new Point(point.X, point.Y, point.Z, u);
        }
        public TsPoint PointToNative(Point point)
        {
            var _point = new TsPoint(ScaleToNative(point.x, point.units),
              ScaleToNative(point.y, point.units),
              ScaleToNative(point.z, point.units));
            return _point;
        }
        #endregion

        // Vectors
        #region Vector
        public Vector VectorToSpeckle(TsVector vector, string units = null)
        {
            var u = units ?? ModelUnits;
            return new Vector(vector.X, vector.Y, vector.Z, u);
        }
        public TsUVector VectorToNative(Vector vector)
        {
            return new TsUVector(
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
        public Plane PlaneToSpeckle(TsPlane plane, string units = null)
        {
            var u = units ?? ModelUnits;
            return new Plane(PointToSpeckle(plane.Po, u), VectorToSpeckle(plane.Vz, u), VectorToSpeckle(plane.Vx, u), VectorToSpeckle(plane.Vy, u), u);
        }
        public TsPlane PlaneToNative(Plane plane)
        {
            return new TsPlane(PointToNative(plane.origin), VectorToNative(plane.normal));
        }
        #endregion

        // LineCurve
        #region Line
        public Line LineToSpeckle(TsLineCurve line, string units = null)
        {
            var u = units ?? ModelUnits;
            return new Line(PointToSpeckle(line.Ps), PointToSpeckle(line.Pe), u);
        }
        public TsLineCurve LineToNative(Line line)
        {
            return new TsLineCurve(PointToNative(line.start), PointToNative(line.end));
        }
        #endregion

        // PolylineCurve
        #region Polyline
        public TsPolylineCurve PolyLineToNative(Polyline polyLine)
        {

            TsPointList _pointsList = new TsPointList();

            foreach (Point p in polyLine.points)
            {
                TsPoint _point = PointToNative(p);
                _pointsList.Add(_point);
            }

            return new TsPolylineCurve(polyLine.closed, _pointsList);

        }
        public Polyline PolyLineToSpeckle(TsPolylineCurve polyLine, string units = null)
        {

            var u = units ?? ModelUnits;
            List<double> _coordinates = new List<double>();

            TsPointList pts = polyLine.CPts;

            foreach (TsPoint p in pts)
            {
                Point _point = PointToSpeckle(p);
                _coordinates.Add(_point.x);
            }

            return new Polyline(_coordinates, u);

        }
        #endregion

        //Curve 2D & 3D
        #region Curve
        public Objects.Geometry.Curve CurveToSpeckle(BSplineCurve tsCurve, string units = null)
        {
            var curve = new Curve();
            var u = units ?? ModelUnits;


            List<TSPoint> tsPoints = tsCurve.CPts.ToList();

            //Weights
            List<double> ptWeights = new List<double>();
            try
            {
                if (tsCurve.CWts.Count != 0)
                {
                    foreach (double weight in tsCurve.CWts)
                    {
                        ptWeights.Add(weight);
                    }
                }
            }
            catch { }

            try
            {
                double range = (tsCurve.Te - tsCurve.Ts);
                PointList polyPoints = new PointList();
                for (int i = 0; i < 100; i++)
                {
                    polyPoints.Add(tsCurve.GetPoint((range / 100) * i));
                }
                TKGD3.Curves.PolylineCurve tspoly = new PolylineCurve(false, polyPoints);
                Polyline displayValue = new Polyline(PointsToFlatArray(polyPoints));


                curve.displayValue = displayValue;
            }
            catch { }

            //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
            List<double> knots = new List<double>();

            for (int i = 0; i < (tsCurve.Bs.Count); i++)
            {
                knots.Add(tsCurve.Bs.ElementAt(i));

            }

            //Prevent errors when weight list is empty
            if (tsCurve.CWts.Count == 0)
            {
                ptWeights.Clear();
                for (int i = 0; i < tsCurve.CPts.Count; i++)
                {
                    ptWeights.Add(1.0);
                }
            }

            Interval interval = new Interval(tsCurve.Ts, tsCurve.Te);

            //set speckle curve info
            curve.points = PointsToFlatArray(tsCurve.CPts).ToList();
            curve.knots = knots;
            curve.weights = ptWeights;
            curve.degree = tsCurve.Degree;
            curve.periodic = tsCurve.IsPeriodic;
            curve.rational = tsCurve.IsRational;
            curve.closed = tsCurve.IsClosed();
            curve.length = tsCurve.GetLength();
            curve.domain = interval;
            //curve.bbox = BoxToSpeckle(spline.GeometricExtents, true);
            curve.units = u;

            return curve;
        }

        public Objects.Geometry.Curve Curve2dToSpeckle(TKGD2.Curves.BSplineCurve tsCurve, string units = null)
        {
            var curve = new Curve();
            var u = units ?? ModelUnits; //TODO investigate this


            List<TKGD2.Point> tsPoints = tsCurve.CPts.ToList();

            //Weights
            List<double> ptWeights = new List<double>();
            try
            {
                if (tsCurve.CWts.Count != 0)
                {
                    foreach (double weight in tsCurve.CWts)
                    {
                        ptWeights.Add(weight);
                    }
                }
            }
            catch { }

            try
            {
                double range = (tsCurve.Te - tsCurve.Ts);
                TKGD2.PointList polyPoints = new TKGD2.PointList();
                for (int i = 0; i < 100; i++)
                {
                    polyPoints.Add(tsCurve.GetPoint((range / 100) * i));
                }
                TKGD2.Curves.PolylineCurve tspoly = new TKGD2.Curves.PolylineCurve(false, polyPoints);
                Polyline displayValue = new Polyline(Points2dToFlatArray(polyPoints));


                curve.displayValue = displayValue;
            }
            catch { }

            //for the knot, the parasolid model uses 2 values more than Rhino, first and last to be removed
            List<double> knots = new List<double>();

            for (int i = 0; i < (tsCurve.Bs.Count); i++)
            {
                knots.Add(tsCurve.Bs.ElementAt(i));
            }

            //Prevent errors when weight list is empty
            if (tsCurve.CWts.Count == 0)
            {
                ptWeights.Clear();
                for (int i = 0; i < tsCurve.CPts.Count; i++)
                {
                    ptWeights.Add(1.0);
                }
            }

            Interval interval = new Interval(tsCurve.Ts, tsCurve.Te);

            //set speckle curve info
            curve.points = Points2dToFlatArray(tsCurve.CPts).ToList();
            curve.knots = knots;
            curve.weights = ptWeights;
            curve.degree = tsCurve.Degree;
            curve.periodic = tsCurve.IsPeriodic;
            curve.rational = tsCurve.IsRational;
            curve.closed = tsCurve.IsClosed();
            curve.length = tsCurve.GetLength();
            curve.domain = interval;
            //curve.bbox = BoxToSpeckle(spline.GeometricExtents, true);
            curve.units = u;

            return curve;
        }
        #endregion

        public TsBsplineCurve CurveToNative(Curve c)
        {
            bool r = c.rational;
            bool p = c.periodic;
            int d = c.degree;

            DoubleList k = ToDoubleList(c.knots);
            var ptsList = c.GetPoints();
            PointList pts = ToPointList(ptsList);
            DoubleList w = ToDoubleList(c.weights.ToList());
            BSpline b = new BSpline(p, d, k);
            if (r)
            {
                //var w = c.Points.ConvertAll(x => x.Weight);
                BSplineCurve bs = new BSplineCurve(b, pts, w);
                return bs;
            }
            else
            {
                BSplineCurve bs = new BSplineCurve(b, pts);
                return bs;
            }

        }


        // Box
        #region Box
        public TsBox BoxToNative(Box box)
        {
            // TODO: BOX To Topsolid
            return new TsBox();
        }
        public Box BoxToSpeckle(TsBox box, string units = null)
        {
            try
            {

                var u = units ?? ModelUnits;

                Box _box = null;


                Frame tsFrame = box.Frame;
                Plane spcklPlane = new Plane(new Point(tsFrame.Po.X, tsFrame.Po.Y, tsFrame.Po.Z, u), VectorToSpeckle(tsFrame.Vz, u), VectorToSpeckle(tsFrame.Vx, u), VectorToSpeckle(tsFrame.Vy, u), u);

                _box = new Box(spcklPlane, new Interval(-box.Hx, box.Hx), new Interval(-box.Hy, box.Hy), new Interval(-box.Hz, box.Hz), u);
                //_box.area = (box.Hx * 2 * box.Hy * 2 * 2) + (box.Hx * 2 * box.Hz * 2 * 2) + (box.Hz * 2 * box.Hy * 2 * 2);
                _box.volume = box.Volume;
                _box.units = u;

                return _box;

            }
            catch
            {
                return null;
            }
        }
        #endregion

        // Surface
        #region Surface
        public TsBSplineSurface SurfaceToNative(Surface surface)
        {
            // Create TopSolid surface
            List<List<ControlPoint>> points = surface.GetControlPoints().Select(l => l.Select(p =>
              new ControlPoint(
                ScaleToNative(p.x, p.units),
                ScaleToNative(p.y, p.units),
                ScaleToNative(p.z, p.units),
                p.weight,
                p.units)).ToList()).ToList();


            //TsBSplineSurface _surface = new TsBSplineSurface();

            BSpline vBspline = new BSpline(surface.closedV, surface.degreeV, ToDoubleList(surface.knotsV));
            BSpline UBspline = new BSpline(surface.closedU, surface.degreeU, ToDoubleList(surface.knotsU));

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
                TsBSplineSurface bs = new TsBSplineSurface(UBspline, vBspline, ToPointList(points.SelectMany(x => x)), ToDoubleList(points.SelectMany(x => x).Select(x => x.weight)));
                return bs;
            }
            else
            {
                TsBSplineSurface bs = new TsBSplineSurface(UBspline, vBspline, ToPointList(points.SelectMany(x => x)));
                return bs;
            }



        }

        public Surface SurfaceToSpeckle(TsBSplineSurface surface, string units = null)
        {
            var u = units ?? ModelUnits;
            var _surface = new Geometry.Surface
            {
                degreeU = surface.UDegree,
                degreeV = surface.VDegree,
                rational = surface.IsRational,
                closedU = surface.IsUClosed,
                closedV = surface.IsVClosed,
                domainU = new Interval(surface.Us, surface.Ue),
                domainV = new Interval(surface.Vs, surface.Ve),
                knotsU = GetCorrectKnots(surface.UBs.ToList(), surface.UCptsCount, surface.UDegree),
                knotsV = GetCorrectKnots(surface.VBs.ToList(), surface.VCptsCount, surface.VDegree)
            };

            _surface.SetControlPoints(ControlPointsToSpeckle(surface));
            _surface.units = u;

            return _surface;
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
            List<BoolList> globalBoolList = new List<BoolList>(facecount);


            //uv curves, 3d curves and surfaces, per face
            foreach (Face face in _shape.Faces)
            {
                global2dList.Add(new TSX.List<TKGD2.Curves.IGeometricProfile>());
                global3dList.Add(new TSX.List<IGeometricProfile>());
                globalEdgeList.Add(new TSX.List<EdgeList>());
                globalBoolList.Add(new BoolList());

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
                spcklBrep.Curve3D.Add(CurveToSpeckle(bsCrv3d, u));
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
            spcklBrep.displayMesh = ShapeDisplayToMesh(shape, u);

            return spcklBrep;
        }


        private Shape BrepToNative(Brep brep, string units = null)
        {
            ModelingDocument doc = global::TopSolid.Kernel.UI.Application.CurrentDocument as ModelingDocument;
            UndoSequence.UndoCurrent();
            UndoSequence.Start("Bake", true);


            //Brep rs = null;
            double tol = 0;
            tol = global::TopSolid.Kernel.G.Precision.ModelingLinearTolerance;
            ShapeList shape = BrepToShapeList(brep, tol);

            EntitiesCreation shapesCreation = new EntitiesCreation(doc, 0);


            foreach (var ts in shape)
            {
                ShapeEntity se = new ShapeEntity(doc, 0);
                se.Geometry = ts;

                se.Create(doc.ShapesFolderEntity);
                shapesCreation.AddChildEntity(se);
                shapesCreation.CanDeleteFromChild(se);

            }

            shapesCreation.Create();


            SewOperation sewOperation = new SewOperation(doc, 0);
            sewOperation.ModifiedEntity = shapesCreation.ChildrenEntities.First() as ShapeEntity;
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
            sewOperation.Create();

            //Hides other shapes when successfull, otherwise keep them shown
            bool isInvalid = sewOperation.IsInvalid;
            if (!isInvalid)
            {
                for (int i = 1; i < shapesCreation.ChildEntityCount; i++)
                {
                    shapesCreation.ChildrenEntities.ElementAt(i).Hide();
                    //shapesCreation.ChildrenEntities.ElementAt(i).IsGhost = true;
                }
            }

            //TODO
            //ShapeEntity se = new ShapeEntity(doc, 0);
            //se.Geometry = shape;
            //se.Create(doc.ShapesFolderEntity);



            UndoSequence.End();

            return sewOperation.ShapeEntities.First().Geometry as Shape;
        }

        public ShapeList BrepToShapeList(Brep brep, double tol = global::TopSolid.Kernel.G.Precision.ModelingLinearTolerance)
        {

            double tol_TS = tol;
            Shape shape = null;
            ShapeList ioShapes = new ShapeList();
            int faceind = 0;
            foreach (BrepFace bface in brep.Faces)
            {
                shape = null;


                shape = MakeSheetFrom3d(brep, bface, tol_TS, faceind++);

                if (shape == null || shape.IsEmpty)
                { }
                else
                    ioShapes.Add(shape);
            }


            return ioShapes;
        }
        private Shape MakeSheetFrom3d(Brep inBRep, BrepFace inFace, double inLinearPrecision, int faceindex)
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
                bsSurface.MakeVNonPeriodic();

                //surface = bsSurface;
            }


            sheetMaker.Surface = new OrientedSurface(bsSurface, false);
            sheetMaker.SurfaceMoniker = new ItemMoniker(false, (byte)ItemType.ShapeFace, key, 1);

            // Get spatial curves and set to maker.
            TSX.List<TKGD3.Curves.CurveList> loops3d = new TSX.List<TKGD3.Curves.CurveList>();
            loops3d.Add(new TKGD3.Curves.CurveList());


            TSX.List<ItemMonikerList> listItemMok = new TSX.List<ItemMonikerList>();
            listItemMok.Add(new ItemMonikerList());
            int i = 0;

            List<int> indices = new List<int>();



            int counter = 0;
            foreach (var loop in inBRep.Faces.ElementAt(faceindex).Loops)
            {
                foreach (var trim in loop.Trims)
                {

                    Curve spCurve = inBRep.Curve3D.ElementAt(trim.Edge.Curve3dIndex) as Curve; //TODO check a more general way to cast ICurve to Curve even for lines

                    if (spCurve != null)
                    {
                        var convertedCrv = CurveToNative(spCurve);
                        listItemMok.First().Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, i++));
                        loops3d.First().Add(convertedCrv);
                    }

                    else
                    {
                        var spLineCrv = inBRep.Curve3D.ElementAt(trim.Edge.Curve3dIndex) as Line;
                        if (spLineCrv != null)
                        {
                            var convertedCrv = LineToNative(spLineCrv);
                            listItemMok.First().Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, i++));
                            loops3d.First().Add(convertedCrv);
                        }
                    }

                    var spCurve2 = inBRep.Curve3D.ElementAt(trim.Edge.Curve3dIndex);

                    counter++;

                }
            }



            //foreach (int ind in indices)
            //{
            //    var spCurve = inBRep.Edges.ElementAt(ind).Curve as Curve;

            //    var convertedCrv = CurveToNative(spCurve);

            //    //if (counter != 0 && convertedCrv.Ps.DistanceTo(inBRep.Edges.ElementAt(indices[counter - 1]).PointAtEnd) > inLinearPrecision)
            //    //{
            //    //    spCurve.Reverse();
            //    //}

            //    listItemMok.First().Add(new ItemMoniker(false, (byte)ItemType.SketchSegment, key, i++));
            //    loops3d.First().Add(convertedCrv);
            //    counter++;

            //}


            if (loops3d != null && loops3d.Count != 0)
            {
                // if (inFace.rev == false || ImporterHelper.MakeReversed(loops3d)) // Useless
                {
                    sheetMaker.SetCurves(loops3d, listItemMok);

                    //AHW setting to true causes an error
                    //sheetMaker.UsesBRepMethod = true;
                    //var x = new ItemMoniker(new CString($"S{op2.Id}"));
                    try
                    {
                        shape = sheetMaker.Make(null, ItemOperationKey.BasicKey);
                    }
                    catch
                    {

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


            var speckleMesh = new Mesh(verts.ToArray(), faces.ToArray(), units: u);

            return speckleMesh;
        }
        #endregion
    }
}


