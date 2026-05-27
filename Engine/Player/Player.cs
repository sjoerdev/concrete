using System.Reflection;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Concrete;

public static class Player
{
    static IPlatform platform;

    static void Main()
    {
        platform = new PlatformSDL3();

        platform.Initialize(new Vector2(1600, 900), "Concrete Player");

        platform.SubscribeStart(StartWindow);
        platform.SubscribeUpdate(UpdateWindow);
        platform.SubscribeRender(RenderWindow);
        platform.SubscribeResize(ResizeWindow);

        platform.Run();
    }

    static void StartWindow()
    {
        Assembly.LoadFile(Path.GetFullPath("Scripts.dll"));
        ProjectManager.LoadProjectDir("./_Resources/GameData/");
        SceneManager.StartPlaying();
    }

    static void UpdateWindow(float deltaTime)
    {
        Metrics.Update(deltaTime);
        if (SceneManager.playState == PlayState.playing) SceneManager.UpdateSceneObjects(deltaTime);
    }

    static void RenderWindow(float deltaTime)
    {
        platform.GetGL().Enable(EnableCap.DepthTest);
        platform.GetGL().Enable(EnableCap.Blend);
        platform.GetGL().BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        platform.GetGL().ClearColor(Scene.Current.FindCamera().clearColor);
        platform.GetGL().Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        SceneManager.RenderSceneObjects(deltaTime, Scene.Current.FindCamera().view, Scene.Current.FindCamera().proj);
    }

    static void ResizeWindow(Vector2 size)
    {
        platform.GetGL().Viewport(new System.Drawing.Size((int)size.X, (int)size.Y));
    }
}