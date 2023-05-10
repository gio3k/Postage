namespace Postage.Core;

public readonly struct GameDirectory
{
	public string Root { get; }
	public string Binaries => $"{Root}\\bin\\win64";
	public string Libraries => $"{Root}\\bin\\managed";

	public GameDirectory( string root ) => Root = root;

	public override string ToString() => Root;
}
