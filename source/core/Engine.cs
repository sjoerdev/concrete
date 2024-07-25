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
        // instance important objects
        opengl = GL.GetApi(window);
        input = window.CreateInput();
        igcontroller = new ImGuiController(opengl, window, input);

        // load debug scene --------

        SceneManager.LoadScene(new Scene());

        var cameraObject = GameObject.Create();
        cameraObject.AddComponent<Camera>();
        cameraObject.transform.localPosition = new Vector3(0, 1, -2);
        cameraObject.name = "Camera";

        var firstModel = GameObject.Create();
        firstModel.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";
        firstModel.name = "Model 1";

        var secondModel = GameObject.Create();
        secondModel.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";
        secondModel.transform.localPosition = new Vector3(1, 1, 0);
        secondModel.transform.parent = firstModel.transform;
        secondModel.name = "Model 2";

        var thirdModel = GameObject.Create();
        thirdModel.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";
        thirdModel.transform.localPosition = new Vector3(-1, 0, 0);
        thirdModel.transform.parent = secondModel.transform;
        thirdModel.name = "Model 3";

        var lightObject = GameObject.Create();
        lightObject.AddComponent<DirectionalLight>();
        lightObject.transform.localEulerAngles = new Vector3(20, 135, 0);
        lightObject.name = "Light";
    }

    static void UpdateWindow(double deltaTime)
    {
        Metrics.Update((float)deltaTime);
        SceneManager.TryUpdate((float)deltaTime);
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