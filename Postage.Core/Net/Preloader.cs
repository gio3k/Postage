using System.Runtime.InteropServices;
using Postage.Core.Engine;

namespace Postage;

public class Preloader
{
	public nint Engine2 { get; }

	public Preloader( Source2Instance engine )
	{
		Log.Info( "Loading engine2.dll" );
		Engine2 = NativeLibrary.Load( $"{engine.Directory.Binaries}\\engine2.dll" );
	}
}
