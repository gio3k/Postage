using HarmonyLib;
using Sandbox;

namespace Postage.Core.Patchers;

public static class LocalPackagePatcher
{
	public static void Patch( Harmony harmony )
	{
		Log.Info( "Attempting to patch local packages..." );
		var original = typeof(LocalPackage).GetMethod( "NeedsLocalBasePackage" );
		harmony.Patch( original, new HarmonyMethod( Prefix ) );
	}

	private static bool Prefix( ref bool __result )
	{
		__result = false;
		return false;
	}
}
