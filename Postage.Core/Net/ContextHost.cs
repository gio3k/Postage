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

	public ContextHost( Source2Instance engine )
	{
		// Initialize the menu
		var menu = new LauncherLoadContext( engine );
		menu.AddAssembly( "Sandbox.Event" );
		menu.AddAssembly( "Sandbox.Game" );
		menu.AddMainAssembly( "Sandbox.Menu" );

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
