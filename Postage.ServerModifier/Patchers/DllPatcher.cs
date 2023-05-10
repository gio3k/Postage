using System.Reflection;
using System.Reflection.Emit;
using MonoMod.Cil;
using Postage.Core;
using Sandbox;
using Sandbox.Internal;

namespace Postage.ServerModifier.Patchers;

using HarmonyLib;

/// <summary>
/// Patches AccessControl to allow any assembly
/// </summary>
internal static class DllPatcher
{
	public static void Patch( Harmony harmony )
	{
		Log.Info( "Attempting to patch ServerDll..." );
		var assembly = Assembly.Load( "Sandbox.Server" );
		var type = assembly.GetType( "Sandbox.ServerDll" );
		if ( type == null )
			throw new Exception( "Couldn't find Sandbox.ServerDll" );
		var original = type.GetMethod( "OnServerInitialize" );
		harmony.Patch( original, new HarmonyMethod( Prefix ), transpiler: new HarmonyMethod( Transpiler ) );
	}

	private static void SetHotloadManagerAssemblyResolver( object accessControl )
	{
		var propertyA = typeof(Game).Property( "HotloadManager" );
		var typeA = propertyA.PropertyType;
		var valueA = propertyA.GetValue( null );

		var propertyB = typeA.Property( "AssemblyResolver" );
		propertyB.SetValue( valueA, accessControl );
	}

	private static bool Prefix( object __instance )
	{
		Log.Info( "Initializing server..." );
		Log.Info( "Preparing package loader..." );
		var packageLoader = ServerInitUtil.PreparePackageLoader( __instance, GlobalGameNamespace.TypeLibrary );

		Log.Info( "Getting package loader access control..." );
		var accessControl = ServerInitUtil.GetPackageLoaderAccessControl( packageLoader );

		Log.Info( "Setting HotloadManager.AssemblyResolver" );
		SetHotloadManagerAssemblyResolver( accessControl );

		Log.Info( "Setting game package..." );
		ServerInitUtil.SetGamePackage();

		Log.Info( "Loading all package assemblies..." );
		ServerInitUtil.LoadAllPackageAssemblies( packageLoader );

		foreach ( var typeDescription in GlobalGameNamespace.TypeLibrary.GetTypes<BaseGameManager>() )
		{
			Log.Info( typeDescription.FullName );
		}

		return true;
	}

	private static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> input, ILGenerator generator )
	{
		var output = new List<CodeInstruction>( input );

		var label = generator.DefineLabel();

		for ( var i = 0; i < output.Count; i++ )
		{
			var instruction = output[i];
			if ( instruction.opcode != OpCodes.Endfinally )
				continue;

			output[i + 1].labels.Add( label );
			break;
		}

		Log.Info( $"jumping to {label}" );
		output[0] = new CodeInstruction( OpCodes.Nop );
		output[1] = new CodeInstruction( OpCodes.Nop );
		output[2] = new CodeInstruction( OpCodes.Br, label );
		return output;
	}
}
