using Postage.Net;

namespace Postage;

/// <summary>
/// Context container
/// </summary>
public class ContextHost
{
	public LauncherLoadContext Client { get; private set; }
	public LauncherLoadContext Server { get; private set; }

	public void Init()
	{
		// Initialize the client
		Client = new LauncherLoadContext();
		Client.AddAssembly( "Sandbox.Event" );
		Client.AddAssembly( "Sandbox.Game" );
		Client.AddMainAssembly( "Sandbox.Client" );

		// Initialize the server
		Client = new LauncherLoadContext();
		Client.AddAssembly( "Sandbox.Event" );
		Client.AddAssembly( "Sandbox.Game" );
		Client.AddMainAssembly( "Sandbox.Server" );
		Client.AddMainAssembly( $"{Launcher.LauncherDirectory}\\Postage.ServerModifier.dll", true );
	}
}
