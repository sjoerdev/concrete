using System.Diagnostics;
using System.Runtime.InteropServices;

using Hexa.NET.ImGui;

namespace Concrete;

public static unsafe class MainMenuBar
{
    private static bool openSceneDialog = false;
    private static bool saveSceneDialog = false;

    private static bool openProjectDirDialog = false;
    private static bool saveProjectDirDialog = false;

    private static string fileDialogPath = "";
    
    public static void Draw(float deltaTime)
    {
        if (saveProjectDirDialog) FileDialog.Show(ref saveProjectDirDialog, ref fileDialogPath, false, () => ProjectManager.SaveProjectDir(fileDialogPath));
        if (openProjectDirDialog) FileDialog.Show(ref openProjectDirDialog, ref fileDialogPath, false, () => ProjectManager.LoadProjectDir(fileDialogPath));

        if (saveSceneDialog) FileDialog.Show(ref saveSceneDialog, ref fileDialogPath, true, () => SceneManager.SaveScene(fileDialogPath));
        if (openSceneDialog) FileDialog.Show(ref openSceneDialog, ref fileDialogPath, true, () => SceneManager.LoadScene(fileDialogPath));

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                ImGui.Spacing();

                if (ImGui.MenuItem("New Project")) ProjectManager.CreateAndLoadTempProject();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.BeginDisabled(ProjectManager.loadedProjectData == null);
                if (ImGui.MenuItem("Save Project")) saveProjectDirDialog = true;
                ImGui.EndDisabled();
                if (ImGui.MenuItem("Open Project")) openProjectDirDialog = true;

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                if (ImGui.MenuItem("New Scene")) SceneManager.CreateAndLoadNewScene();
                ImGui.BeginDisabled(Scene.Current == null);
                if (ImGui.MenuItem("Save Scene")) saveSceneDialog = true;
                ImGui.EndDisabled();
                if (ImGui.MenuItem("Open Scene")) openSceneDialog = true;

                ImGui.Spacing();
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("Open engine repository")) OpenLink("https://github.com/sjoerdev/concrete");
                ImGui.EndMenu();

                void OpenLink(string url)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start(new ProcessStartInfo(url) { FileName = url, UseShellExecute = true, });
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", url);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", url);
                }
            }
            
            ImGui.EndMainMenuBar();
        }
    }
}