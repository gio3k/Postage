using HarmonyLib;

namespace Postage;

internal static class PatchCore
{
	public static Harmony Harmony { get; private set; }

	public static void Update()
	{
		Harmony ??= new Harmony( "gio.postage" );
	}
}
