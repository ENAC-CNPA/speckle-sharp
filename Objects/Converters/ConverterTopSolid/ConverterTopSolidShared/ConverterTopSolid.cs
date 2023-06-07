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


using D3Box = TopSolid.Kernel.G.D3.Box;
using D3Plane = TopSolid.Kernel.G.D3.Plane;
using D3Point = TopSolid.Kernel.G.D3.Point;
using D3Vector = TopSolid.Kernel.G.D3.Vector;
using D3UVector = TopSolid.Kernel.G.D3.UnitVector;
using TsEntity = TopSolid.Kernel.DB.Entities.Entity;
using D3LineCurve = TopSolid.Kernel.G.D3.Curves.LineCurve;
using D3SplineCurve = TopSolid.Kernel.G.D3.Curves.BSplineCurve;
using D3PolylineCurve = TopSolid.Kernel.G.D3.Curves.PolylineCurve;
using D3Geometry = TopSolid.Kernel.G.IGeometry;
using D3BSplineSurface = TopSolid.Kernel.G.D3.Surfaces.BSplineSurface;
using D3Shape = TopSolid.Kernel.G.D3.Shapes.Shape;



using TopSolid.Kernel.G.D3.Shapes;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.G.D3.Shapes.Polyhedrons;
using TopSolid.Cad.Design.DB;
using TopSolid.Kernel;
using G = TopSolid.Kernel.G;
using DB = TopSolid.Kernel.DB;
using TopSolid.Kernel.G.D3.Sketches;
using TopSolid.Kernel.DB.D3.Modeling.Documents;

using Application = TopSolid.Kernel.UI.Application;
using TopSolid.Kernel.G.D3.Sketches.Planar;
using TopSolid.Kernel.DB.Operations;

using Speckle.ConnectorTopSolid.DB.Operations;
using Speckle.Core.Api;
using TopSolid.Kernel.DB.Scheduling;
using DesktopUI2.Models;

namespace Objects.Converter.TopSolid
{
    public partial class ConverterTopSolid : ISpeckleConverter
    {
#if TOPSOLID715
        public static string TopSolidAppName = HostApplications.TopSolid.GetVersion(HostAppVersion.v715);
#else
        public static string TopSolidAppName = HostApplications.TopSolid.GetVersion(HostAppVersion.v716);
#endif

        public SpeckleFolderOperation sfo = null;

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

        public void SetConverterSettings(object settings)
        {
            Settings = settings as Dictionary<string, string>;
        }
        public void SetPreviousContextObjects(List<ApplicationObject> objects) => throw new NotImplementedException();

    public void SetContextDocument(Object doc)
    {

      // init is necessary for TopSolid
      DB.Documents.Document sDoc = Doc;
      if (sDoc != null)
      {
        //sDoc.Elements[Convert.ToInt32(10000)];
        //var op = sDoc.RootOperation.SearchDeepOperation(typeof(FolderOperation));

        if (sfo == null)
        {
          //Speckle.ConnectorTopSolid.DB.Operations.SpeckleFolderOperation.
          Settings.TryGetValue("stream-name", out string streamName);

          var op = sDoc.RootOperation.DeepConstituents.Where(x => x.Name == streamName).FirstOrDefault();
          if (op != null)
          {
            sfo = op as SpeckleFolderOperation;
            Console.WriteLine(sfo.Name);
          }
          else
          {
            SpeckleFolderOperation scor = new SpeckleFolderOperation(sDoc, 0)
            {
              Name = streamName
            };
            sDoc.EnsureIsDirty();
            scor.Create();
            sfo = scor;
          }
        }

      }


    }

    public Element CurrentHostElement { get; set; }

