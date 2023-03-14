using TopSolid.Kernel.DB.Parameters;
using System.Runtime.InteropServices;
using TopSolid.Cad.Design.DB;
using TopSolid.Cad.Design.DB.Parts;
using TopSolid.Kernel.DB.D3.Directions;
using TopSolid.Kernel.DB.D3.Planes;
using TopSolid.Kernel.DB.D3.Sections;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.D3.Shapes.Bosses;
using TopSolid.Kernel.DB.D3.Shapes.Pockets;
using TopSolid.Kernel.DB.Documents;
using TopSolid.Kernel.DB.Elements;
using TopSolid.Kernel.DB.Operations;
using TopSolid.Kernel.DB.SmartObjects;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.G.D3.Shapes.Extruded;
using TopSolid.Kernel.TX.Units;
using TopSolid.Kernel.SX.Collections.Generic;
using Extent = TopSolid.Kernel.G.D2.Extent;

namespace Speckle.ConnectorTopSolid.DB.Operations
{
    public sealed class SpeckleCompositeOperationSend : SpeckleCompositeOperation
    {
       
        /// <summary>
        /// The pocket operation
        /// </summary>
        //private PocketOperation pocketOperation;

        // Constructors:

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeckleCompositeOperation"/> class.
        /// </summary>
        /// <param name="inDocument">Container document (referenced).</param>
        /// <param name="inId">Element identifier, or zero for automatic.</param>
        public SpeckleCompositeOperationSend(ref TopSolid.Kernel.DB.Documents.Document inDocument, int inId): base(ref inDocument, inId)
        {
            return;
        }

     }
}
