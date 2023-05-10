using HarmonyLib;
using Postage.Core.Engine;
using Sandbox.Engine;

namespace Postage.Core.Patchers;

public static class BootstrapPatcher
{
	public static void Patch( Harmony harmony )
	{
		Log.Info( "Attempting to patch bootstrap..." );
		{
			var original = typeof(Bootstrap).Method( "PreInit" );
			harmony.Patch( original, new HarmonyMethod( PreInitPrefix ) );
		}

		{
			var original = typeof(Bootstrap).Method( "Init" );
			harmony.Patch( original, new HarmonyMethod( InitPrefix ) );
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
}
