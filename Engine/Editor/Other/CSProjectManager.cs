using System.Xml;
using System.Diagnostics;

namespace Concrete;

public static class CSProjectManager
{
    public static void RebuildCSProject(string dir)
    {
        // create hidden concrete folder
        string hidden = Path.Combine(dir, ".concrete");
        if (!Directory.Exists(hidden)) Directory.CreateDirectory(hidden);
        new DirectoryInfo(hidden).Attributes |= FileAttributes.Hidden;

        // rebuild csproj file
        string csproj = Path.Combine(dir, "project.csproj");
        if (File.Exists(csproj)) File.Delete(csproj);
        string[] properties =
        [
            "<OutputType>library</OutputType>",
            "<TargetFramework>net10.0</TargetFramework>",
            "<ImplicitUsings>enable</ImplicitUsings>",
            "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>",
            "<DebugType>embedded</DebugType>",
            "<SatelliteResourceLanguages>none</SatelliteResourceLanguages>",
            "<BaseOutputPath>.concrete/bin/</BaseOutputPath>",
            "<BaseIntermediateOutputPath>.concrete/obj/</BaseIntermediateOutputPath>",
            "<RestoreOutputPath>.concrete/obj/</RestoreOutputPath>",
        ];
        Dotnet.New(csproj, properties);

        // add the editor's shared assembly as a reference for script autocomplete
        Dotnet.AddDll(csproj, Path.GetFullPath("Shared.dll"));

        // make sure gitignore exists
        string[] ignores = ["*.csproj", "bin/", "obj/", ".idea/", ".vscode/", ".vs/", ".concrete/"];
        string gitignore_contents = "";
        foreach (var ignore in ignores)
        {
            bool last = ignore == ignores[ignores.Length - 1];
            gitignore_contents += last ? ignore : ignore + "\n";
        }
        string gitignore_path = Path.Combine(dir, ".gitignore");
        if (!File.Exists(gitignore_path)) File.WriteAllText(gitignore_path, gitignore_contents);
    }
}