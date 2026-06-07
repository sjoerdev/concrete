using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Concrete;

public static class ScriptCompiler
{
    public static byte[] RecompileScripts(string directoryToScan)
    {
        bool NotOfBinOrObj(string file)
        {
            var dir = Path.GetDirectoryName(file);
            var parts = dir.Split(Path.DirectorySeparatorChar);
            foreach (var part in parts) if (part == "bin" || part == "obj") return false;
            return true;
        }

        // scan entire project root recursively for all script files (excluding dotnet temps from bin and obj)
        var scriptPaths = Directory.GetFiles(directoryToScan, "*.cs", SearchOption.AllDirectories).Where(NotOfBinOrObj).ToList();

        if (scriptPaths.Count == 0)
        {
            Debug.Log("No scripts found to compile.");
            return null;
        }

        Debug.Log($"Compiling {scriptPaths.Count} script(s)...");

        var compiledAssembly = CompileScriptsToAssembly(scriptPaths, out var errors, out var dllbytes, directoryToScan);

        if (compiledAssembly == null)
        {
            Debug.Log($"Script compilation failed with {errors.Count} errors");
            foreach (var error in errors) Debug.Log(error.ToString());
            return null;
        }

        Debug.Log("Scripts compiled and loaded successfully.");

        return dllbytes;
    }

    public static Assembly CompileScriptsToAssembly(List<string> paths, out List<Diagnostic> errors, out byte[] dllbytes, string projectRoot)
    {
        dllbytes = null;
        errors = null;

        // parse scripts into syntax trees
        List<SyntaxTree> syntaxTrees = [];
        for (int i = 0; i < paths.Count; i++)
        {
            string path = paths[i];
            string source = File.ReadAllText(path);
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            syntaxTrees.Add(syntaxTree);
        }

        // get reference to the public api assembly
        string executableDirectory = AppContext.BaseDirectory;
        string sharedAssemblyPath = Path.Combine(executableDirectory, "Shared.dll");
        if (!File.Exists(sharedAssemblyPath))
        {
            Debug.Log($"Error: Shared.dll not found at '{sharedAssemblyPath}'");
            return null;
        }
        var sharedAssemblyReference = MetadataReference.CreateFromFile(sharedAssemblyPath);

        // get references to dotnet runtime
        string[] trustedPlatformAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
        var dotnetRuntimeReferences = trustedPlatformAssembliesPaths.Select(path => MetadataReference.CreateFromFile(path)).ToList();

        // combine all references
        List<MetadataReference> references = [];
        references.Add(sharedAssemblyReference);
        references.AddRange(dotnetRuntimeReferences);
        
        // compile scripts
        var compilationOptions = new CSharpCompilationOptions(
            outputKind: OutputKind.DynamicallyLinkedLibrary, 
            optimizationLevel: OptimizationLevel.Release, 
            deterministic: true
        );
        var compilation = CSharpCompilation.Create("Scripts", syntaxTrees, references, compilationOptions);

        // load il into memory
        using var memoryStream = new MemoryStream();
        var result = compilation.Emit(memoryStream);

        // check for compilation errors
        if (!result.Success)
        {
            errors = result.Diagnostics.Where(diagnosis => diagnosis.Severity == DiagnosticSeverity.Error).ToList();
            return null;
        }

        // return dll bytes
        dllbytes = memoryStream.ToArray();

        // return assembly
        var assembly = Assembly.Load(dllbytes);
        return assembly;
    }
}