using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Sandbox;
using Sandbox.Utility;

namespace Postage;

public static class AccessPatcher
{
	public static void Patch()
	{
		Log.Info( "Initializing Access Control patcher..." );

		var harmony = new Harmony( "gio.postage" );
		var original = typeof(AccessControl).GetMethod( "VerifyAssembly" );
		var patched = typeof(AccessPatcher).GetMethod( "VerifyAssembly", BindingFlags.NonPublic | BindingFlags.Static );

		Log.Info( "Patching VerifyAssembly..." );
		harmony.Patch( original, new HarmonyMethod( patched ) );
	}

	private static TrustedBinaryStream Create( Stream stream )
	{
		var method = typeof(TrustedBinaryStream).GetMethod( "CreateInternal",
			BindingFlags.NonPublic | BindingFlags.Static,
			new[] { typeof(Stream) } );
		if ( method == null )
			throw new Exception( "Couldn't find TrustedBinaryStream.CreateInternal" );
		var ret = method.Invoke( null, new object[] { stream } );
		if ( ret is not TrustedBinaryStream tbs )
			throw new Exception( "Unknown return from TrustedBinaryStream.CreateInternal" );
		return tbs;
	}

	private static void AddSafeAssembly( AccessControl instance, string name )
	{
		var method = typeof(AccessControl).GetMethod( "AddSafeAssembly", BindingFlags.NonPublic | BindingFlags.Instance,
			new[] { typeof(string) } );
		if ( method == null )
			throw new Exception( "Couldn't find AccessControl.AddSafeAssembly" );
		method.Invoke( instance, new object[] { name } );
	}

	private static AssemblyDefinition GetInstanceAssembly( AccessControl instance )
	{
		var field = typeof(AccessControl).GetField( "Assembly", BindingFlags.NonPublic | BindingFlags.Instance );
		if ( field == null )
			throw new Exception( "Couldn't find AccessControl.Assembly" );
		var ret = field.GetValue( instance );
		if ( ret is not AssemblyDefinition asm )
			throw new Exception( "Unknown value in AccessControl.Assembly" );
		return asm;
	}

	private static void AddToInstanceVerified( AccessControl instance, ulong hash )
	{
		var field = typeof(AccessControl).GetField( "verified", BindingFlags.NonPublic | BindingFlags.Static );
		if ( field == null )
			throw new Exception( "Couldn't find AccessControl.verified" );
		var ret = field.GetValue( instance );
		if ( ret is not HashSet<ulong> hs )
			throw new Exception( "Unknown value in AccessControl.verified" );
		hs.Add( hash );
	}

	private static bool VerifyAssembly( Stream dll, out TrustedBinaryStream outStream, ref bool __result,
		AccessControl __instance )
	{
		outStream = null;

		try
		{
			__instance.Errors.Clear();

			__instance.InitTouches( dll );

			var asm = GetInstanceAssembly( __instance );

			Log.Info( $"Verifying assembly {asm.Name}" );

			AddSafeAssembly( __instance, asm.Name.Name );

			dll.Seek( 0, SeekOrigin.Begin );

			AddToInstanceVerified( __instance, Crc64.FromStream( dll ) );
		}
		catch ( Exception e )
		{
			Log.Warn( "Exception in patched VerifyAssembly" );
			Log.Info( e );
		}

		outStream = Create( dll );
		Log.Info( $"Verified -> input size {dll.Length}, output size {outStream.Length}" );

		outStream.Seek( 0, SeekOrigin.Begin );

		__result = true;
		return false;
	}
}
