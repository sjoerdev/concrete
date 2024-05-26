using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;

namespace GameEngine;

class Engine
{
    public static GL opengl;
    public static IWindow window;
    public static IInputContext input;
    
    public static Scene activeScene = null;
    public static List<Scene> scenes = [];

    public static Camera activeCamera = null;

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

        var scene = new Scene();
        scene.SetActive();

        var cameraGameObject = new GameObject();
        var camera = cameraGameObject.AddComponent<Camera>();
        camera.SetActive();

        var gameObject = new GameObject();
        gameObject.AddComponent<MeshRenderer>().modelPath = "resources/models/suzanne.obj";
        
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
        opengl.ClearColor(System.Drawing.Color.CornflowerBlue);

        activeScene?.Render((float)deltaTime);
        
        window.SwapBuffers();
    }

    public void Resize(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}