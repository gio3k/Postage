using Sandbox;

namespace Postage;

public class Project
{
	public Project() => ProjectManager.Projects.Add( this );

	public List<string> Assemblies;
	public string AddonDirectory;

	internal PackageManager.ActivePackage ActivePackage;
}
