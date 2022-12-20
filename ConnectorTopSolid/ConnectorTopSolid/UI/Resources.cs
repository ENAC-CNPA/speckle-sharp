using System;
using System.Drawing;
using System.Reflection;
using TopSolid.Kernel.SX.Resources;

namespace Speckle.ConnectorTopSolid.UI
{
    /// <summary>
    /// Manages the resources.
    /// </summary>
    [Obfuscation(Exclude = true)]
	public static class Resources
	{
		// Static fields:

		/// <summary>
		/// Resources manager.
		/// </summary>
		private static ResourceManager manager = null;

		// Properties:

		/// <summary>
		/// Gets the resources manager.
		/// </summary>
		public static ResourceManager Manager
		{
			get
			{
				if (manager == null)
				{
					manager = new ResourceManager(typeof(Resources));
				}

				return manager;
			}
		}

        /// <summary>
        /// Gets the folder icon.
        /// </summary>
        /// <param name="inDesiredSize">Desired size.</param>
        public static Icon GetFolderIcon(Size inDesiredSize)
        {
            return TopSolid.Kernel.SX.ResourceDictionary.GetIconWithExtension(typeof(Resources), "SpeckleWindow.ico", inDesiredSize);
        }
    }
}
