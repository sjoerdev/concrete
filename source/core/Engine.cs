using System.Numerics;
using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Hexa.NET.ImGui;

namespace GameEngine;

unsafe class Engine
{
    public static GL opengl;
    public static IWindow window;
    public static IInputContext input;
    public static ImGuiController controller;
    
    public static Scene activeScene = null;
    public static List<Scene> scenes = [];

    public static Camera activeCamera = null;
    public static Framebuffer framebuffer = null;

    public static List<DirectionalLight> directionalLights = [];
    public static List<PointLight> pointLights = [];
    public static List<SpotLight> spotLights = [];

    bool dockbuilderInitialized = false;

    public Engine()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "untitled";
        window = Window.Create(options);
        window.Load += Start;
        window.Update += Update;
        window.Render += Render;
        window.FramebufferResize += Resize;
        window.Run();
        window.Dispose();
    }

    public void Start()
    {
        opengl = GL.GetApi(window);
        input = window.CreateInput();
        controller = new ImGuiController(opengl, window, input);
        framebuffer = new Framebuffer();
        
        ImGui.GetIO().Handle->IniFilename = null;
        ImGui.GetIO().ConfigFlags = ImGuiConfigFlags.DockingEnable;

        new Scene().SetActive();

        var camera = new GameObject();
        camera.AddComponent<Camera>().SetActive();
        camera.AddComponent<SpotLight>();
        camera.transform.position = new Vector3(0, 1, -2);

        var model = new GameObject();
        model.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";
        
        activeScene?.Start();
    }

    public void Update(double deltaTime)
    {
        activeScene?.Update((float)deltaTime);
        controller.Update((float)deltaTime);
    }

    public void Render(double deltaTime)
    {
        opengl.Enable(EnableCap.DepthTest);
        opengl.ClearColor(Color.Black);
        opengl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        int dockspace = ImGui.DockSpaceOverViewport();
        if (!dockbuilderInitialized)
        {
            int leftdock, rightdock = 0;
            ImGui.DockBuilderSplitNode(dockspace, ImGuiDir.Left, 0.25f, &leftdock, &rightdock);
            int topleftdock, bottomleftdock = 0;
            ImGui.DockBuilderSplitNode(leftdock, ImGuiDir.Up, 0.5f, &bottomleftdock, &topleftdock);
            ImGui.DockBuilderDockWindow("Scene", rightdock);
            ImGui.DockBuilderDockWindow("Hierarchy", topleftdock);
            ImGui.DockBuilderDockWindow("Inspector", bottomleftdock);
            ImGui.DockBuilderFinish(dockspace);
            dockbuilderInitialized = true;
        }
        
        ImGui.Begin("Scene", ImGuiWindowFlags.NoScrollbar);
        framebuffer.Resize(ImGui.GetContentRegionAvail());
        framebuffer.Enable();
        framebuffer.Clear(Color.DarkGray);
        activeScene?.Render((float)deltaTime);
        framebuffer.Disable();
        ImGui.Image((nint)framebuffer.colorTexture, framebuffer.size, Vector2.UnitY, Vector2.UnitX);
        ImGui.End();

        ImGui.Begin("Hierarchy");
        ImGui.End();

        ImGui.Begin("Inspector");
        ImGui.End();

        controller.Render();
    }

    public void Resize(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}