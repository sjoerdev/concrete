using System.Drawing;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Hexa.NET.ImGui;

public class ImGuiController : IDisposable
{
    public ImGuiContextPtr context;

    private GL opengl;
    private IView view;
    private IInputContext input;
    private IKeyboard keyboard;

    private bool framebegun;
    private readonly List<char> pressedchars = [];

    private int alocTex;
    private int alocProj;
    private int alocPos;
    private int alocUv;
    private int alocColor;

    private uint vao;
    private uint vbo;
    private uint ebo;

    private ImGuiTexture fontTexture;
    private ImGuiShader shader;

    private int width;
    private int height;

    public ImGuiController(GL opengl, IView view, IInputContext input, Action onConfigureIO = null)
    {
        Init(opengl, view, input);
        var io = ImGui.GetIO();
        onConfigureIO?.Invoke();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        CreateDeviceResources();
        SetPerFrameImGuiData(1f / 60f);
        BeginFrame();
    }

    private void Init(GL opengl, IView view, IInputContext input)
    {
        this.opengl = opengl;
        this.view = view;
        this.input = input;
        width = view.Size.X;
        height = view.Size.Y;
        context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        ImGui.StyleColorsDark();
    }

    private void BeginFrame()
    {
        ImGui.NewFrame();
        framebegun = true;
        keyboard = input.Keyboards[0];
        view.Resize += WindowResized;
        keyboard.KeyChar += OnKeyChar;
    }

    private void OnKeyChar(IKeyboard arg1, char arg2)
    {
        pressedchars.Add(arg2);
    }

    private void WindowResized(Vector2D<int> size)
    {
        width = size.X;
        height = size.Y;
    }
    
