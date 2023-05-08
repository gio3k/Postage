using System.Runtime.InteropServices;

namespace Postage;

public static class Hooks
{
	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_PreInit( nint gameFolder, int isDedicatedServer, int isRetail,
		int toolsMode, int testMode, int netVersion, int apiVersion )
	{
		Log.Info( "hello from preinit" );
		return 1;
	}

	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_Init()
	{
		Log.Info( "hello from init" );
		return 1;
	}
}
