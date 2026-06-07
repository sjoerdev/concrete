using System.Drawing;
using System.Numerics;

using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.ImGui.Backends.SDL3;
using Hexa.NET.ImGuizmo;
using Hexa.NET.ImPlot;

using Silk.NET.OpenGL;

namespace Concrete;

public static unsafe class Editor
{
    static IPlatform platform;
    static PlatformSDL3 platform_sdl3 => platform as PlatformSDL3;

    static void Main()
    {
        platform = new PlatformSDL3();

        platform.Initialize(new Vector2(1600, 900), "Concrete Player");

        platform.SubscribeStart(StartWindow);
        platform.SubscribeUpdate(UpdateWindow);
        platform.SubscribeRender(RenderWindow);
        platform.SubscribeResize(ResizeWindow);
        platform.SubscribeFileDrop(FileDrop);
        
        // connect imgui sdl backend to sdl events
        platform_sdl3.SubscribeExtraSDLEvent(eventPtr => ImGuiImplSDL3.ProcessEvent((SDLEvent*)eventPtr));

        platform.Run();
    }

    static void StartWindow()
    {
        // imgui contexts
        var guiContext = ImGui.CreateContext();
        var plotContext = ImPlot.CreateContext();
        ImGui.SetCurrentContext(guiContext);
        ImGuizmo.SetImGuiContext(guiContext);
        ImPlot.SetImGuiContext(guiContext);
        ImPlot.SetCurrentContext(plotContext);

        // imgui flags
        ImGui.GetIO().ConfigFlags = ImGuiConfigFlags.DockingEnable;
        ImGui.GetIO().Handle->IniFilename = null;

        // imgui backends
        ImGuiImplSDL3.SetCurrentContext(guiContext);
        ImGuiImplSDL3.InitForOpenGL(new((SDLWindow*)platform_sdl3.Get_SDL_Window_Ptr()), (void*)platform_sdl3.Get_SDL_GLContext_Ptr());
        ImGuiImplOpenGL3.SetCurrentContext(guiContext);
        ImGuiImplOpenGL3.Init((byte*)null);

        // setup imgui controller and styling
        EditorStyleChanger.ClearFonts();
        EditorStyleChanger.AddFont(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Resources", "cascadia.ttf"), 14);
        EditorStyleChanger.AddFont(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Resources", "fontawesome_free_solid.otf"), 14, true);
        EditorStyleChanger.SetupCustomTheme();
        
        // imgui display scaling
        ImGui.GetStyle().FontScaleDpi = platform.GetDisplayScalingFactor();

        // load concrete project
        if (File.Exists(ProjectManager.lastProjectMemoryPath))
        {
            Debug.Log("Last project memory file found.");
            string lastOpenedProjectPath = File.ReadAllText(ProjectManager.lastProjectMemoryPath);

            if (File.Exists(lastOpenedProjectPath))
            {
                Debug.Log("Last project memory file contained a valid project.");
                string lastProjectRoot = Directory.GetParent(lastOpenedProjectPath).ToString();

                // compile in memory so the types inside the scripts are known before deserializing the scenes in the project
                ScriptManager.cachedAssembly = null;
                var dllbytes = ScriptManager.RecompileScripts(lastProjectRoot);

                // load the project and deserialize its main scene file
                ProjectManager.LoadProjectDir(lastProjectRoot);
            }
        }
        else
        {
            Debug.Log("No last project memory file found.");
            ProjectManager.CreateAndLoadTempProject();
        }
    }

    static void UpdateWindow(float deltaTime)
    {
        Metrics.Update(deltaTime);
        if (SceneManager.playState == PlayState.playing) SceneManager.UpdateSceneObjects(deltaTime);
    }

    static void RenderWindow(float deltaTime)
    {
        platform.GetGL().Enable(EnableCap.DepthTest);
        platform.GetGL().Enable(EnableCap.Blend);
        platform.GetGL().BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        platform.GetGL().ClearColor(Color.Black);
        platform.GetGL().Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        ImGuiImplOpenGL3.NewFrame();
        ImGuiImplSDL3.NewFrame();
        ImGui.NewFrame();
        ImGuizmo.BeginFrame();

        Render(deltaTime);

        ImGui.Render();
        ImGui.EndFrame();
        ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());
    }

    static void ResizeWindow(Vector2 size)
    {
        platform.GetGL().Viewport(new Size((int)size.X, (int)size.Y));
    }

    static void FileDrop(string[] paths)
    {
        Debug.Log(FilesWindow.hovered.ToString());
        if (!FilesWindow.hovered) return;
        foreach (var path in paths)
        {
            // calculate destination parent
            var parent = ProjectManager.projectRoot;
            var hovered = FilesWindow.hoveredFileOrDir;
            if (hovered != null)
            {
                if (Directory.Exists(hovered)) parent = Path.GetFullPath(hovered);
                if (File.Exists(hovered)) parent = Path.GetDirectoryName(Path.GetFullPath(hovered));
            }

            if (Directory.Exists(path))
            {
                // move dir to project dir
                var dirname = Path.GetFileName(path);
                var destination = Path.Combine(parent, dirname);
                Directory.Move(path, destination);
                
                // rebuild asset database
                AssetDatabase.Rebuild();
            }
            else if (File.Exists(path))
            {
                // move file to project dir
                var filename = Path.GetFileName(path);
                var destination = Path.Combine(parent, filename);
                Directory.Move(path, destination);
                
                // rebuild asset database
                AssetDatabase.Rebuild();
            }
        }
    }

    private static bool dockbuilderInitialized = false;

    public static void Render(float deltaTime)
    {
        SetupDockSpace();
        MainMenuBar.Draw(deltaTime);
        SceneWindow.Draw(deltaTime);
        GameWindow.Draw(deltaTime);
        HierarchyWindow.Draw(deltaTime);
        FilesWindow.Draw(deltaTime);
        ConsoleWindow.Draw(deltaTime);
        InspectorWindow.Draw(deltaTime);
        MetricsWindow.Draw(deltaTime);
        BuildWindow.Draw(deltaTime);
    }

    private static void SetupDockSpace()
    {
        uint dockspace = ImGui.DockSpaceOverViewport((ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.NoWindowMenuButton);
        if (!dockbuilderInitialized)
        {
            uint left, mid, right;
            uint topleft, lowleft;
            uint topmid, lowmid;

            ImGuiP.DockBuilderSplitNode(dockspace, ImGuiDir.Left, 0.25f, &left, &mid);
            ImGuiP.DockBuilderSplitNode(mid, ImGuiDir.Left, 0.66f, &mid, &right);
            ImGuiP.DockBuilderSplitNode(left, ImGuiDir.Up, 0.5f, &topleft, &lowleft);
            ImGuiP.DockBuilderSplitNode(mid, ImGuiDir.Down, 0.3f, &lowmid, &topmid);

            ImGuiP.DockBuilderDockWindow("\uf009 Scene", topmid);
            ImGuiP.DockBuilderDockWindow("\uf11b Game", topmid);
            ImGuiP.DockBuilderDockWindow("\uf552 Build", topmid);
            ImGuiP.DockBuilderDockWindow("\ue473 Metrics", topmid);
            ImGuiP.DockBuilderDockWindow("\uf0ca Hierarchy", topleft);
            ImGuiP.DockBuilderDockWindow("\uf07c Files", lowleft);
            ImGuiP.DockBuilderDockWindow("\uf002 Inspector", right);
            ImGuiP.DockBuilderDockWindow("\uf120 Console", lowmid);

            ImGuiP.DockBuilderFinish(dockspace);
            dockbuilderInitialized = true;
        }
    }
}