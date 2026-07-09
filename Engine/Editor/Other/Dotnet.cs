using System.Xml;
using System.Diagnostics;

namespace Concrete;

public static class Dotnet
{
    public static void Execute(string parameters)
    {
        var processStartInfo = new ProcessStartInfo("dotnet", parameters)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(processStartInfo);
        string output = process.StandardOutput.ReadToEnd();
        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();

        // if (!string.IsNullOrWhiteSpace(output)) Debug.Log("Output: " + output);
        if (!string.IsNullOrWhiteSpace(errors)) Debug.Log("Errors: " + errors);
    }

    public static void New(string projectPath, string[] properties)
    {
        string combined_properties = String.Join(Environment.NewLine, properties);

        string template = 
        $@"
            <Project Sdk=""Microsoft.NET.Sdk"">

            <PropertyGroup>
                {combined_properties}
            </PropertyGroup>

            </Project>
        ";

        File.WriteAllText(projectPath, template);
    }

    public static void AddDll(string csproj, string dll)
    {
        // checks
        if (!File.Exists(csproj)) throw new Exception($"the csproj file was not found: {csproj}");
        if (!File.Exists(dll)) throw new Exception($"The dll file was not found: {dll}");

        // load existing csproj xml
        var document = new XmlDocument();
        document.Load(csproj);

        // find or create itemgroup
        XmlNode itemGroup = document.SelectSingleNode("//ItemGroup[Reference]");
        if (itemGroup == null)
        {
            itemGroup = document.CreateElement("ItemGroup", document.DocumentElement?.NamespaceURI ?? string.Empty);
            document.DocumentElement?.AppendChild(itemGroup);
        }

        // create <Reference Include="">
        string referenceName = Path.GetFileNameWithoutExtension(dll);
        XmlElement reference = document.CreateElement("Reference", document.DocumentElement?.NamespaceURI ?? string.Empty);
        reference.SetAttribute("Include", referenceName);

        // create <HintPath>
        XmlElement hintPath = document.CreateElement("HintPath", document.DocumentElement?.NamespaceURI ?? string.Empty);
        string relativePath = Path.GetRelativePath(Path.GetDirectoryName(csproj)!, dll);
        hintPath.InnerText = relativePath;

        // append the new xml
        reference.AppendChild(hintPath);
        itemGroup.AppendChild(reference);

        // save
        document.Save(csproj);
        Execute($"build \"{csproj}\"");
    }
}