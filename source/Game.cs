using System;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;

namespace Project;

class Entry
{
    static void Main()
    {
        new Game();
    }
}

class Game
{
    public static IWindow window;
    public static GL opengl;
    public static Scene activeScene = null;
    public static List<Scene> scenes = [];

    public Game()
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

        var scene = new Scene();

        var cam = scene.CreateGameObject();
        cam.AddComponent(new Camera());

        var gameObject = scene.CreateGameObject();
        gameObject.AddComponent(new MeshRenderer("resources/models/suzanne.obj"));

        scene.Activate();

        if (activeScene != null) activeScene.Start();
    }

    public void Update(double deltaTime)
    {
        if (activeScene != null) activeScene.Update((float)deltaTime);
    }

    public void Render(double deltaTime)
    {
        opengl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        opengl.ClearColor(System.Drawing.Color.CornflowerBlue);

        if (activeScene != null) activeScene.Render((float)deltaTime);
        
        window.SwapBuffers();
    }

    public void Resize(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}