DirectoryInfo info = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Library"));
var assemblies = info.GetFiles("*.dll", SearchOption.AllDirectories);

using(var writer = File.CreateText("LoadUnityAssemblies.csx"))
{
	foreach(var assembliyPath in assemblies)
	{
		writer.WriteLine(string.Format("#r \"{0}\"", assembliyPath.FullName));
	}
}
