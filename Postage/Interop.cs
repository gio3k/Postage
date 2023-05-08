using Managed.SandboxEngine;

namespace Postage;

public static class Interop
{
	public static void Init( string root )
	{
		Environment.CurrentDirectory = root;

		NativeInterop.Initialize();
	}
}
