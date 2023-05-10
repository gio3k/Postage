using System.Reflection;
using System.Runtime.Loader;
using Postage.Core.Engine;

namespace Postage.Net;

public class LauncherLoadContext : AssemblyLoadContext
{
	private readonly Dictionary<string, Assembly> _lookup = new();

	private readonly Source2Instance _engine;

	public LauncherLoadContext( Source2Instance engine ) => _engine = engine;

	public void AddMainAssembly( string name, bool absolute = false )
	{
		AddAssembly( name, absolute );
		Get( name ).GetType( "AssemblyInitialize", true, true )!
			.GetMethod( "Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )!
			.Invoke( null, null );
	}

	public void AddAssembly( string v, bool absolute = false ) =>
		_lookup[v] = LoadFromAssemblyPath( absolute ? v : $"{_engine.Directory.Libraries}\\{v}.dll" );

	public Assembly Get( string name ) => _lookup.TryGetValue( name, out var assembly ) ? assembly : null;

	protected override Assembly Load( AssemblyName assemblyName ) => Get( assemblyName.Name );
}
