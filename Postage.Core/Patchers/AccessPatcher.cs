using HarmonyLib;
using Sandbox;

namespace Postage.Core.Patchers;

/// <summary>
/// Patches AccessControl to allow any assembly
/// </summary>
internal static class AccessPatcher
{
	public static void Patch( Harmony harmony )
	{
		Log.Info( "Attempting to patch Access Control..." );
		var original = typeof(AccessControl).Method( "VerifyAssembly" );
		harmony.Patch( original, new HarmonyMethod( Prefix ) );
	}

	private static TrustedBinaryStream Create( Stream stream )
	{
		var method = typeof(TrustedBinaryStream).Method( "CreateInternal", new[] { typeof(Stream) } );
		if ( method == null )
			throw new Exception( "Couldn't find TrustedBinaryStream.CreateInternal" );
		var ret = method.Invoke( null, new object[] { stream } );
		if ( ret is not TrustedBinaryStream tbs )
			throw new Exception( "Unknown return from TrustedBinaryStream.CreateInternal" );
		return tbs;
	}

	private static bool Prefix( Stream dll, out TrustedBinaryStream outStream, ref bool __result,
		AccessControl __instance )
	{
		outStream = Create( dll );
		Log.Info( $"Passed access control -> input size {dll.Length}, output size {outStream.Length}" );

		outStream.Seek( 0, SeekOrigin.Begin );

		__result = true;
		return false;
	}
}
