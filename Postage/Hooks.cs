using System.Runtime.InteropServices;

namespace Postage;

public static class Hooks
{
	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_PreInit( nint ptrRoot, int iDedicated, int iRetail,
		int iTools, int iTest, int iNet, int iApi )
	{
		Log.Info( "PreInit hook worked!" );

		var root = Sandbox.Interop.GetString( ptrRoot );
		var isDedicated = iDedicated == 1;
		var isRetail = iRetail == 1;
		var isTools = iTools == 1;
		var isTest = iTest == 1;
		var netVersion = iNet;
		var apiVersion = iApi;

		Log.Info( $"dedicated {isDedicated}, retail {isRetail}, tools {isTools}, test {isTest}" );

		return 1;
	}

	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_Init()
	{
		Log.Info( "Init hook worked!" );
		return 1;
	}
}
