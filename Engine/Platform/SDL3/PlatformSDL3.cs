using Silk.NET.OpenGL;
using SDL3;
using System.Numerics;

namespace Concrete;

public unsafe class PlatformSDL3 : IPlatform
{
    private GL opengl = null;

    private nint window;
    private nint glContext;

    private Action startCallback;
    private Action<float> updateCallback;
    private Action<float> renderCallback;
    private Action<Vector2> resizeCallback;
    private Action<string[]> fileDropCallback;
    private Action<nint> SDLEventCallbacks;

    private bool[] keyStates = new bool[512];
    private bool[] mouseButtonStates = new bool[8];
    private Vector2 mousePosition;

    private string[] pendingDroppedFiles = null;
    private ulong lastTime;
    private bool isRunning;

    private void ProcessSDLEvents()
    {
        var MapSDLMouseButton = (byte sdlButton) =>
        {
            if (sdlButton == 1) return 0;
            if (sdlButton == 3) return 1;
            if (sdlButton == 2) return 2;
            else return -1;
        };

        SDL.Event sdlEvent;
        while (SDL.PollEvent(out sdlEvent) != false)
        {
            SDLEventCallbacks?.Invoke((nint)(&sdlEvent));

            var eventType = (SDL.EventType)sdlEvent.Type;

            if (eventType == SDL.EventType.Quit)
            {
                isRunning = false;
            }
            else if (eventType == SDL.EventType.WindowResized)
            {
                SDL.GetWindowSize(window, out int width, out int height);
                resizeCallback?.Invoke(new Vector2(width, height));
            }
            else if (eventType == SDL.EventType.KeyDown)
            {
                if ((int)sdlEvent.Key.Scancode < keyStates.Length)
                {
                    keyStates[(int)sdlEvent.Key.Scancode] = true;
                }
            }
            else if (eventType == SDL.EventType.KeyUp)
            {
                if ((int)sdlEvent.Key.Scancode < keyStates.Length)
                {
                    keyStates[(int)sdlEvent.Key.Scancode] = false;
                }
            }
            else if (eventType == SDL.EventType.MouseButtonDown)
            {
                int mappedButton = MapSDLMouseButton(sdlEvent.Button.Button);
                if (mappedButton >= 0 && mappedButton < mouseButtonStates.Length)
                {
                    mouseButtonStates[mappedButton] = true;
                }
            }
            else if (eventType == SDL.EventType.MouseButtonUp)
            {
                int mappedButton = MapSDLMouseButton(sdlEvent.Button.Button);
                if (mappedButton >= 0 && mappedButton < mouseButtonStates.Length)
                {
                    mouseButtonStates[mappedButton] = false;
                }
            }
            else if (eventType == SDL.EventType.MouseMotion)
            {
                mousePosition = new Vector2(sdlEvent.Motion.X, sdlEvent.Motion.Y);
            }
            else if (eventType == SDL.EventType.DropFile)
            {
                if (fileDropCallback != null)
                {
                    var path = new String((sbyte*)sdlEvent.Drop.Data);
                    pendingDroppedFiles = [path];
                }
            }
        }
    }

