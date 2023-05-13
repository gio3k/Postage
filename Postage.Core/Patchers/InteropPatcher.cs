using HarmonyLib;
using NativeEngine;
using Postage.Core.Engine;
using Sandbox;
using Sandbox.Engine;

namespace Postage.Core.Patchers;

public static class InteropPatcher
{
	public static void Patch( Harmony harmony )
	{
		Log.Info( "Attempting to patch exports..." );
		{
			var original = typeof(Bootstrap).Method( "PreInit" );
			harmony.Patch( original, new HarmonyMethod( PreInitPrefix ) );
		}

		{
			var original = typeof(Bootstrap).Method( "Init" );
			harmony.Patch( original, new HarmonyMethod( InitPrefix ) );
		}

		{
			var original = typeof(EngineLoop).Method( "EnterMainMenu" );
			harmony.Patch( original, new HarmonyMethod( EnterMainMenuPrefix ) );
		}
	}

	private static bool PreInitPrefix( ref bool __result, string rootFolder, bool dedicatedserver, bool isRetail,
		bool toolsMode, bool testMode, int netVersion, int apiVersion )
	{
		PostageBootstrap.PreInit( rootFolder, dedicatedserver, isRetail, toolsMode, testMode, netVersion, apiVersion );
		__result = true;
		return false;
	}

	private static bool InitPrefix( ref bool __result )
	{
		PostageBootstrap.Init();
		__result = true;
		return false;
	}

	private static bool EnterMainMenuPrefix()
	{
		Log.Info( "Starting game..." );
		g_pInputService.InsertCommand( InputCommandSource.ICS_SERVER, "gamemode local.calico\n", 0, 0 );
		g_pInputService.InsertCommand( InputCommandSource.ICS_SERVER, "map calico_dev\n", 0, 0 );
		return true;
	}
}
