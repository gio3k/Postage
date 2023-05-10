using Sandbox;
using Sandbox.Engine;
using Steamworks;

namespace Postage.Core.Engine;

/// <summary>
/// Initializes things that the menu usually does
/// </summary>
public static class NoMenuHelper
{
	public static void Bootstrap()
	{
		if ( IMenuDll.Current != null ) return;

		Log.Info( "Loaded without menu - doing extra bootstrapping" );

		ConsoleSystem.Collection.AddAssembly( "Sandbox.Engine" );
		ConsoleSystem.Collection.AddAssembly( "Sandbox.System" );

		SteamClient.Init( 590830 );
		AccountInformation.Update();
	}

	public static void PostBootstrap()
	{
		if ( IMenuDll.Current != null ) return;
	}
}
