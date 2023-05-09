using Sandbox;
using Sandbox.Engine;

namespace Postage;

public struct ServerDllShim
{
	internal IServerDll Instance;
	private Type _type;

	internal ServerDllShim( IServerDll instance )
	{
		Instance = instance;
		_type = instance.GetType();
	}

	internal PackageLoader PackageLoader
	{
		get => _type.Field( "PackageLoader" ).GetValue( Instance ) as PackageLoader;
		set => _type.Field( "PackageLoader" ).SetValue( Instance, value );
	}
}
