using System;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Documents;
using TopSolid.Kernel.DB.D3.Planes;
using TopSolid.Kernel.DB.Documents;
using TopSolid.Kernel.DB.Parameters;
using TopSolid.Kernel.G.D3;
using TopSolid.Kernel.GR.Displays;
using TopSolid.Kernel.TX.Units;
using TopSolid.Kernel.UI.Commands;
using TopSolid.Kernel.WX;


namespace Speckle.ConnectorTopSolid.UI.LaunchCommand
{
    class LaunchSpeckleCommand : MenuCommand
    {
 
        protected override void Invoke()
        {
          
            SpeckleCommand();
        }
       
        /// <summary>
        /// Main command to initialize Speckle Connector
        /// </summary>
        public static void SpeckleCommand()

        {
            Speckle.ConnectorTopSolid.UI.Entry.SpeckleTopSolidCommand.SpeckleCommand(); //.SendCommand();
            
        }
        
    }

    class LaunchSpeckleInfoCommand : MenuCommand
    {
 
        protected override void Invoke()
        {
            SpeckleCommand(); // TODO : Create new info panel
        }

        protected override bool CanInvoke
        {
            get {
                if (!base.CanInvoke) return false;
                return this.CurrentDocument is DesignDocument; // PartDocument / AssemblyDocument ...
            }
        }

        /// <summary>
        /// Main command to initialize Speckle Connector
        /// </summary>
        public static void SpeckleCommand()
        {
            Speckle.ConnectorTopSolid.UI.Entry.SpeckleTopSolidCommand.SpeckleCommand(); //.SendCommand();
            
        }

    }
}
