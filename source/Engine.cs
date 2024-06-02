using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Numerics;

namespace GameEngine;

class Engine
{
    public static GL opengl;
    public static IWindow window;
    public static IInputContext input;
    
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

        new Scene().SetActive();

        var camera = new GameObject();
        camera.AddComponent<Camera>().SetActive();
        camera.AddComponent<SpotLight>();

        var model = new GameObject();
        model.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";
        
        activeScene?.Start();
    }

    public void Update(double deltaTime)
    {
        activeScene?.Update((float)deltaTime);
    }

    public void Render(double deltaTime)
    {
        opengl.Enable(EnableCap.DepthTest);
        opengl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        opengl.ClearColor(System.Drawing.Color.DarkGray);
        activeScene?.Render((float)deltaTime);
    }

    public void Resize(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}