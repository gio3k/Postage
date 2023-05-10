using System.Runtime.InteropServices;

namespace Postage.Engine;

public static class EngineHooks
{
	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_PreInit( nint ptrRoot, int iDedicated, int iRetail,
		int iTools, int iTest, int iNet, int iApi )
	{
		return 1;
	}

	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_Init()
	{
		return 1;
	}
}
