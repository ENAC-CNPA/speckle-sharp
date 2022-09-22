using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using Application = TopSolid.Kernel.UI.Application;

using Speckle.Core.Models;
using Objects.BuiltElements;


namespace Objects.Converter.TopSolid
{
    public partial class ConverterTopSolid
    {

        public static ModelingDocument Doc => Application.CurrentDocument as ModelingDocument;

        // Elements
        #region Elements
        public Base ElementToSpeckle(Element topSolidElement, string units = null)
        {
            var u = units ?? ModelUnits;
           Base speckleElement = new Base();

            speckleElement["renderMaterial"] = new Other.RenderMaterial() { opacity = 0.2, diffuse = System.Drawing.Color.AliceBlue.ToArgb() };
     

            SetInstanceParameters(speckleElement, topSolidElement);
            GetHostedElements(speckleElement, topSolidElement);

            Console.WriteLine(speckleElement);
            return speckleElement;
        }
        public List<ApplicationPlaceholderObject> ElementToNative(Base speckleElement)
        {
            Element topSolidElement = Doc.Elements[Convert.ToInt32(speckleElement.applicationId)];
            if (topSolidElement != null && ReceiveMode == Speckle.Core.Kits.ReceiveMode.Ignore)
                return new List<ApplicationPlaceholderObject> { new ApplicationPlaceholderObject { applicationId = speckleElement.applicationId, ApplicationGeneratedId = topSolidElement.Id.ToString(), NativeObject = topSolidElement } }; ;

            bool isUpdate = true;
            if (topSolidElement == null) // Create element
            {

            }
            if (topSolidElement == null) // Check if created
            {
                throw new Speckle.Core.Logging.SpeckleException($"Failed to create wall ${speckleElement.applicationId}.");
            }

            var placeholders = new List<ApplicationPlaceholderObject>()
              {
                new ApplicationPlaceholderObject
                {
                applicationId = speckleElement.applicationId,
                ApplicationGeneratedId = topSolidElement.Id.ToString(),
                NativeObject = topSolidElement
                }
              };

            var hostedElements = SetHostedElements(speckleElement, topSolidElement);
            placeholders.AddRange(hostedElements);


            Report.Log($"{(isUpdate ? "Updated" : "Created")} Wall {topSolidElement.Id}");

            return placeholders;
        }

        #endregion


        //public Surface SurfaceToSpeckle(TsSurface surfaceEntity, string units = null)
        //{
        //    var u = units ?? ModelUnits;
        //    TsBSplineSurface surface = surfaceEntity.Geometry.GetBsplineGeometry(Precision.LinearPrecision, false, false, false);
        //    Surface _surface = new Geometry.Surface
        //    {
        //        degreeU = surface.UDegree,
        //        degreeV = surface.VDegree,
        //        rational = surface.IsRational,
        //        closedU = surface.IsUClosed,
        //        closedV = surface.IsVClosed,
        //        domainU = new Interval(surface.Us, surface.Ue),
        //        domainV = new Interval(surface.Vs, surface.Ve),
        //        knotsU = GetCorrectKnots(surface.UBs.ToList(), surface.UCptsCount, surface.UDegree),
        //        knotsV = GetCorrectKnots(surface.VBs.ToList(), surface.VCptsCount, surface.VDegree)
        //    };

        //    _surface.SetControlElements(ControlElementsToSpeckle(surface));
        //    _surface.units = u;

        //    // TODO: Make Shape Display
        //    //FacetedShapeMaker.MakeShape(surfaceEntity, surfaceEntity.LevelKey, surfaceEntity.Geometry);

        //    return _surface;
        //}


    }
}
