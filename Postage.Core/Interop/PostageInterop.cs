using System.Numerics;
using System.Runtime.InteropServices;
using Managed.SandboxEngine;
using NativeEngine;
using Postage.Core.Engine;
using Sandbox;

namespace Postage.Core;

public static class Interop
{
	[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
	private unsafe delegate void NetCoreImportDelegate( int hash, void* imports, void* exports, int* structSizes );

	public static unsafe void InitializeEngineInterop( Source2Instance engine )
	{
		var export = NativeLibrary.GetExport( engine.Preloader.Engine2, "igen_engine" );
		if ( export == 0 )
			throw new Exception( "Couldn't load igen_engine from engine2.dll" );

		var dgp =
			Marshal.GetDelegateForFunctionPointer<NetCoreImportDelegate>( export );
		if ( dgp == null )
			throw new Exception( "Couldn't load fn ptr from engine2.dll" );

		Log.Info( "Preparing to initialize interop!" );
		var etm = GenerateEngineToManagedArray();
		var sizes = GenerateSizeArray();
		var mte = new nint[1740];

		if ( PreInitPtr == 0x0 || InitPtr == 0x0 )
			throw new Exception( "Interop hooks not set! Use SetHooks()" );

		Log.Info( "Initializing interop..." );
		fixed ( nint* imports = etm )
		fixed ( nint* exports = mte )
		fixed ( int* structSizes = sizes )
			dgp( 2164, imports, exports, structSizes );

		Log.Info( "Initialized." );
	}

	public static void SetHooks( nint preInit, nint init )
	{
		PreInitPtr = preInit;
		InitPtr = init;
	}

	private static nint PreInitPtr;
	private static nint InitPtr;

	private static unsafe nint[] GenerateEngineToManagedArray()
	{
		return new[]
		{
			(nint)(delegate* unmanaged<int, nint, void>)(&Exports.SndbxDgnstcs_Logging_RegisterEngineLogger),
			PreInitPtr, InitPtr,
			(nint)(delegate* unmanaged<nint, nint, ulong, void>)(&Exports.SandboxEngine_Hardware_SetGpu),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_EnterMainMenu),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_LeaveMainMenu),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_EnterGame),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_LeaveGame),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_ShowGameUI),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_HideGameUI),
			(nint)(delegate* unmanaged<double, uint, int, void>)(&Exports.Sandbox_EngineLoop_FrameStage),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_PreInput),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_PostInput),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_PreOutput),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_PostOutput),
			(nint)(delegate* unmanaged<int, int, int, void>)(&Exports.Sandbox_EngineLoop_PreServerTick),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_PostServerTick),
			(nint)(delegate* unmanaged<int, int, int, void>)(&Exports.Sandbox_EngineLoop_PreClientTick),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_PostClientTick),
			(nint)(delegate* unmanaged<nint, void>)(&Exports.Sandbox_EngineLoop_InitNetworkGameClient),
			(nint)(delegate* unmanaged<nint, void>)(&Exports.Sandbox_EngineLoop_InitServerSideClient),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_StartConnecting),
			(nint)(delegate* unmanaged<long, void>)(&Exports.Sandbox_EngineLoop_UpdateProgressBar),
			(nint)(delegate* unmanaged<InputEvent, int>)(&Exports.Sandbox_EngineLoop_HandleInputEvent),
			(nint)(delegate* unmanaged<int, nint, nint, int>)(&Exports.Sandbox_EngineLoop_ConvarFromClient),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_SimulateUI),
			(nint)(delegate* unmanaged<int, nint, nint, void>)(&Exports.Sandbox_EngineLoop_Print),
			(nint)(delegate* unmanaged<nint, void>)(&Exports.Sandbox_EngineLoop_LoadStart),
			(nint)(delegate* unmanaged<int>)(&Exports.Sandbox_EngineLoop_LoadLoop),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_HideLoadingPlaque),
			(nint)(delegate* unmanaged<nint, nint>)(&Exports.Sandbox_EngineLoop_ResolveMapName),
			(nint)(delegate* unmanaged<long, nint, void>)(&Exports.Sandbox_EngineLoop_ClientDisconnect),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_Exiting),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_UpdateServerDetails),
			(nint)(delegate* unmanaged<nint, nint, long, void>)(&Exports.Sandbox_EngineLoop_DispatchConsoleCommand),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_PostStringTableUpdate),
			(nint)(delegate* unmanaged<nint, void>)(&Exports.Sandbox_EngineLoop_NotifyOfServerConnect),
			(nint)(delegate* unmanaged<nint, void>)(&Exports.Sandbox_Graphics_OnMainView),
			(nint)(delegate* unmanaged<int, ManagedRenderSetup_t, void>)(&Exports.Sandbox_Graphics_OnLayer),
			(nint)(delegate* unmanaged<nint, int, void>)(&Exports.Sandbox_Graphics_InitializeView),
			(nint)(delegate* unmanaged<nint, uint, int>)(&Exports.Sandbox_HandleIndex_RegisterHandle),
			(nint)(delegate* unmanaged<int, void>)(&Exports.Sandbox_HandleIndex_FreeHandle),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkClient_Shutdown),
			(nint)(delegate* unmanaged<uint, nint, void>)(&Exports.Sandbox_INetworkClient_OnFullConnect),
			(nint)(delegate* unmanaged<uint, int, nint, int, void>)(&Exports.Sandbox_INetworkClient_OnNet),
			(nint)(delegate* unmanaged<uint, nint, nint, int>)(&Exports.Sandbox_INetworkClient_ProcessServerInfo),
			(nint)(delegate* unmanaged<uint, int, int, void>)(&Exports.Sandbox_INetworkClient_SignOnState_New),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkClient_SignOnState_Full),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkClient_Tick),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkServer_Shutdown),
			(nint)(delegate* unmanaged<uint, int, nint, int, void>)(&Exports.Sandbox_INetworkServer_OnNet),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkServer_Tick),
			(nint)(delegate* unmanaged<uint, nint>)(&Exports.Sandbox_INetworkServer_FillServerInfo),
			(nint)(delegate* unmanaged<uint, long, int>)(&Exports.Sandbox_INetworkServer_ShouldConnect),
			(nint)(delegate* unmanaged<int, void>)(&Exports.SandboxPhysics_PhysicsEngine_OnPhysicsJointBreak),
			(nint)(delegate* unmanaged<float, void>)(&Exports.Sandbox_RealTime_Update),
			(nint)(delegate* unmanaged<ManagedRenderSetup_t, int, void>)(&Exports
				.Sandbox_ScnCstmbjctRndr_RenderObject),
			(nint)(delegate* unmanaged<int, nint, int, void>)(&Exports.Steamworks_Dispatch_OnClientCallback)
		};
	}

	private static unsafe int[] GenerateSizeArray()
	{
		var arr = new[]
		{
			0, 1, 0, 4, 0, 0, 0, 4, 4, 8, 0, 0, 0, 4, 8, 4, 8, 4, 0, 4, 0, 4, 0, 4, 4, 4, 0, 4, 4, 4, 4, 0, 0, 4, 0,
			0, 0, 0, 1, 4, 4, 0, 4, 4, 1, 0, 0, 1, 4, 4, 0, 4, 4, 4, 4, 1, 4, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 0,
			0, 1
		};
		arr[0] = sizeof(BBox);
		arr[2] = sizeof(AudioDeviceDesc);
		arr[4] = sizeof(AnimVariant);
		arr[5] = sizeof(Color32);
		arr[6] = sizeof(Color24);
		arr[10] = sizeof(CTextureDesc);
		arr[11] = sizeof(Transform);
		arr[12] = sizeof(VideoDisplayMode);
		arr[18] = sizeof(HMaterial);
		arr[20] = sizeof(IndexBufferHandle_t);
		arr[22] = sizeof(InputEvent);
		arr[26] = sizeof(ManagedRenderSetup_t);
		arr[31] = sizeof(Angles);
		arr[32] = sizeof(Rotation);
		arr[34] = sizeof(RayTracingRequest);
		arr[35] = sizeof(RayTracingSingleResult);
		arr[36] = sizeof(NativeRect);
		arr[37] = sizeof(Rect3D);
		arr[41] = sizeof(RenderShaderHandle_t);
		arr[45] = sizeof(RenderViewport);
		arr[46] = sizeof(Capsule);
		arr[50] = sizeof(Vertex);
		arr[57] = sizeof(SwapChainHandle_t);
		arr[58] = sizeof(TextureConfig);
		arr[59] = sizeof(TextureBuilder);
		arr[65] = sizeof(Vector3);
		arr[66] = sizeof(Vector2);
		arr[67] = sizeof(Vector4);
		arr[68] = sizeof(VertexBufferHandle_t);
		arr[69] = sizeof(VertexField);
		arr[70] = sizeof(Matrix);
		return arr;
	}
}
