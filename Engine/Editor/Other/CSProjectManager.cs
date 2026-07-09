using System.Xml;
using System.Diagnostics;

namespace Concrete;

public static class CSProjectManager
{
    public static void RebuildCSProject(string dir)
    {
        // rebuild or initialize the csproj file without removing any manual edits
        string csproj = Path.Combine(dir, "project.csproj");
        string[] properties =
        [
            "<OutputType>library</OutputType>",
            "<TargetFramework>net10.0</TargetFramework>",
            "<ImplicitUsings>enable</ImplicitUsings>",
            "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>",
            "<DebugType>embedded</DebugType>",
            "<SatelliteResourceLanguages>none</SatelliteResourceLanguages>"
        ];
        if (!File.Exists(csproj)) Dotnet.New(csproj, properties);

        // add or update the editor's shared assembly reference for script autocomplete
        Dotnet.EnsureSharedReference(csproj);

        // rebuild gitignore
        string[] ignores = 
        [
            "*.csproj",
            "bin/",
            "obj/",
            ".idea/",
            ".vscode/",
            ".vs/"
        ];
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