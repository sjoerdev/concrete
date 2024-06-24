using System.Numerics;
using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;

namespace GameEngine;

unsafe class Engine
{
    public static GL opengl;
    public static IWindow window;
    public static IInputContext input;
    public static ImGuiController controller;
    public static Framebuffer framebuffer;
    public static Editor editor;
    
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
        controller = new ImGuiController(opengl, window, input);
        framebuffer = new Framebuffer();
        editor = new Editor();

        // debug scene --------

        new Scene().SetActive();

        var camera = new GameObject();
        camera.AddComponent<Camera>().SetActive();
        camera.AddComponent<SpotLight>();
        camera.transform.localPosition = new Vector3(0, 1, -2);
        camera.name = "Camera";

        var firstModel = new GameObject();
        firstModel.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";
        firstModel.name = "Model (1)";

        var secondModel = new GameObject();
        secondModel.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";
        secondModel.transform.localPosition = new Vector3(1, 1, 0);
        secondModel.transform.parent = firstModel.transform;
        secondModel.name = "Model (2)";
        
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
        editor.Render((float)deltaTime);
        controller.Render();
    }

    public void Resize(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}