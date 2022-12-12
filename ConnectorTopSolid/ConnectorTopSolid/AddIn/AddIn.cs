using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TK = TopSolid.Kernel;

namespace Speckle.ConnectorTopSolid.AddIn
{
	/// <summary>
	/// Represents the TopSolid add-in.
	/// </summary>
	[Guid("C534F186-FC35-4B91-8B91-ECF71942011F")]
    public class AddIn : TopSolid.Kernel.TX.AddIns.AddIn
	{
		// Properties:

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.Name"/>
		/// </summary>
		public override string Name
		{
			get { return Resources.Manager.GetString("$Name"); }
		}

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.Description"/>
		/// </summary>
		public override string[] Description
		{
			get
			{
				string[] description = new string[1];
				description[0] = Resources.Manager.GetString("$Description");
				return description;
			}
		}

		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.Manufacturer"/>
		/// </summary>
		public override string Manufacturer
		{
			get { return Resources.Manager.GetString("$Manufacturer"); }
		}

		/// <summary>
		/// Overrides <see cref="TopSolid.Kernel.TX.AddIns.AddIn.RequiredAddIns"/>.
		/// </summary>
		public override Guid[] RequiredAddIns
		{
			get
			{
				return new Guid[0];
			}
		}

		// Methods:

		/// <summary>
		/// Initialize the context menus
		/// </summary>
		public override void InitializeSession()
		{
            //Add the UI menus
            ConnectorTopSolid.UI.ContextMenu.AddMenu();
		}

		/// <summary>
		/// Start the needed sessions
		/// </summary>
		public override void StartSession()
		{
			//Start the UI session
			TK.SX.SessionManager.Start(typeof(ConnectorTopSolid.UI.Session));
		}

		/// <summary>
		/// See <see cref="TopSolid.Kernel.TX.AddIns.AddIn.EndSession"/>.
		/// </summary>
		public override void EndSession()
		{
		}


		/// <summary>
		/// Overrides <see cref="TK.TX.AddIns.AddIn.GetRegistrationCertificate"/>.
		/// </summary>
		public override string GetRegistrationCertificate()
		{
			return TK.SX.String.ReadResourceTextFile(typeof(AddIn), "Speckle.ConnectorTopSolid.AddIn.xml");

		}
	}
}
