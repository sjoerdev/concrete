using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Concrete;

public abstract class IPlatform
{
    public static IPlatform Current;

    public abstract GL GetGL();

    public abstract void Initialize(Vector2 windowSize, string windowTitle);

    public abstract void Run();

    public abstract void SubscribeStart(Action action);

    public abstract void SubscribeUpdate(Action<float> action);

    public abstract void SubscribeRender(Action<float> action);

    public abstract void SubscribeResize(Action<Vector2> action);

    public abstract void SubscribeFileDrop(Action<string[]> action);

    public abstract float GetDisplayScalingFactor();

    public abstract bool IsKeyPressed(PlatformKey platformKey);

    public abstract bool IsMouseButtonPressed(int button);

    public abstract Vector2 GetMousePosition();

    public abstract Vector2 GetWindowSize();

    public abstract void SetWindowTitle(string title);
}