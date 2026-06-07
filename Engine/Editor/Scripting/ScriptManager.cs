using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;

namespace Concrete;

public static class ScriptManager
{
    public static Assembly cachedAssembly;

    public static byte[] RecompileScripts(string directoryToScan)
    {
        var dllbytes = ScriptCompiler.RecompileScripts(directoryToScan);
        if (dllbytes != null) cachedAssembly = Assembly.Load(dllbytes);
        return dllbytes;
    }

    public static Type GetClassTypeOfScript(string scriptPath)
    {
        string source = File.ReadAllText(scriptPath);
        var regexMatch = Regex.Match(source, @"class\s+([A-Za-z0-9_]+)");
        string className = regexMatch.Groups[1].Value;

        if (cachedAssembly != null)
        {
            var non_recomp_type = cachedAssembly.GetTypes().FirstOrDefault(x => x.Name == className);

            if (non_recomp_type != null) return non_recomp_type;
            else
            {
                var root = FindProjectRootFromScriptPath(scriptPath);
                RecompileScripts(root);
                var recomp_type = cachedAssembly?.GetTypes().FirstOrDefault(x => x.Name == className);
                return recomp_type;
            }
        }
        else
        {
            var root = FindProjectRootFromScriptPath(scriptPath);
            RecompileScripts(root);
            var recomp_type = cachedAssembly?.GetTypes().FirstOrDefault(x => x.Name == className);
            return recomp_type;
        }
    }

    static string FindProjectRootFromScriptPath(string scriptPath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(scriptPath));
        while (!string.IsNullOrEmpty(dir))
        {
            var candidate = Path.Combine(dir, "project.json");
            if (File.Exists(candidate)) return dir;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return Path.GetDirectoryName(Path.GetFullPath(scriptPath));
    }
}