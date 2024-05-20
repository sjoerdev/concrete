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
        Console.WriteLine("entry found");
        new Game();
    }
}

class Game
{
    private static IWindow window;
    private static GL opengl;

    public Scene activeScene = null;
    public List<Scene> scenes = [];

    public Game()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "untitled";
        window = Window.Create(options);
        
        window.Load += Start;
        window.Render += Update;
        window.FramebufferResize += Resize;

        window.Run();
        window.Dispose();
    }

    public void Start()
    {
        opengl = GL.GetApi(window);
        // LoadScene("scene.xml");
    }

    public void Update(double deltaTime)
    {
        if (activeScene != null) activeScene.Update((float)deltaTime);
        opengl.Clear(ClearBufferMask.ColorBufferBit);
        opengl.ClearColor(System.Drawing.Color.CornflowerBlue);
    }

    public void Resize(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }

    private void LoadScene(string path)
    {
        var scene = new Scene(path);
        scenes.Add(scene);
        activeScene = scene;
        activeScene.Start();
    }
}