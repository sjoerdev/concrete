using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;

namespace Concrete;

public static class BuildWindow
{
    public static string buildDirectory = "";
    private static bool building = false;
    private static bool choosingDirectory = false;

    private static string statusText = "idle";
    private static Vector3 statusColor = Vector3.One;

    private static int platform = 0;
    private static string[] availablePlatforms = ["Windows x64", "Linux x64"];

    private static bool selfContained = true;

    private static bool hasDotnetSdkInstalled = Shell.IsCommandInPath("dotnet");

    public static void Draw(float deltaTime)
    {
        ImGui.Begin("\uf552 Build", ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoScrollbar);

        if (!hasDotnetSdkInstalled) SetStatus("The .NET SDK is not installed!", new Vector3(1, 0, 0));

        ImGui.Text("> Status:");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(statusColor.X, statusColor.Y, statusColor.Z, 1), statusText);

        ImGui.Separator();
        
        ImGui.BeginDisabled(!hasDotnetSdkInstalled);

        ImGui.BeginDisabled(building);

        ImGui.Text("Directory:");

        ImGui.SameLine();

        ImGui.SetNextItemWidth(500);

        ImGui.InputText("##inputdir", ref buildDirectory, 800);

        ImGui.SameLine();

        if (ImGui.Button("Choose")) choosingDirectory = true;

        ImGui.Text("Platform:");

        ImGui.SameLine();

        ImGui.SetNextItemWidth(200);

        if (ImGui.BeginCombo("##platformcombo", availablePlatforms[platform]))
        {
            for (int i = 0; i < availablePlatforms.Length; i++)
            {
                if (ImGui.Selectable(availablePlatforms[i], platform == i)) platform = i;
                if (platform == i) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        ImGui.Text("Self Contained:");

        ImGui.SameLine();

        ImGui.Checkbox("##selfcontained", ref selfContained);

        ImGui.BeginDisabled(!Directory.Exists(buildDirectory));
        if (ImGui.Button("Start Building")) StartBuildingAsync();
        ImGui.EndDisabled();

        ImGui.EndDisabled();

        ImGui.EndDisabled();

        ImGui.End();

        if (choosingDirectory) FileDialog.Show(ref choosingDirectory, ref buildDirectory, false);
    }
    
    public async static void StartBuildingAsync() => await Task.Run(StartBuilding);

    public static void StartBuilding()
    {
        // initialize
        building = true;
        SetStatus("Initializing...");

        // delete existing directory children
        var files = Directory.GetFiles(buildDirectory);
        for (int i = 0; i < files.Length; i++) File.Delete(files[i]);
        var dirs = Directory.GetDirectories(buildDirectory);
        for (int i = 0; i < dirs.Length; i++) Directory.Delete(dirs[i], true);

        // move script assembly dll to build dir
        SetStatus("Compiling scripts...");
        var dllbytes = ScriptManager.RecompileScripts(ProjectManager.projectRoot);
        if (dllbytes == null)
        {
            SetStatus("Script compilation failed", new Vector3(1, 0, 0));
            building = false;
            return;
        }
        File.WriteAllBytes(Path.Combine(buildDirectory, "Scripts.dll"), dllbytes);

        // move game assets to build directory
        SetStatus("Copying game data...");
        CopyDirectory(ProjectManager.projectRoot, Path.Combine(buildDirectory, "_Resources/GameData"));

        // copy player pre build files
        SetStatus("Building player...");
        if (platform == 0) BuildPlayer();
        if (platform == 1) BuildPlayer();

        // finalize
        building = false;
        SetStatus("Finished building.");
        OpenDirectoryInFileExplorer(buildDirectory);
    }

    public static void BuildPlayer()
    {
        string csproj = Path.GetFullPath("_Resources/SourceForGameBuilding/Player/Player.csproj");

        string rid = "";
        if (platform == 0) rid = "win-x64";
        if (platform == 1) rid = "linux-x64";

        string sc = "";
        sc = selfContained ? "true" : "false";

        string args = $"publish {csproj} -o {buildDirectory} -r {rid} -c release --sc {sc}";

        Shell.Run("dotnet", args, out string output, out string errors);

        Debug.Log(output);
        if (!string.IsNullOrWhiteSpace(errors)) Debug.Log("Errors: " + errors);
    }

    public static void CopyDirectory(string source, string dest)
    {
        // ensure existence
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException($"directory not found: {source}");

        // create destination
        Directory.CreateDirectory(dest);

        // copy files
        foreach (string file in Directory.GetFiles(source))
        {
            string destFile = Path.Combine(dest, Path.GetFileName(file));
            File.Copy(file, destFile);
        }

        // copy dirs
        foreach (string subdir in Directory.GetDirectories(source))
        {
            string dubdirdest = Path.Combine(dest, Path.GetFileName(subdir));
            CopyDirectory(subdir, dubdirdest);
        }
    }

    public static void OpenDirectoryInFileExplorer(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start("explorer.exe", path);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", path);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", path);
    }

    private static void SetStatus(string status)
    {
        statusText = status;
        statusColor = Vector3.One;
    }

    private static void SetStatus(string status, Vector3 color)
    {
        statusText = status;
        statusColor = color;
    }
}