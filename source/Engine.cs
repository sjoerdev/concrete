using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using ImGuiNET;

namespace GameEngine;

class Engine
{
    public static GL opengl;
    public static IWindow window;
    public static IInputContext input;
    public static ImGuiController imgui;
    
    public static Scene activeScene = null;
    public static List<Scene> scenes = [];

    public static Camera activeCamera = null;

    public static List<DirectionalLight> directionalLights = [];
    public static List<PointLight> pointLights = [];
    public static List<SpotLight> spotLights = [];

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
        imgui = new ImGuiController(opengl, window, input);

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
        imgui.Update((float)deltaTime);
    }

    public void Render(double deltaTime)
    {
        opengl.Enable(EnableCap.DepthTest);
        opengl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        opengl.ClearColor(System.Drawing.Color.DarkGray);
        activeScene?.Render((float)deltaTime);

        ImGui.SetNextWindowPos(new Vector2(8, 8), ImGuiCond.Once);
        ImGui.ShowDemoWindow();
        imgui.Render();
    }

    public void Resize(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}