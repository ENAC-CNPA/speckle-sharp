using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.D3.Modeling.Documents;
using Application = TopSolid.Kernel.UI.Application;
using DB = TopSolid.Kernel.DB;

using Speckle.Core.Models;
using Objects.BuiltElements;


namespace Objects.Converter.TopSolid
{
    public partial class ConverterTopSolid
    {

        //public static ModelingDocument Doc => Application.CurrentDocument as ModelingDocument;

        // Elements
        #region Elements
        public Base ElementToSpeckle(Element topSolidElement, string units = null)
        {
            var u = units ?? ModelUnits;
            Base speckleElement = new Base();

            //speckleElement["renderMaterial"] = new Other.RenderMaterial() { opacity = 0.2, diffuse = System.Drawing.Color.AliceBlue.ToArgb() };


            SetInstanceParameters(speckleElement, topSolidElement);
            GetHostedElements(speckleElement, topSolidElement);

            Console.WriteLine(speckleElement);
            return speckleElement;
        }
        public List<ApplicationObject> ElementToNative(Base speckleElement)
        {
            Element topSolidElement = Doc.Elements[Convert.ToInt32(speckleElement.applicationId)];
            if (topSolidElement != null && ReceiveMode == Speckle.Core.Kits.ReceiveMode.Ignore)
                return new List<ApplicationObject>(); // { new ApplicationPlaceholderObject { applicationId = speckleElement.applicationId, ApplicationGeneratedId = topSolidElement.Id.ToString(), NativeObject = topSolidElement } }; ;

            bool isUpdate = true;
            if (topSolidElement == null) // TODO : Create element
            {

            }
            if (topSolidElement == null) // Check if created
            {
                throw new Speckle.Core.Logging.SpeckleException($"Failed to create Entity ${speckleElement.applicationId}.");
            }

            var placeholders = new List<ApplicationObject>();

            //{
            //  new ApplicationPlaceholderObject
            //  {
            //  applicationId = speckleElement.applicationId,
            //  ApplicationGeneratedId = topSolidElement.Id.ToString(),
            //  NativeObject = topSolidElement
            //  }
            //};

            var hostedElements = SetHostedElements(speckleElement, topSolidElement);
            placeholders.AddRange(hostedElements);


            Report.Log($"{(isUpdate ? "Updated" : "Created")} Entity {topSolidElement.Id}");

            return placeholders;
        }

        #endregion



        // SketchEntity
        #region SketchEntity
        public Base D3SketchEntityToSpeckle(DB.D3.Sketches.SketchEntity topSolidElement, string units = null)
        {
            var u = units ?? ModelUnits;
            Base speckleElement = new Base();

            //speckleElement["renderMaterial"] = new Other.RenderMaterial() { opacity = 0.2, diffuse = System.Drawing.Color.AliceBlue.ToArgb() };


            //SetInstanceParameters(speckleElement, topSolidElement);
            GetHostedElements(speckleElement, topSolidElement);

            Console.WriteLine(speckleElement);
            return speckleElement;
        }
        public List<ApplicationObject> D3SketchEntityToNative(Base speckleElement)
        {
            Element topSolidElement = Doc.Elements[Convert.ToInt32(speckleElement.applicationId)];
            if (topSolidElement != null && ReceiveMode == Speckle.Core.Kits.ReceiveMode.Ignore)
                return new List<ApplicationObject>(); // { new ApplicationPlaceholderObject { applicationId = speckleElement.applicationId, ApplicationGeneratedId = topSolidElement.Id.ToString(), NativeObject = topSolidElement } }; ;

            bool isUpdate = true;
            if (topSolidElement == null) // TODO : Create element
            {

            }
            if (topSolidElement == null) // Check if created
            {
                throw new Speckle.Core.Logging.SpeckleException($"Failed to create Entity ${speckleElement.applicationId}.");
            }

            var placeholders = new List<ApplicationObject>();

            //{
            //  new ApplicationPlaceholderObject
            //  {
            //  applicationId = speckleElement.applicationId,
            //  ApplicationGeneratedId = topSolidElement.Id.ToString(),
            //  NativeObject = topSolidElement
            //  }
            //};

            var hostedElements = SetHostedElements(speckleElement, topSolidElement);
            placeholders.AddRange(hostedElements);


            Report.Log($"{(isUpdate ? "Updated" : "Created")} Entity {topSolidElement.Id}");

            return placeholders;
        }
        public Base D2SketchEntityToSpeckle(DB.D2.Sketches.SketchEntity topSolidSketchEntity, string units = null)
        {
            var u = units ?? ModelUnits;
            Base speckleSketchEntity = new Base();

            speckleSketchEntity["renderMaterial"] = new Other.RenderMaterial() { opacity = 1 - topSolidSketchEntity.ExplicitTransparency.ToFloat(), diffuse = topSolidSketchEntity.ExplicitColor.Argb.ToArgb() };

            //SetInstanceParameters(speckleElement, topSolidElement);
            GetHostedElements(speckleSketchEntity, topSolidSketchEntity);


            Console.WriteLine(speckleSketchEntity);
            return speckleSketchEntity;
        }
        public List<ApplicationObject> D2SketchEntityToNative(Base speckleElement)
        {
            Element topSolidElement = Doc.Elements[Convert.ToInt32(speckleElement.applicationId)];
            if (topSolidElement != null && ReceiveMode == Speckle.Core.Kits.ReceiveMode.Ignore)
                return new List<ApplicationObject>(); // { new ApplicationPlaceholderObject { applicationId = speckleElement.applicationId, ApplicationGeneratedId = topSolidElement.Id.ToString(), NativeObject = topSolidElement } }; ;

            bool isUpdate = true;
            if (topSolidElement == null) // TODO : Create element
            {

            }
            if (topSolidElement == null) // Check if created
            {
                throw new Speckle.Core.Logging.SpeckleException($"Failed to create Entity ${speckleElement.applicationId}.");
            }


            var placeholders = new List<ApplicationObject>();

            //{
            //  new ApplicationPlaceholderObject
            //  {
            //  applicationId = speckleElement.applicationId,
            //  ApplicationGeneratedId = topSolidElement.Id.ToString(),
            //  NativeObject = topSolidElement
            //  }
            //};

            var hostedElements = SetHostedElements(speckleElement, topSolidElement);
            placeholders.AddRange(hostedElements);


            Report.Log($"{(isUpdate ? "Updated" : "Created")} Entity {topSolidElement.Id}");

            return placeholders;
        }

        #endregion



    }
}
