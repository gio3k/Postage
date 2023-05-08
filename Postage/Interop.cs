using System.Runtime.InteropServices;
using Managed.SandboxEngine;
using NativeEngine;
using Sandbox;

namespace Postage;

/// <summary>
/// Customized NativeInterop
/// </summary>
public static class Interop
{
	[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
	private unsafe delegate void NetCoreImportDelegate( int hash, void* imports, void* exports, int* structSizes );

	public static void Init( string root )
	{
		// Try to load engine2.dll
		Log.Info( "Loading engine2..." );
		if ( !NativeLibrary.TryLoad( $"{root}\\bin\\win64\\engine2.dll", out var handle ) )
			throw new Exception( "Couldn't load engine2.dll" );

		// Try to get the interop gen helper export
		Log.Info( "Finding igen_engine export..." );
		var export = NativeLibrary.GetExport( handle, "igen_engine" );
		if ( export == 0 )
			throw new Exception( "Couldn't get igen_engine export" );

		Log.Info( "Getting delegate for export..." );
		var dfn = Marshal.GetDelegateForFunctionPointer<NetCoreImportDelegate>( export );
		Perform( dfn );
	}

	private static unsafe void Perform( NetCoreImportDelegate dfn )
	{
		nint[] imports =
		{
			(nint)(delegate* unmanaged<int, IntPtr, void>)(&Exports.SndbxDgnstcs_Logging_RegisterEngineLogger),
			(nint)(delegate* unmanaged<IntPtr, int, int, int, int, int, int, int>)(&Hooks
				.SandboxEngine_Bootstrap_PreInit),
			(nint)(delegate* unmanaged<int>)(&Hooks.SandboxEngine_Bootstrap_Init),
			(nint)(delegate* unmanaged<IntPtr, IntPtr, ulong, void>)(&Exports.SandboxEngine_Hardware_SetGpu),
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
			(nint)(delegate* unmanaged<IntPtr, void>)(&Exports.Sandbox_EngineLoop_InitNetworkGameClient),
			(nint)(delegate* unmanaged<IntPtr, void>)(&Exports.Sandbox_EngineLoop_InitServerSideClient),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_StartConnecting),
			(nint)(delegate* unmanaged<long, void>)(&Exports.Sandbox_EngineLoop_UpdateProgressBar),
			(nint)(delegate* unmanaged<InputEvent, int>)(&Exports.Sandbox_EngineLoop_HandleInputEvent),
			(nint)(delegate* unmanaged<int, IntPtr, IntPtr, int>)(&Exports.Sandbox_EngineLoop_ConvarFromClient),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_SimulateUI),
			(nint)(delegate* unmanaged<int, IntPtr, IntPtr, void>)(&Exports.Sandbox_EngineLoop_Print),
			(nint)(delegate* unmanaged<IntPtr, void>)(&Exports.Sandbox_EngineLoop_LoadStart),
			(nint)(delegate* unmanaged<int>)(&Exports.Sandbox_EngineLoop_LoadLoop),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_HideLoadingPlaque),
			(nint)(delegate* unmanaged<IntPtr, IntPtr>)(&Exports.Sandbox_EngineLoop_ResolveMapName),
			(nint)(delegate* unmanaged<long, IntPtr, void>)(&Exports.Sandbox_EngineLoop_ClientDisconnect),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_Exiting),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_UpdateServerDetails),
			(nint)(delegate* unmanaged<IntPtr, IntPtr, long, void>)(&Exports
				.Sandbox_EngineLoop_DispatchConsoleCommand),
			(nint)(delegate* unmanaged<void>)(&Exports.Sandbox_EngineLoop_PostStringTableUpdate),
			(nint)(delegate* unmanaged<IntPtr, void>)(&Exports.Sandbox_EngineLoop_NotifyOfServerConnect),
			(nint)(delegate* unmanaged<IntPtr, void>)(&Exports.Sandbox_Graphics_OnMainView),
			(nint)(delegate* unmanaged<int, ManagedRenderSetup_t, void>)(&Exports.Sandbox_Graphics_OnLayer),
			(nint)(delegate* unmanaged<IntPtr, int, void>)(&Exports.Sandbox_Graphics_InitializeView),
			(nint)(delegate* unmanaged<IntPtr, uint, int>)(&Exports.Sandbox_HandleIndex_RegisterHandle),
			(nint)(delegate* unmanaged<int, void>)(&Exports.Sandbox_HandleIndex_FreeHandle),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkClient_Shutdown),
			(nint)(delegate* unmanaged<uint, IntPtr, void>)(&Exports.Sandbox_INetworkClient_OnFullConnect),
			(nint)(delegate* unmanaged<uint, int, IntPtr, int, void>)(&Exports.Sandbox_INetworkClient_OnNet),
			(nint)(delegate* unmanaged<uint, IntPtr, IntPtr, int>)
			(&Exports.Sandbox_INetworkClient_ProcessServerInfo),
			(nint)(delegate* unmanaged<uint, int, int, void>)(&Exports.Sandbox_INetworkClient_SignOnState_New),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkClient_SignOnState_Full),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkClient_Tick),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkServer_Shutdown),
			(nint)(delegate* unmanaged<uint, int, IntPtr, int, void>)(&Exports.Sandbox_INetworkServer_OnNet),
			(nint)(delegate* unmanaged<uint, void>)(&Exports.Sandbox_INetworkServer_Tick),
			(nint)(delegate* unmanaged<uint, IntPtr>)(&Exports.Sandbox_INetworkServer_FillServerInfo),
			(nint)(delegate* unmanaged<uint, long, int>)(&Exports.Sandbox_INetworkServer_ShouldConnect),
			(nint)(delegate* unmanaged<int, void>)(&Exports.SandboxPhysics_PhysicsEngine_OnPhysicsJointBreak),
			(nint)(delegate* unmanaged<float, void>)(&Exports.Sandbox_RealTime_Update),
			(nint)(delegate* unmanaged<ManagedRenderSetup_t, int, void>)(&Exports
				.Sandbox_ScnCstmbjctRndr_RenderObject),
			(nint)(delegate* unmanaged<int, IntPtr, int, void>)(&Exports.Steamworks_Dispatch_OnClientCallback)
		};

		var sizes = new[]
		{
			0, 1, 0, 4, 0, 0, 0, 4, 4, 8, 0, 0, 0, 4, 8, 4, 8, 4, 0, 4, 0, 4, 0, 4, 4, 4, 0, 4, 4, 4, 4, 0, 0, 4, 0,
			0, 0, 0, 1, 4, 4, 0, 4, 4, 1, 0, 0, 1, 4, 4, 0, 4, 4, 4, 4, 1, 4, 0, 0, 0, 4, 4, 4, 4, 4, 0, 0, 0, 0, 0,
			0, 1
		};
		sizes[0] = sizeof(BBox);
		sizes[2] = sizeof(AudioDeviceDesc);
		sizes[4] = sizeof(AnimVariant);
		sizes[5] = sizeof(Color32);
		sizes[6] = sizeof(Color24);
		sizes[10] = sizeof(CTextureDesc);
		sizes[11] = sizeof(Transform);
		sizes[12] = sizeof(VideoDisplayMode);
		sizes[18] = sizeof(HMaterial);
		sizes[20] = sizeof(IndexBufferHandle_t);
		sizes[22] = sizeof(InputEvent);
		sizes[26] = sizeof(ManagedRenderSetup_t);
		sizes[31] = sizeof(Angles);
		sizes[32] = sizeof(Rotation);
		sizes[34] = sizeof(RayTracingRequest);
		sizes[35] = sizeof(RayTracingSingleResult);
		sizes[36] = sizeof(NativeRect);
		sizes[37] = sizeof(Rect3D);
		sizes[41] = sizeof(RenderShaderHandle_t);
		sizes[45] = sizeof(RenderViewport);
		sizes[46] = sizeof(Capsule);
		sizes[50] = sizeof(Vertex);
		sizes[57] = sizeof(SwapChainHandle_t);
		sizes[58] = sizeof(TextureConfig);
		sizes[59] = sizeof(TextureBuilder);
		sizes[65] = sizeof(Vector3);
		sizes[66] = sizeof(Vector2);
		sizes[67] = sizeof(Vector4);
		sizes[68] = sizeof(VertexBufferHandle_t);
		sizes[69] = sizeof(VertexField);
		sizes[70] = sizeof(Matrix);

		var exports = new nint[1740];

		Log.Info( "Performing engine interop update..." );
		fixed ( nint* pImports = imports )
		fixed ( nint* pExports = exports )
		fixed ( int* pSizes = sizes )
			dfn( 2164, pImports, pExports, pSizes );
	}
}