    private int PlatformKeyToSDLKey(PlatformKey key)
    {
        return key switch
        {
            PlatformKey.A => 4,
            PlatformKey.B => 5,
            PlatformKey.C => 6,
            PlatformKey.D => 7,
            PlatformKey.E => 8,
            PlatformKey.F => 9,
            PlatformKey.G => 10,
            PlatformKey.H => 11,
            PlatformKey.I => 12,
            PlatformKey.J => 13,
            PlatformKey.K => 14,
            PlatformKey.L => 15,
            PlatformKey.M => 16,
            PlatformKey.N => 17,
            PlatformKey.O => 18,
            PlatformKey.P => 19,
            PlatformKey.Q => 20,
            PlatformKey.R => 21,
            PlatformKey.S => 22,
            PlatformKey.T => 23,
            PlatformKey.U => 24,
            PlatformKey.V => 25,
            PlatformKey.W => 26,
            PlatformKey.X => 27,
            PlatformKey.Y => 28,
            PlatformKey.Z => 29,
            PlatformKey.Number1 => 30,
            PlatformKey.Number2 => 31,
            PlatformKey.Number3 => 32,
            PlatformKey.Number4 => 33,
            PlatformKey.Number5 => 34,
            PlatformKey.Number6 => 35,
            PlatformKey.Number7 => 36,
            PlatformKey.Number8 => 37,
            PlatformKey.Number9 => 38,
            PlatformKey.Number0 => 39,
            PlatformKey.Enter => 40,
            PlatformKey.Escape => 41,
            PlatformKey.Backspace => 42,
            PlatformKey.Tab => 43,
            PlatformKey.Space => 44,
            PlatformKey.Minus => 45,
            PlatformKey.Equal => 46,
            PlatformKey.LeftBracket => 47,
            PlatformKey.RightBracket => 48,
            PlatformKey.BackSlash => 49,
            PlatformKey.Semicolon => 51,
            PlatformKey.Apostrophe => 52,
            PlatformKey.GraveAccent => 53,
            PlatformKey.Comma => 54,
            PlatformKey.Period => 55,
            PlatformKey.Slash => 56,
            PlatformKey.CapsLock => 57,
            PlatformKey.F1 => 58,
            PlatformKey.F2 => 59,
            PlatformKey.F3 => 60,
            PlatformKey.F4 => 61,
            PlatformKey.F5 => 62,
            PlatformKey.F6 => 63,
            PlatformKey.F7 => 64,
            PlatformKey.F8 => 65,
            PlatformKey.F9 => 66,
            PlatformKey.F10 => 67,
            PlatformKey.F11 => 68,
            PlatformKey.F12 => 69,
            PlatformKey.PrintScreen => 70,
            PlatformKey.ScrollLock => 71,
            PlatformKey.Pause => 72,
            PlatformKey.Insert => 73,
            PlatformKey.Home => 74,
            PlatformKey.PageUp => 75,
            PlatformKey.Delete => 76,
            PlatformKey.End => 77,
            PlatformKey.PageDown => 78,
            PlatformKey.Right => 79,
            PlatformKey.Left => 80,
            PlatformKey.Down => 81,
            PlatformKey.Up => 82,
            PlatformKey.NumLock => 83,
            PlatformKey.KeypadDivide => 84,
            PlatformKey.KeypadMultiply => 85,
            PlatformKey.KeypadSubtract => 86,
            PlatformKey.KeypadAdd => 87,
            PlatformKey.KeypadEnter => 88,
            PlatformKey.Keypad1 => 89,
            PlatformKey.Keypad2 => 90,
            PlatformKey.Keypad3 => 91,
            PlatformKey.Keypad4 => 92,
            PlatformKey.Keypad5 => 93,
            PlatformKey.Keypad6 => 94,
            PlatformKey.Keypad7 => 95,
            PlatformKey.Keypad8 => 96,
            PlatformKey.Keypad9 => 97,
            PlatformKey.Keypad0 => 98,
            PlatformKey.KeypadDecimal => 99,
            PlatformKey.KeypadEqual => 103,
            PlatformKey.F13 => 104,
            PlatformKey.F14 => 105,
            PlatformKey.F15 => 106,
            PlatformKey.F16 => 107,
            PlatformKey.F17 => 108,
            PlatformKey.F18 => 109,
            PlatformKey.F19 => 110,
            PlatformKey.F20 => 111,
            PlatformKey.F21 => 112,
            PlatformKey.F22 => 113,
            PlatformKey.F23 => 114,
            PlatformKey.F24 => 115,
            PlatformKey.ControlLeft => 224,
            PlatformKey.ShiftLeft => 225,
            PlatformKey.AltLeft => 226,
            PlatformKey.SuperLeft => 227,
            PlatformKey.ControlRight => 228,
            PlatformKey.ShiftRight => 229,
            PlatformKey.AltRight => 230,
            PlatformKey.SuperRight => 231,
            PlatformKey.Menu => 118,
            _ => -1
        };
    }