    public unsafe void Render()
    {
        if (framebegun)
        {
            var oldCtx = ImGui.GetCurrentContext();

            if (oldCtx != context)
            {
                ImGui.SetCurrentContext(context);
            }

            framebegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());

            if (oldCtx != context)
            {
                ImGui.SetCurrentContext(oldCtx);
            }
        }
    }

    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(float deltaSeconds)
    {
        var oldCtx = ImGui.GetCurrentContext();

        if (oldCtx != context)
        {
            ImGui.SetCurrentContext(context);
        }

        if (framebegun)
        {
            ImGui.Render();
        }

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput();

        framebegun = true;
        ImGui.NewFrame();

        if (oldCtx != context)
        {
            ImGui.SetCurrentContext(oldCtx);
        }
    }

    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(width, height);

        if (width > 0 && height > 0)
        {
            io.DisplayFramebufferScale = new Vector2(view.FramebufferSize.X / width,
                view.FramebufferSize.Y / height);
        }

        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    private void UpdateImGuiInput()
    {
        var io = ImGui.GetIO();

        var mouseState = input.Mice[0].CaptureState();
        var keyboardState = input.Keyboards[0].CaptureState();

        io.MouseDown[0] = mouseState.IsButtonPressed(MouseButton.Left);
        io.MouseDown[1] = mouseState.IsButtonPressed(MouseButton.Right);
        io.MouseDown[2] = mouseState.IsButtonPressed(MouseButton.Middle);
        io.MousePos = mouseState.Position;

        var wheel = mouseState.GetScrollWheels()[0];
        io.MouseWheel = wheel.Y;
        io.MouseWheelH = wheel.X;

        foreach (var key in (Key[])Enum.GetValues(typeof(Key)))
        {
            if (key == Key.Unknown) continue;
            io.AddKeyEvent(ToImGuiKey(key), keyboardState.IsKeyPressed(key));
        }

        foreach (var pressed in pressedchars) io.AddInputCharacter(pressed);
        pressedchars.Clear();

        io.KeyCtrl = keyboardState.IsKeyPressed(Key.ControlLeft) || keyboardState.IsKeyPressed(Key.ControlRight);
        io.KeyAlt = keyboardState.IsKeyPressed(Key.AltLeft) || keyboardState.IsKeyPressed(Key.AltRight);
        io.KeyShift = keyboardState.IsKeyPressed(Key.ShiftLeft) || keyboardState.IsKeyPressed(Key.ShiftRight);
        io.KeySuper = keyboardState.IsKeyPressed(Key.SuperLeft) || keyboardState.IsKeyPressed(Key.SuperRight);
    }

    internal void PressChar(char keyChar)
    {
        pressedchars.Add(keyChar);
    }

    private ImGuiKey ToImGuiKey(Key key)
    {
        return key switch
        {
            Key.Tab => ImGuiKey.Tab,
            Key.Left => ImGuiKey.LeftArrow,
            Key.Right => ImGuiKey.RightArrow,
            Key.Up => ImGuiKey.UpArrow,
            Key.Down => ImGuiKey.DownArrow,
            Key.PageUp => ImGuiKey.PageUp,
            Key.PageDown => ImGuiKey.PageDown,
            Key.Home => ImGuiKey.Home,
            Key.End => ImGuiKey.End,
            Key.Insert => ImGuiKey.Insert,
            Key.Delete => ImGuiKey.Delete,
            Key.Backspace => ImGuiKey.Backspace,
            Key.Space => ImGuiKey.Space,
            Key.Enter => ImGuiKey.Enter,
            Key.Escape => ImGuiKey.Escape,
            Key.Apostrophe => ImGuiKey.Apostrophe,
            Key.Comma => ImGuiKey.Comma,
            Key.Minus => ImGuiKey.Minus,
            Key.Period => ImGuiKey.Period,
            Key.Slash => ImGuiKey.Slash,
            Key.Semicolon => ImGuiKey.Semicolon,
            Key.Equal => ImGuiKey.Equal,
            Key.LeftBracket => ImGuiKey.LeftBracket,
            Key.BackSlash => ImGuiKey.Backslash,
            Key.RightBracket => ImGuiKey.RightBracket,
            Key.GraveAccent => ImGuiKey.GraveAccent,
            Key.CapsLock => ImGuiKey.CapsLock,
            Key.ScrollLock => ImGuiKey.ScrollLock,
            Key.NumLock => ImGuiKey.NumLock,
            Key.PrintScreen => ImGuiKey.PrintScreen,
            Key.Pause => ImGuiKey.Pause,
            Key.Keypad0 => ImGuiKey.Keypad0,
            Key.Keypad1 => ImGuiKey.Keypad1,
            Key.Keypad2 => ImGuiKey.Keypad2,
            Key.Keypad3 => ImGuiKey.Keypad3,
            Key.Keypad4 => ImGuiKey.Keypad4,
            Key.Keypad5 => ImGuiKey.Keypad5,
            Key.Keypad6 => ImGuiKey.Keypad6,
            Key.Keypad7 => ImGuiKey.Keypad7,
            Key.Keypad8 => ImGuiKey.Keypad8,
            Key.Keypad9 => ImGuiKey.Keypad9,
            Key.KeypadDecimal => ImGuiKey.KeypadDecimal,
            Key.KeypadDivide => ImGuiKey.KeypadDivide,
            Key.KeypadMultiply => ImGuiKey.KeypadMultiply,
            Key.KeypadSubtract => ImGuiKey.KeypadSubtract,
            Key.KeypadAdd => ImGuiKey.KeypadAdd,
            Key.KeypadEnter => ImGuiKey.KeypadEnter,
            Key.KeypadEqual => ImGuiKey.KeypadEqual,
            Key.ControlLeft => ImGuiKey.LeftCtrl,
            Key.ShiftLeft => ImGuiKey.LeftShift,
            Key.AltLeft => ImGuiKey.LeftAlt,
            Key.SuperLeft => ImGuiKey.LeftSuper,
            Key.ControlRight => ImGuiKey.RightCtrl,
            Key.ShiftRight => ImGuiKey.RightShift,
            Key.AltRight => ImGuiKey.RightAlt,
            Key.SuperRight => ImGuiKey.RightSuper,
            Key.Menu => ImGuiKey.Menu,
            Key.Number0 => ImGuiKey.Key0,
            Key.Number1 => ImGuiKey.Key1,
            Key.Number2 => ImGuiKey.Key2,
            Key.Number3 => ImGuiKey.Key3,
            Key.Number4 => ImGuiKey.Key4,
            Key.Number5 => ImGuiKey.Key5,
            Key.Number6 => ImGuiKey.Key6,
            Key.Number7 => ImGuiKey.Key7,
            Key.Number8 => ImGuiKey.Key8,
            Key.Number9 => ImGuiKey.Key9,
            Key.A => ImGuiKey.A,
            Key.B => ImGuiKey.B,
            Key.C => ImGuiKey.C,
            Key.D => ImGuiKey.D,
            Key.E => ImGuiKey.E,
            Key.F => ImGuiKey.F,
            Key.G => ImGuiKey.G,
            Key.H => ImGuiKey.H,
            Key.I => ImGuiKey.I,
            Key.J => ImGuiKey.J,
            Key.K => ImGuiKey.K,
            Key.L => ImGuiKey.L,
            Key.M => ImGuiKey.M,
            Key.N => ImGuiKey.N,
            Key.O => ImGuiKey.O,
            Key.P => ImGuiKey.P,
            Key.Q => ImGuiKey.Q,
            Key.R => ImGuiKey.R,
            Key.S => ImGuiKey.S,
            Key.T => ImGuiKey.T,
            Key.U => ImGuiKey.U,
            Key.V => ImGuiKey.V,
            Key.W => ImGuiKey.W,
            Key.X => ImGuiKey.X,
            Key.Y => ImGuiKey.Y,
            Key.Z => ImGuiKey.Z,
            Key.F1 => ImGuiKey.F1,
            Key.F2 => ImGuiKey.F2,
            Key.F3 => ImGuiKey.F3,
            Key.F4 => ImGuiKey.F4,
            Key.F5 => ImGuiKey.F5,
            Key.F6 => ImGuiKey.F6,
            Key.F7 => ImGuiKey.F7,
            Key.F8 => ImGuiKey.F8,
            Key.F9 => ImGuiKey.F9,
            Key.F10 => ImGuiKey.F10,
            Key.F11 => ImGuiKey.F11,
            Key.F12 => ImGuiKey.F12,
            _ => ImGuiKey.None,
        };
    }

    private unsafe void SetupRenderState(ImDrawDataPtr drawDataPtr, int framebufferWidth, int framebufferHeight)
    {
        // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, polygon fill
        opengl.Enable(GLEnum.Blend);
        opengl.BlendEquation(GLEnum.FuncAdd);
        opengl.BlendFuncSeparate(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha, GLEnum.One, GLEnum.OneMinusSrcAlpha);
        opengl.Disable(GLEnum.CullFace);
        opengl.Disable(GLEnum.DepthTest);
        opengl.Disable(GLEnum.StencilTest);
        opengl.Enable(GLEnum.ScissorTest);
#if !GLES && !LEGACY
        opengl.Disable(GLEnum.PrimitiveRestart);
        opengl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
#endif

        float L = drawDataPtr.DisplayPos.X;
        float R = drawDataPtr.DisplayPos.X + drawDataPtr.DisplaySize.X;
        float T = drawDataPtr.DisplayPos.Y;
        float B = drawDataPtr.DisplayPos.Y + drawDataPtr.DisplaySize.Y;

        Span<float> orthoProjection = stackalloc float[] {
            2.0f / (R - L), 0.0f, 0.0f, 0.0f,
            0.0f, 2.0f / (T - B), 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            (R + L) / (L - R), (T + B) / (B - T), 0.0f, 1.0f,
        };

        shader.UseShader();
        opengl.Uniform1(alocTex, 0);
        opengl.UniformMatrix4(alocProj, 1, false, orthoProjection);

        opengl.BindSampler(0, 0);

        // Setup desired GL state
        // Recreate the VAO every time (this is to easily allow multiple GL contexts to be rendered to. VAO are not shared among GL contexts)
        // The renderer would actually work without any VAO bound, but then our VertexAttrib calls would overwrite the default one currently bound.
        vao = opengl.GenVertexArray();
        opengl.BindVertexArray(vao);

        // Bind vertex/index buffers and setup attributes for ImDrawVert
        opengl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
        opengl.EnableVertexAttribArray((uint) alocPos);
        opengl.EnableVertexAttribArray((uint) alocUv);
        opengl.EnableVertexAttribArray((uint) alocColor);
        opengl.VertexAttribPointer((uint) alocPos, 2, GLEnum.Float, false, (uint) sizeof(ImDrawVert), (void*) 0);
        opengl.VertexAttribPointer((uint) alocUv, 2, GLEnum.Float, false, (uint) sizeof(ImDrawVert), (void*) 8);
        opengl.VertexAttribPointer((uint) alocColor, 4, GLEnum.UnsignedByte, true, (uint) sizeof(ImDrawVert), (void*) 16);
    }

    private unsafe void RenderImDrawData(ImDrawData* drawDataPtr)
    {
        int framebufferWidth = (int) (drawDataPtr->DisplaySize.X * drawDataPtr->FramebufferScale.X);
        int framebufferHeight = (int) (drawDataPtr->DisplaySize.Y * drawDataPtr->FramebufferScale.Y);
        if (framebufferWidth <= 0 || framebufferHeight <= 0)
            return;

        // Backup GL state
        opengl.GetInteger(GLEnum.ActiveTexture, out int lastActiveTexture);
        opengl.ActiveTexture(GLEnum.Texture0);

        opengl.GetInteger(GLEnum.CurrentProgram, out int lastProgram);
        opengl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);

        opengl.GetInteger(GLEnum.SamplerBinding, out int lastSampler);

        opengl.GetInteger(GLEnum.ArrayBufferBinding, out int lastArrayBuffer);
        opengl.GetInteger(GLEnum.VertexArrayBinding, out int lastVertexArrayObject);

