using System;
using System.Diagnostics;
using System.Resources;
using System.Collections;
using TK = TopSolid.Kernel;

namespace Speckle.ConnectorTopSolid.UI
{
	/// <summary>
	/// Manages the session.
	/// </summary>
	public static class Session
	{
		static Session()
		{
			//Important to be in static constructor, I don't know why...
			Resolver.Initialize();

		}

		/// <summary>
		/// Starts the session.
		/// </summary>
		public static void Start()
		{
		}

		/// <summary>
		/// End the session.
		/// </summary>
		public static void End()
		{
		}


	}
}
