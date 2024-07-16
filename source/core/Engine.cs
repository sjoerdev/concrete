using System.Numerics;
using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;

namespace GameEngine;

public static class Engine
{
    public static GL opengl;
    public static IWindow window;
    public static IInputContext input;
    public static ImGuiController igcontroller;
    public static SceneManager sceneManager;
    public static Editor editor;

    static void Main()
    {
        // setup window and callbacks
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "untitled";
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
        // create important objects
        opengl = GL.GetApi(window);
        input = window.CreateInput();
        igcontroller = new ImGuiController(opengl, window, input);
        sceneManager = new SceneManager();
        editor = new Editor();

        // load debug scene --------

        var debugScene = new Scene();
        sceneManager.LoadScene(debugScene);

        var cameraObject = new GameObject();
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<SpotLight>();
        cameraObject.transform.localPosition = new Vector3(0, 1, -2);

        var firstModel = new GameObject();
        firstModel.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";

        var secondModel = new GameObject();
        secondModel.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";
        secondModel.transform.localPosition = new Vector3(1, 1, 0);
        secondModel.transform.parent = firstModel.transform;
    }

    static void UpdateWindow(double deltaTime)
    {
        sceneManager.TryUpdate((float)deltaTime);
        editor.Update((float)deltaTime);
        igcontroller.Update((float)deltaTime);
    }

    static void RenderWindow(double deltaTime)
    {
        opengl.Enable(EnableCap.DepthTest);
        opengl.ClearColor(Color.Black);
        opengl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        editor.Render((float)deltaTime);
        igcontroller.Render();
    }

    static void ResizeWindow(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}