#if !GLES
        Span<int> lastPolygonMode = stackalloc int[2];
        opengl.GetInteger(GLEnum.PolygonMode, lastPolygonMode);
#endif

        Span<int> lastScissorBox = stackalloc int[4];
        opengl.GetInteger(GLEnum.ScissorBox, lastScissorBox);

        opengl.GetInteger(GLEnum.BlendSrcRgb, out int lastBlendSrcRgb);
        opengl.GetInteger(GLEnum.BlendDstRgb, out int lastBlendDstRgb);

        opengl.GetInteger(GLEnum.BlendSrcAlpha, out int lastBlendSrcAlpha);
        opengl.GetInteger(GLEnum.BlendDstAlpha, out int lastBlendDstAlpha);

        opengl.GetInteger(GLEnum.BlendEquationRgb, out int lastBlendEquationRgb);
        opengl.GetInteger(GLEnum.BlendEquationAlpha, out int lastBlendEquationAlpha);

        bool lastEnableBlend = opengl.IsEnabled(GLEnum.Blend);
        bool lastEnableCullFace = opengl.IsEnabled(GLEnum.CullFace);
        bool lastEnableDepthTest = opengl.IsEnabled(GLEnum.DepthTest);
        bool lastEnableStencilTest = opengl.IsEnabled(GLEnum.StencilTest);
        bool lastEnableScissorTest = opengl.IsEnabled(GLEnum.ScissorTest);

        SetupRenderState(drawDataPtr, framebufferWidth, framebufferHeight);

        // Will project scissor/clipping rectangles into framebuffer space
        Vector2 clipOff = drawDataPtr->DisplayPos;         // (0,0) unless using multi-viewports
        Vector2 clipScale = drawDataPtr->FramebufferScale; // (1,1) unless using retina display which are often (2,2)

        // Render command lists
        for (int n = 0; n < drawDataPtr->CmdListsCount; n++)
        {
            ImDrawListPtr cmdListPtr = drawDataPtr->CmdLists.Data[n];

            // Upload vertex/index buffers

            opengl.BufferData(GLEnum.ArrayBuffer, (nuint) (cmdListPtr.VtxBuffer.Size * sizeof(ImDrawVert)), (void*) cmdListPtr.VtxBuffer.Data, GLEnum.StreamDraw);
            opengl.BufferData(GLEnum.ElementArrayBuffer, (nuint) (cmdListPtr.IdxBuffer.Size * sizeof(ushort)), (void*) cmdListPtr.IdxBuffer.Data, GLEnum.StreamDraw);

            for (int cmd_i = 0; cmd_i < cmdListPtr.CmdBuffer.Size; cmd_i++)
            {
                ImDrawCmdPtr cmdPtr = &cmdListPtr.CmdBuffer.Data[cmd_i];

                Vector4 clipRect;
                clipRect.X = (cmdPtr.ClipRect.X - clipOff.X) * clipScale.X;
                clipRect.Y = (cmdPtr.ClipRect.Y - clipOff.Y) * clipScale.Y;
                clipRect.Z = (cmdPtr.ClipRect.Z - clipOff.X) * clipScale.X;
                clipRect.W = (cmdPtr.ClipRect.W - clipOff.Y) * clipScale.Y;

                if (clipRect.X < framebufferWidth && clipRect.Y < framebufferHeight && clipRect.Z >= 0.0f && clipRect.W >= 0.0f)
                {
                    // Apply scissor/clipping rectangle
                    opengl.Scissor((int) clipRect.X, (int) (framebufferHeight - clipRect.W), (uint) (clipRect.Z - clipRect.X), (uint) (clipRect.W - clipRect.Y));

                    // Bind texture, Draw
                    opengl.BindTexture(GLEnum.Texture2D, (uint)cmdPtr.TextureId.Handle);

                    opengl.DrawElementsBaseVertex(GLEnum.Triangles, cmdPtr.ElemCount, GLEnum.UnsignedShort, (void*) (cmdPtr.IdxOffset * sizeof(ushort)), (int) cmdPtr.VtxOffset);
                }
            }
        }

        // Destroy the temporary VAO
        opengl.DeleteVertexArray(vao);
        vao = 0;

        // Restore modified GL state
        opengl.UseProgram((uint) lastProgram);
        opengl.BindTexture(GLEnum.Texture2D, (uint) lastTexture);

        opengl.BindSampler(0, (uint) lastSampler);

        opengl.ActiveTexture((GLEnum) lastActiveTexture);

        opengl.BindVertexArray((uint) lastVertexArrayObject);

        opengl.BindBuffer(GLEnum.ArrayBuffer, (uint) lastArrayBuffer);
        opengl.BlendEquationSeparate((GLEnum) lastBlendEquationRgb, (GLEnum) lastBlendEquationAlpha);
        opengl.BlendFuncSeparate((GLEnum) lastBlendSrcRgb, (GLEnum) lastBlendDstRgb, (GLEnum) lastBlendSrcAlpha, (GLEnum) lastBlendDstAlpha);

        if (lastEnableBlend)
        {
            opengl.Enable(GLEnum.Blend);
        }
        else
        {
            opengl.Disable(GLEnum.Blend);
        }

        if (lastEnableCullFace)
        {
            opengl.Enable(GLEnum.CullFace);
        }
        else
        {
            opengl.Disable(GLEnum.CullFace);
        }

        if (lastEnableDepthTest)
        {
            opengl.Enable(GLEnum.DepthTest);
        }
        else
        {
            opengl.Disable(GLEnum.DepthTest);
        }
        if (lastEnableStencilTest)
        {
            opengl.Enable(GLEnum.StencilTest);
        }
        else
        {
            opengl.Disable(GLEnum.StencilTest);
        }

        if (lastEnableScissorTest)
        {
            opengl.Enable(GLEnum.ScissorTest);
        }
        else
        {
            opengl.Disable(GLEnum.ScissorTest);
        }

        opengl.Scissor(lastScissorBox[0], lastScissorBox[1], (uint) lastScissorBox[2], (uint) lastScissorBox[3]);
    }

    private void CreateDeviceResources()
    {
        // Backup GL state

        opengl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);
        opengl.GetInteger(GLEnum.ArrayBufferBinding, out int lastArrayBuffer);
        opengl.GetInteger(GLEnum.VertexArrayBinding, out int lastVertexArray);

        string vertexSource =
        @"#version 330
        layout (location = 0) in vec2 Position;
        layout (location = 1) in vec2 UV;
        layout (location = 2) in vec4 Color;
        uniform mat4 ProjMtx;
        out vec2 Frag_UV;
        out vec4 Frag_Color;
        void main()
        {
            Frag_UV = UV;
            Frag_Color = Color;
            gl_Position = ProjMtx * vec4(Position.xy,0,1);
        }";

        string fragmentSource =
        @"#version 330
        in vec2 Frag_UV;
        in vec4 Frag_Color;
        uniform sampler2D Texture;
        layout (location = 0) out vec4 Out_Color;
        void main()
        {
            Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
        }";

        shader = new ImGuiShader(opengl, vertexSource, fragmentSource);

        alocTex = shader.GetUniformLocation("Texture");
        alocProj = shader.GetUniformLocation("ProjMtx");
        alocPos = shader.GetAttribLocation("Position");
        alocUv = shader.GetAttribLocation("UV");
        alocColor = shader.GetAttribLocation("Color");

        vbo = opengl.GenBuffer();
        ebo = opengl.GenBuffer();

        RecreateFontDeviceTexture();

        // Restore modified GL state
        opengl.BindTexture(GLEnum.Texture2D, (uint) lastTexture);
        opengl.BindBuffer(GLEnum.ArrayBuffer, (uint) lastArrayBuffer);

        opengl.BindVertexArray((uint) lastVertexArray);
    }

    /// <summary>
    /// Creates the texture used to render text.
    /// </summary>
    private unsafe void RecreateFontDeviceTexture()
    {
        // Build texture atlas
        var io = ImGui.GetIO();
        byte* pixels;
            int width;
            int height;
            ImGui.GetTexDataAsRGBA32(io.Fonts, &pixels, &width, &height, null);   // Load as RGBA 32-bit (75% of the memory is wasted, but default font is so small) because it is more likely to be compatible with user's existing shaders. If your ImTextureId represent a higher-level concept than just a GL texture id, consider calling GetTexDataAsAlpha8() instead to save on GPU memory.

        // Upload texture to graphics system
        opengl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);

        fontTexture = new ImGuiTexture(opengl, width, height, (nint)pixels);
        fontTexture.Bind();
        fontTexture.SetMagFilter(TextureMagFilter.Linear);
        fontTexture.SetMinFilter(TextureMinFilter.Linear);

        // Store our identifier
        io.Fonts.SetTexID((IntPtr) fontTexture.GlTexture);

        // Restore state
        opengl.BindTexture(GLEnum.Texture2D, (uint) lastTexture);
    }

    /// <summary>
    /// Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        view.Resize -= WindowResized;
        keyboard.KeyChar -= OnKeyChar;

        opengl.DeleteBuffer(vbo);
        opengl.DeleteBuffer(ebo);
        opengl.DeleteVertexArray(vao);

        fontTexture.Dispose();
        shader.Dispose();

        ImGui.DestroyContext(context);
    }
}