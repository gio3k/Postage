using HarmonyLib;
using Postage.ServerModifier.Patchers;

public class AssemblyInitialize
{
	public static void Initialize()
	{
		Log.Info( "Postage ServerModifier initialized!" );

		Log.Info( "Preparing patcher..." );
		var harmony = new Harmony( "gio.postage.server" );

		Log.Info( "Patching..." );
		DllPatcher.Patch( harmony );
	}
}
