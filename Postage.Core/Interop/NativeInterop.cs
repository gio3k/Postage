namespace Postage.Core;

public static class NativeInterop
{
	public static void Initialize() => Managed.SandboxEngine.NativeInterop.Initialize();
}
