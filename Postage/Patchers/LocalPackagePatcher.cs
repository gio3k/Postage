using HarmonyLib;
using Sandbox;

namespace Postage;

public static class LocalPackagePatcher
{
	public static void Patch()
	{
		PatchCore.Update();
		Log.Info( "About to patch local packages..." );

		var original = typeof(LocalPackage).GetMethod( "NeedsLocalBasePackage" );

		Log.Info( "Patching OnServerInitialize..." );
		PatchCore.Harmony.Patch( original, new HarmonyMethod( Prefix ) );
	}

	private static bool Prefix( ref bool __result )
	{
		__result = false;
		return false;
	}
}
