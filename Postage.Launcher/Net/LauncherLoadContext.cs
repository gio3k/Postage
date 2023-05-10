using System.Reflection;
using System.Runtime.Loader;

namespace Postage.Net;

public class LauncherLoadContext : AssemblyLoadContext
{
	private readonly Dictionary<string, Assembly> _lookup = new();

	public LauncherLoadContext()
	{
	}

	public void AddMainAssembly( string name, bool absolute = false )
	{
		AddAssembly( name, absolute );
		Get( name ).GetType( "AssemblyInitialize", true, true )!
			.GetMethod( "Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )!
			.Invoke( null, null );
	}

	public void AddAssembly( string v, bool absolute = false ) =>
		_lookup[v] = LoadFromAssemblyPath( absolute ? v : $"{Launcher.GameDirectory.Libraries}\\{v}.dll" );

	public Assembly Get( string name ) => _lookup.TryGetValue( name, out var assembly ) ? assembly : null;

	protected override Assembly Load( AssemblyName assemblyName ) => Get( assemblyName.Name );
}
