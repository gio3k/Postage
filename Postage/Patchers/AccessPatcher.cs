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
		PatchCore.Update();
		Log.Info( "About to patch access control..." );

		var original = typeof(AccessControl).Method( "VerifyAssembly" );

		Log.Info( "Patching VerifyAssembly..." );
		PatchCore.Harmony.Patch( original, new HarmonyMethod( Prefix ) );
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

	private static void AddSafeAssembly( this AccessControl instance, string name )
	{
		var method = typeof(AccessControl).Method( "AddSafeAssembly", new[] { typeof(string) } );
		if ( method == null )
			throw new Exception( "Couldn't find AccessControl.AddSafeAssembly" );
		method.Invoke( instance, new object[] { name } );
	}

	private static AssemblyDefinition GetAssemblyDefinition( this AccessControl instance )
	{
		var field = typeof(AccessControl).Field( "Assembly" );
		if ( field == null )
			throw new Exception( "Couldn't find AccessControl.Assembly" );
		var ret = field.GetValue( instance );
		if ( ret is not AssemblyDefinition asm )
			throw new Exception( "Unknown value in AccessControl.Assembly" );
		return asm;
	}

	private static void AddToVerified( this AccessControl instance, ulong hash )
	{
		var field = typeof(AccessControl).GetField( "verified", BindingFlags.NonPublic | BindingFlags.Static );
		if ( field == null )
			throw new Exception( "Couldn't find AccessControl.verified" );
		var ret = field.GetValue( instance );
		if ( ret is not HashSet<ulong> hs )
			throw new Exception( "Unknown value in AccessControl.verified" );
		hs.Add( hash );
	}

	private static bool Prefix( Stream dll, out TrustedBinaryStream outStream, ref bool __result,
		AccessControl __instance )
	{
		outStream = null;

		try
		{
			__instance.Errors.Clear();

			__instance.InitTouches( dll );

			var asm = __instance.GetAssemblyDefinition();

			Log.Info( $"Verifying assembly {asm.Name}" );

			__instance.AddSafeAssembly( asm.Name.Name );

			dll.Seek( 0, SeekOrigin.Begin );

			__instance.AddToVerified( Crc64.FromStream( dll ) );
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
