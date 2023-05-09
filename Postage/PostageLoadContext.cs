using System.Reflection;
using System.Runtime.Loader;
using Sandbox.Engine;

namespace Postage;

public class PostageLoadContext : AssemblyLoadContext
{
	private readonly Dictionary<string, Assembly> _lookup = new();

	protected string EnginePath { get; }

	public PostageLoadContext( string enginePath, string assemblyName )
	{
		using var timer = Bootstrap.StartupTiming?.ScopeTimer( "LoadContext." + assemblyName );

		EnginePath = enginePath;
		AddAssembly( "Sandbox.Event" );
		AddAssembly( "Sandbox.Game" );
		AddAssembly( assemblyName );
		Get( assemblyName ).GetType( "AssemblyInitialize", true, true )!
			.GetMethod( "Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )!
			.Invoke( null, null );
	}

	protected void AddAssembly( string v ) =>
		_lookup[v] = LoadFromAssemblyPath( $"{EnginePath}\\{v}.dll" );

	public Assembly Get( string name ) => _lookup.TryGetValue( name, out var assembly ) ? assembly : null;

	protected override Assembly Load( AssemblyName assemblyName ) => Get( assemblyName.Name );
}
