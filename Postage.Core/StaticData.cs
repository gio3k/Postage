using Sandbox;
using Sandbox.Engine;

namespace Postage.Core;

public static class StaticData
{
	internal static List<PackageManager.ActivePackage> Loaded = new();
	internal static PackageManager.ActivePackage GamePackage = new();

	public static void UpdateServerContextPackage() => ServerContext.GamePackage = GamePackage;
}
