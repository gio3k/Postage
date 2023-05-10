using Sandbox;
using Sandbox.Internal;
using Steamworks;

namespace Postage.Core.Engine;

/// <summary>
/// Initializes things that the menu usually does
/// </summary>
public static class NoMenuHelper
{
	public static void Bootstrap()
	{
		if ( IMenuAddon.Current != null ) return;

		Log.Info( "Loaded without menu - doing extra bootstrapping" );

		ConsoleSystem.Collection.AddAssembly( "Sandbox.Engine" );
		ConsoleSystem.Collection.AddAssembly( "Sandbox.System" );

		SteamClient.Init( 590830 );
		AccountInformation.Update();
	}

	public static void PostBootstrap()
	{
		if ( IMenuAddon.Current != null ) return;
	}
}
