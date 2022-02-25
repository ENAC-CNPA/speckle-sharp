using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using Arc = Objects.Geometry.Arc;
using Box = Objects.Geometry.Box;
using BlockInstance = Objects.Other.BlockInstance;
using BlockDefinition = Objects.Other.BlockDefinition;
using Brep = Objects.Geometry.Brep;
using BrepEdge = Objects.Geometry.BrepEdge;
using BrepFace = Objects.Geometry.BrepFace;
using BrepLoop = Objects.Geometry.BrepLoop;
using BrepLoopType = Objects.Geometry.BrepLoopType;
using BrepTrim = Objects.Geometry.BrepTrim;
using Circle = Objects.Geometry.Circle;
using ControlPoint = Objects.Geometry.ControlPoint;
using Curve = Objects.Geometry.Curve;
using Plane = Objects.Geometry.Plane;
using Ellipse = Objects.Geometry.Ellipse;
using Interval = Objects.Primitive.Interval;
using Line = Objects.Geometry.Line;
using Mesh = Objects.Geometry.Mesh;
using Surface = Objects.Geometry.Surface;
using Point = Objects.Geometry.Point;
using Polycurve = Objects.Geometry.Polycurve;
using Polyline = Objects.Geometry.Polyline;
using Vector = Objects.Geometry.Vector;
using Speckle.Core.Models;
using Speckle.Core.Kits;

using dbBox = TopSolid.Kernel.DB.D3.Boxes.BoxEntity;
using dbPlane = TopSolid.Kernel.DB.D3.Planes.PlaneEntity;
using dbPoint = TopSolid.Kernel.DB.D3.Points.PointEntity;
using dbVector = TopSolid.Kernel.DB.D3.Axes.AxisEntity;
using TsSurface = TopSolid.Kernel.DB.D3.Surfaces.SurfaceEntity;
using TsBSplineSurface = TopSolid.Kernel.G.D3.Surfaces.BSplineSurface;
using TopSolid.Kernel.G;
using TopSolid.Kernel.G.D3.Surfaces;
using TopSolid.Kernel.G.D3.Shapes.FacetShapes;
using TopSolid.Kernel.G.D3.Shapes;

namespace Objects.Converter.TopSolid
{
    public partial class ConverterTopSolid
    {

        public Surface SurfaceToSpeckle(TsSurface surfaceEntity, string units = null)
        {
            var u = units ?? ModelUnits;
            TsBSplineSurface surface = surfaceEntity.Geometry.GetBsplineGeometry(Precision.LinearPrecision, false, false, false);
            Surface _surface = new Geometry.Surface
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

            // TODO: Make Shape Display
            //FacetedShapeMaker.MakeShape(surfaceEntity, surfaceEntity.LevelKey, surfaceEntity.Geometry);

            return _surface;
        }


    }
}
