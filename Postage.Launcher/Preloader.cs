using System.Runtime.InteropServices;

namespace Postage;

public class Preloader
{
	public nint Engine2 { get; private set; }

	public Preloader()
	{
		Log.Info( "Loading engine2.dll" );
		Engine2 = NativeLibrary.Load( $"{Launcher.GameDirectory.Binaries}\\engine2.dll" );
	}
}
