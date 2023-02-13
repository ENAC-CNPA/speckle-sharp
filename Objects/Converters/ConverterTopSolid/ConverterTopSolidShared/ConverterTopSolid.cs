using System;
using System.Collections.Generic;
using System.Linq;

using Speckle.Core.Kits;
using Speckle.Core.Models;
using Arc = Objects.Geometry.Arc;
using BlockInstance = Objects.Other.BlockInstance;
using BlockDefinition = Objects.Other.BlockDefinition;
using Brep = Objects.Geometry.Brep;
using Circle = Objects.Geometry.Circle;
using Curve = Objects.Geometry.Curve;
using Ellipse = Objects.Geometry.Ellipse;
using Interval = Objects.Primitive.Interval;
using Line = Objects.Geometry.Line;
using Mesh = Objects.Geometry.Mesh;
using Plane = Objects.Geometry.Plane;
using Point = Objects.Geometry.Point;
using Polycurve = Objects.Geometry.Polycurve;
using Polyline = Objects.Geometry.Polyline;
using Surface = Objects.Geometry.Surface;
using Vector = Objects.Geometry.Vector;
using Box = Objects.Geometry.Box;


using TsBox = TopSolid.Kernel.G.D3.Box;
using TsPlane = TopSolid.Kernel.G.D3.Plane;
using TsPoint = TopSolid.Kernel.G.D3.Point;
using TsVector = TopSolid.Kernel.G.D3.Vector;
using TsUVector = TopSolid.Kernel.G.D3.UnitVector;
using TsEntity = TopSolid.Kernel.DB.Entities.Entity;
using TsLineCurve = TopSolid.Kernel.G.D3.Curves.LineCurve;
using TsSplineCurve = TopSolid.Kernel.G.D3.Curves.BSplineCurve;
using TsPolylineCurve = TopSolid.Kernel.G.D3.Curves.PolylineCurve;
using TsGeometry = TopSolid.Kernel.G.IGeometry;
using TsBSplineSurface = TopSolid.Kernel.G.D3.Surfaces.BSplineSurface;
using TopSolid.Kernel.DB.D3.Curves;
using TsShape = TopSolid.Kernel.G.D3.Shapes.Shape;
using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.G.D3.Shapes.Polyhedrons;
using TopSolid.Cad.Design.DB;
using G = TopSolid.Kernel.G;
using DB = TopSolid.Kernel.DB;
using TopSolid.Kernel.G.D3.Sketches;
using TopSolid.Kernel.DB.D3.Modeling.Documents;

using Application = TopSolid.Kernel.UI.Application;


namespace Objects.Converter.TopSolid
{
    public partial class ConverterTopSolid : ISpeckleConverter
    {
#if TOPSOLID715
        public static string TopSolidAppName = HostApplications.TopSolid.GetVersion(HostAppVersion.v715);
#else
        public static string TopSolidAppName = HostApplications.TopSolid.GetVersion(HostAppVersion.v716);
#endif
       
        public ConverterTopSolid()
        {
            var ver = System.Reflection.Assembly.GetAssembly(typeof(ConverterTopSolid)).GetName().Version;
        }

        #region ISpeckleConverter props
        public string Description => "Default Speckle Kit for TopSolid";
        public string Name => nameof(ConverterTopSolid);
        public string Author => "Speckle";
        public string WebsiteOrEmail => "https://speckle.systems";
        public ProgressReport Report { get; private set; } = new ProgressReport();
        public IEnumerable<string> GetServicedApplications() => new string[] { TopSolidAppName };
        public ModelingDocument Doc => Application.CurrentDocument as ModelingDocument;
        public Dictionary<string, string> Settings { get; private set; } = new Dictionary<string, string>();
   
        #endregion ISpeckleConverter props

        public ReceiveMode ReceiveMode { get; set; }

        public List<ApplicationObject> ContextObjects { get; set; } = new List<ApplicationObject>();

        public List<int> ConvertedObjectsList { get; set; } = new List<int>();

        public void SetContextObjects(List<ApplicationObject> objects) => ContextObjects = objects;

        public void SetPreviousContextObjects(List<ApplicationObject> objects) => throw new NotImplementedException();

        public void SetContextDocument(object doc)
        {
            // TODO: if documnent init is necessary for TopSolid
        }

        public Element CurrentHostElement { get; set; }

