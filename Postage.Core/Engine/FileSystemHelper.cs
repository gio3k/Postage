using Sandbox;

namespace Postage.Core.Engine;

public static class FileSystemHelper
{
	/// <summary>
	/// Initializes the most basic parts of EngineFileSystem
	/// </summary>
	public static void Initialize( string root )
	{
		var type = typeof(EngineFileSystem);
		type.Property( "Root" ).SetValue( null, new RootFileSystem( root ) );
		type.Property( "Temporary" ).SetValue( null, new MemoryFileSystem() );
	}
}
