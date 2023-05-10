using HarmonyLib;
using Postage.Core.Patchers;

namespace Postage.Core;

public static class Patches
{
	public static void Apply()
	{
		var harmony = new Harmony( "gio.postage.core" );
		AccessPatcher.Patch( harmony );
		LocalPackagePatcher.Patch( harmony );
	}
}
