using Postage.Core.Engine;
using Postage.Net;

namespace Postage;

/// <summary>
/// Context container
/// </summary>
public class ContextHost
{
	public LauncherLoadContext Client { get; }
	public LauncherLoadContext Server { get; }
	public LauncherLoadContext Menu { get; }

	public ContextHost( Source2Instance engine, bool menu = false )
	{
		// Initialize the menu
		if ( menu )
		{
			Menu = new LauncherLoadContext( engine );
			Menu.AddAssembly( "Sandbox.Event" );
			Menu.AddAssembly( "Sandbox.Game" );
			Menu.AddMainAssembly( "Sandbox.Menu" );
		}

		// Initialize the client
		Client = new LauncherLoadContext( engine );
		Client.AddAssembly( "Sandbox.Event" );
		Client.AddAssembly( "Sandbox.Game" );
		Client.AddMainAssembly( "Sandbox.Client" );

		// Initialize the server
		Server = new LauncherLoadContext( engine );
		Server.AddAssembly( "Sandbox.Event" );
		Server.AddAssembly( "Sandbox.Game" );
		Server.AddMainAssembly( "Sandbox.Server" );
	}
}
