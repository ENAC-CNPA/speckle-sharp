
using TopSolid.Kernel.UI.Commands;
using Speckle.ConnectorTopSolid.UI.CustomWindows;



namespace Speckle.ConnectorTopSolid.UI.LaunchCommand
{
    class LaunchSpeckleCommand : MenuCommand
    {

        private static SpeckleWindow newSpeckleWindow;
        protected override void Invoke()
        {

            //SpeckleCommand();

            base.Invoke();
            if (newSpeckleWindow == null)
            {
                newSpeckleWindow = new SpeckleWindow();
                newSpeckleWindow.AddOrModifyDockedWindow();
            }
            else
            {
                if (newSpeckleWindow.DockedContent != null)
                {
                    newSpeckleWindow.DockedContent.Visible = true;
                }
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
