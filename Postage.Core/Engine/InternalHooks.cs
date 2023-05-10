using System.Runtime.InteropServices;

namespace Postage.Core.Engine;

public class InternalHooks
{
	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_PreInit( nint ptrRoot, int iDedicated, int iRetail,
		int iTools, int iTest, int iNet, int iApi )
	{
		Log.Info( "Postage is handling PreInit!" );

		// Hand over to PostageBootstrap
		PostageBootstrap.PreInit( Sandbox.Interop.GetString( ptrRoot ), iDedicated == 1, iRetail == 1, iTools == 1,
			iTest == 1, iNet, iApi );

		return 1;
	}

	[UnmanagedCallersOnly]
	public static int SandboxEngine_Bootstrap_Init()
	{
		Log.Info( "Postage is handling Init!" );

		// Hand over to PostageBootstrap
		PostageBootstrap.Init();

		return 1;
	}
}