        public Base ConvertToSpeckle(object @object)
        {
            switch (@object)
            {
                case TsBox o:
                    return BoxToSpeckle(o);

                case TsPlane o:
                    return PlaneToSpeckle(o);

                case TsPoint o:
                    return PointToSpeckle(o);

                case TsVector o:
                    return VectorToSpeckle(o);

                case TsLineCurve o:
                    return LineToSpeckle(o);

                case TsPolylineCurve o:
                    return PolyLineToSpeckle(o);

                case TsBSplineSurface o:
                    return SurfaceToSpeckle(o);

                case TsShape o:
                    return BrepToSpeckle(o);

                case G.D3.Sketches.Sketch o:
                    return D3SketchToSpeckle(o);

                case DB.D3.Sketches.SketchEntity o:
                    return D3SketchEntityToSpeckle(o);

                case DB.D2.Sketches.SketchEntity o:
                    return D2SketchEntityToSpeckle(o);

                case Element o:
                    return ElementToSpeckle(o);

                default:
                    throw new NotSupportedException();
            }
        }

        public List<Base> ConvertToSpeckle(List<object> objects) => objects.Select(ConvertToSpeckle).ToList();


        private Base ObjectToSpeckleBuiltElement(TsEntity o)
        {
            throw new NotImplementedException();
        }

        public object ConvertToNative(Base @object)
        {
            switch (@object)
            {
                case Box o:
                    return BoxToNative(o);

                case Plane o:
                    return PlaneToNative(o);

                case Point o:
                    return PointToNative(o);

                case Vector o:
                    return UnitVectorToNative(o);

                case Polyline o:
                    return PolyLineToNative(o);

                case Surface o:
                    return SurfaceToNative(o);

                case Brep o:
                    return BrepToNative(o); // TODO

                default:
                    throw new NotSupportedException();
            }
        }

        public List<object> ConvertToNative(List<Base> objects)
        {
            return objects.Select(x => ConvertToNative(x)).ToList();
        }

        /// <summary>
        /// Converts a TopSolid Entity <see cref="TsGeometry"/> instance to a Speckle <see cref="Base"/>
        /// </summary>
        /// <param name="obj">TopSolid Entity to be converted.</param>
        /// <returns></returns>
        public Base ObjectToSpeckle(TsGeometry obj)
        {
            switch (obj)
            {

                case TsBox o:
                    return BoxToSpeckle(o);

                case TsPlane o:
                    return PlaneToSpeckle(o);

                case TsPoint o:
                    return PointToSpeckle(o);

                case TsLineCurve o:
                    return LineToSpeckle(o);

                case TsPolylineCurve o:
                    return PolyLineToSpeckle(o);

                case G.D3.Curves.GeometricProfile o:
                    return ProfileToSpeckle(o);

                case TsBSplineSurface o:
                    return SurfaceToSpeckle(o);

                case TsShape o:
                    return BrepToSpeckle(o);

                case Polyhedron o:
                    return PolyhedronToSpeckle(o);

                case G.D3.Sketches.Planar.PlanarSketch o:
                    return PlanarSketchToSpeckle(o);

                case G.D3.Sketches.PositionedSketch o:
                    return PositionedSketchToSpeckle(o);

                // TODO: using multi type (TsGeometry isn't compatible)
                //case TsVector o:
                //    return VectorToSpeckle(o);

                default:
                    return null;
            }
        }

        public bool CanConvertToSpeckle(object @object)
        {
            switch (@object)
            {
                case PartEntity p:
                    switch (p.Geometry)
                    {
                        case TsBox _:
                        case TsPlane _:
                        case TsPoint _:
                        case TsLineCurve _:
                        case TsPolylineCurve _:
                        case TsBSplineSurface _:
                        case TsShape _:
                            return true;

                        default:
                            return false;
                    }
                case Element e:
                    switch (e.Geometry)
                    {
                        // TODO : Polyhedrons
                        case TsBox _:
                        case TsPlane _:
                        case TsPoint _:
                        case TsLineCurve _:
                        case TsPolylineCurve _:
                        case TsBSplineSurface _:
                        case TsShape _:
                        case Polyhedron _:
                        case Sketch _:
                        case G.D2.Sketches.Sketch _:

                            return true;

                        default:
                            return false;
                    }
                case TsGeometry o:
                    switch (o)
                    {
                        case TsBox _:
                        case TsPlane _:
                        case TsPoint _:
                        case TsLineCurve _:
                        case TsPolylineCurve _:
                        case TsBSplineSurface _:
                        case TsShape _:
                        case Polyhedron _:
                        case G.D3.Sketches.Planar.PlanarSketch _:
                            return true;

                        default:
                            return false;
                    }

                case TsVector _:
                    return true;

                default:
                    return false;
            }
        }

        public bool CanConvertToNative(Base @object)
        {
            switch (@object)
            {
                case Box _:
                case Plane _:
                case Point _:
                case Vector _:
                case Line _:
                case Polyline _:
                case Surface _:
                case Brep _:
                    return true;

                default:
                    return false;
            }
        }

        public void SetConverterSettings(object settings)
        {
            Settings = settings as Dictionary<string, string>;
        }
    }
}