    #region SDL_Specific_Publics

    public nint Get_SDL_Window_Ptr() => window;

    public nint Get_SDL_GLContext_Ptr() => glContext;

    public void SubscribeExtraSDLEvent(Action<nint> action) => SDLEventCallbacks += action;

    #endregion

    #region Platform_Implementation

    override public GL GetGL()
    {
        var gl = opengl;
        return gl;
    }

    override public void Initialize(Vector2 windowSize, string windowTitle)
    {
        // make this platform the current active one
        Current ??= this;

        // Initialize SDL
        SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Events);

        // Set OpenGL attributes
        SDL.GLSetAttribute(SDL.GLAttr.ContextMajorVersion, 3);
        SDL.GLSetAttribute(SDL.GLAttr.ContextMinorVersion, 3);
        SDL.GLSetAttribute(SDL.GLAttr.ContextProfileMask, (int)ContextProfileMask.CoreProfileBit);
        SDL.GLSetAttribute(SDL.GLAttr.DoubleBuffer, 1);

        // Create window
        window = SDL.CreateWindow(
            windowTitle,
            (int)windowSize.X,
            (int)windowSize.Y,
            SDL.WindowFlags.OpenGL | SDL.WindowFlags.Resizable
        );

        // Create OpenGL context
        glContext = SDL.GLCreateContext(window);
        SDL.GLMakeCurrent(window, glContext);
        SDL.GLSetSwapInterval(1);

        lastTime = SDL.GetPerformanceCounter();
    }

    override public void Run()
    {
        // Call start callback
        opengl = GL.GetApi((name) => (nint)SDL.GLGetProcAddress(name));
        startCallback?.Invoke();

        isRunning = true;

        while (isRunning)
        {
            // Calculate delta time
            ulong currentTime = SDL.GetPerformanceCounter();
            float deltaTime = (currentTime - lastTime) / (float)SDL.GetPerformanceFrequency();
            lastTime = currentTime;

            // Process events
            ProcessSDLEvents();

            // Update
            updateCallback?.Invoke(deltaTime);

            // Render
            renderCallback?.Invoke(deltaTime);

            // File Drop
            if (pendingDroppedFiles != null)
            {
                fileDropCallback?.Invoke(pendingDroppedFiles);
                pendingDroppedFiles = null;
            }

            // Swap buffers
            SDL.GLSwapWindow(window);
        }

        // Cleanup
        SDL.GLDestroyContext(glContext);
        SDL.DestroyWindow(window);
        SDL.Quit();
    }

    override public void SubscribeStart(Action action)
    {
        startCallback = action;
    }

    override public void SubscribeUpdate(Action<float> action)
    {
        updateCallback = action;
    }

    override public void SubscribeRender(Action<float> action)
    {
        renderCallback = action;
    }

    override public void SubscribeResize(Action<Vector2> action)
    {
        resizeCallback = action;
    }

    override public void SubscribeFileDrop(Action<string[]> action)
    {
        fileDropCallback = action;
    }

    override public bool IsKeyPressed(PlatformKey platformKey)
    {
        var sdlKey = PlatformKeyToSDLKey(platformKey);
        return sdlKey < keyStates.Length && keyStates[sdlKey];
    }

    override public bool IsMouseButtonPressed(int button)
    {
        return button >= 0 && button < mouseButtonStates.Length && mouseButtonStates[button];
    }

    override public Vector2 GetMousePosition()
    {
        return mousePosition;
    }

    override public float GetDisplayScalingFactor()
    {
        uint displayId = SDL.GetPrimaryDisplay();
        float scale = SDL.GetDisplayContentScale(displayId);
        return scale;
    }

    override public Vector2 GetWindowSize()
    {
        SDL.GetWindowSize(window, out int width, out int height);
        return new Vector2(width, height);
    }

    override public void SetWindowTitle(string title)
    {
        SDL.SetWindowTitle(window, title);
    }

    #endregion
}