using System.Reflection;

namespace Postage;

public static class AssemblyResolver
{
	public static Assembly AssemblyResolve( object sender, ResolveEventArgs args )
	{
		var name = args.Name.Split( ',' )[0].Replace( ".resources", "" );
		Log.Info( name );

		var path = $"{Launcher.GameDirectory.Libraries}\\{name}.dll";
		if ( File.Exists( path ) )
			return Assembly.LoadFrom( path );

		path = $"{Launcher.LauncherDirectory}\\{name}.dll";

		if ( File.Exists( path ) )
			return Assembly.LoadFrom( path );

		foreach ( var (lib, bytes) in Launcher.Libraries )
		{
			if ( lib != name && name != $"package.{lib}" )
				continue;

			Log.Info( $"Loading game library {lib}" );
			return Assembly.Load( bytes );
		}

		Log.Warn( $"{name} / {path} not found" );
		return null;
	}
}
