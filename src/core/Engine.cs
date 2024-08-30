using System.Numerics;
using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;

namespace Concrete;

public static class Engine
{
    public static GL opengl;
    public static IWindow window;
    public static IInputContext input;
    public static ImGuiController igcontroller;

    static void Main()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "Concrete";
        window = Window.Create(options);
        window.Load += StartWindow;
        window.Update += UpdateWindow;
        window.Render += RenderWindow;
        window.FramebufferResize += ResizeWindow;
        window.Run();
        window.Dispose();
    }

    static void StartWindow()
    {
        // instance important objects
        opengl = GL.GetApi(window);
        input = window.CreateInput();
        igcontroller = new ImGuiController(opengl, window, input);

        // load debug scene --------

        SceneManager.LoadScene(new Scene());

        var camera = GameObject.Create();
        camera.AddComponent<Camera>();
        camera.transform.localPosition = new Vector3(0, 1, -2);
        camera.name = "Camera";

        var robot = GameObject.Create();
        robot.AddComponent<MeshRenderer>().modelPath = "res/models/robot.glb";
        robot.transform.localPosition = new Vector3(-1, 0, 0);
        robot.name = "Robot Model";
        
        var technic = GameObject.Create();
        technic.AddComponent<MeshRenderer>().modelPath = "res/models/technic.glb";
        technic.transform.localPosition = new Vector3(1, 0, 0);
        technic.transform.localScale = Vector3.One * 0.0005f;
        technic.name = "Technic Model";

        var light = GameObject.Create();
        light.AddComponent<DirectionalLight>();
        light.transform.localEulerAngles = new Vector3(20, 135, 0);
        light.name = "Directional Light";
    }

    static void UpdateWindow(double deltaTime)
    {
        Metrics.Update((float)deltaTime);
        if (SceneManager.playState == PlayState.playing) SceneManager.UpdateScene((float)deltaTime);
        Editor.Update((float)deltaTime);
        igcontroller.Update((float)deltaTime);
    }

    static void RenderWindow(double deltaTime)
    {
        opengl.Enable(EnableCap.DepthTest);
        opengl.ClearColor(Color.Black);
        opengl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Editor.Render((float)deltaTime);
        igcontroller.Render();
    }

    static void ResizeWindow(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}