        public Base ConvertToSpeckle(object @object)
        {
            switch (@object)
            {
                case D3Box o:
                    return BoxToSpeckle(o);

                case D3Plane o:
                    return PlaneToSpeckle(o);

                case D3Point o:
                    return PointToSpeckle(o);

                case D3Vector o:
                    return VectorToSpeckle(o);

                case D3LineCurve o:
                    return LineToSpeckle(o);

                case D3PolylineCurve o:
                    return PolyLineToSpeckle(o);

                case D3BSplineSurface o:
                    return SurfaceToSpeckle(o);

                case D3Shape o:
                    return BrepToSpeckle(o);

                case G.D3.Sketches.Planar.PlanarSketch o:
                    return PlanarSketchToSpeckle(o);

                case G.D3.Sketches.PositionedSketch o:
                    return PositionedSketchToSpeckle(o);

                case DB.D3.Sketches.PositionedSketchEntity o:
                    return PositionedSketchToSpeckle(o.Geometry as PositionedSketch);

                case DB.D3.Sketches.Planar.PlanarSketchEntity o:
                    return PlanarSketchToSpeckle(o.Geometry as PlanarSketch);

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
                    return EntityBrepToNative(o); // TODO
                    //return BrepToNative(o); // TODO

                default:
                    throw new NotSupportedException();
            }
        }

        public List<object> ConvertToNative(List<Base> objects)
        {
            return objects.Select(x => ConvertToNative(x)).ToList();
        }

        /// <summary>
        /// Converts a TopSolid Entity <see cref="D3Geometry"/> instance to a Speckle <see cref="Base"/>
        /// </summary>
        /// <param name="obj">TopSolid Entity to be converted.</param>
        /// <returns></returns>
        public Base ObjectToSpeckle(D3Geometry obj)
        {
            switch (obj)
            {

                case D3Box o:
                    return BoxToSpeckle(o);

                case D3Plane o:
                    return PlaneToSpeckle(o);

                case D3Point o:
                    return PointToSpeckle(o);

                case G.D2.Point o:
                    return PointToSpeckle(o);

                case D3LineCurve o:
                    return LineToSpeckle(o);

                case D3PolylineCurve o:
                    return PolyLineToSpeckle(o);

                case D3BSplineSurface o:
                    return SurfaceToSpeckle(o);

                case D3Shape o:
                    return BrepToSpeckle(o);

                case Polyhedron o:
                    return PolyhedronToSpeckle(o);

                case G.D2.Curves.GeometricProfile o:
                    return ProfileToSpeckle(o);

                case G.D3.Curves.GeometricProfile o:
                    return ProfileToSpeckle(o);

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
                        case D3Box _:
                        case D3Plane _:
                        case D3Point _:
                        case D3LineCurve _:
                        case D3PolylineCurve _:
                        case D3BSplineSurface _:
                        case D3Shape _:
                            return true;

                        default:
                            return false;
                    }
                case Element e:
                    switch (e.Geometry)
                    {
                        // TODO : Polyhedrons
                        case D3Box _:
                        case D3Plane _:
                        case D3Point _:
                        case D3LineCurve _:
                        case D3PolylineCurve _:
                        case D3BSplineSurface _:
                        case D3Shape _:
                        case Polyhedron _:
                        case Sketch _:
                        case G.D2.Sketches.Sketch _:

                            return true;

                        default:
                            return false;
                    }
                case D3Geometry o:
                    switch (o)
                    {
                        case D3Box _:
                        case D3Plane _:
                        case D3Point _:
                        case D3LineCurve _:
                        case D3PolylineCurve _:
                        case D3BSplineSurface _:
                        case D3Shape _:
                        case Polyhedron _:
                        case G.D3.Sketches.Planar.PlanarSketch _:
                            return true;

                        default:
                            return false;
                    }

                case D3Vector _:
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
    }


    public class Alias
    {
        public List<GeometryAlias> Faces { get; set; } 
        public List<GeometryAlias> Edges { get; set; } 
        public List<GeometryAliasLinked> Vertices { get; set; } 
    }
    public class GeometryAlias
    {
        public double Index { get; set; }
        public string Moniker { get; set; }
    }
    public class GeometryAliasLinked
    {
        public double Index { get; set; }
        public string Moniker { get; set; }
        public string Hash { get; set; }
        //public List<int> EdgeIndex { get; set; }
        //public string FacesIndex { get; set; }
    }
}
