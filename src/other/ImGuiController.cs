using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Hexa.NET.ImPlot;

public unsafe class ImGuiController
{
    public ImGuiContextPtr guiContext;
    public ImPlotContextPtr plotContext;

    private GL opengl;
    private IView view;
    private IInputContext input;
    private IKeyboard keyboard;

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

    public ImGuiController(GL opengl, IView view, IInputContext input)
    {
        // init window
        this.opengl = opengl;
        this.view = view;
        this.input = input;
        width = view.Size.X;
        height = view.Size.Y;
        keyboard = input.Keyboards[0];
        view.Resize += WindowResized;
        keyboard.KeyChar += OnKeyChar;

        // create contexts
        guiContext = ImGui.CreateContext();
        plotContext = ImPlot.CreateContext();
        UpdateContexts();

        // set flags
        ImGui.GetIO().BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        
        // set stuff
        CreateDeviceResources();
        SetPerFrameImGuiData(1f / 60f);

        // other imgui settings
        ImGui.GetIO().Handle->IniFilename = null;
        ImGui.GetIO().ConfigFlags = ImGuiConfigFlags.DockingEnable;
        ImGui.StyleColorsDark();
    }

    public void Update(float deltaTime)
    {
        UpdateContexts();
        SetPerFrameImGuiData(deltaTime);
        UpdateImGuiInput();
        ImGui.NewFrame();
        ImGuizmo.BeginFrame();
    }

    public void Render()
    {
        ImGui.Render();
        ImGui.EndFrame();
        RenderImDrawData(ImGui.GetDrawData());
    }

    public void UpdateContexts()
    {
        ImGui.SetCurrentContext(guiContext);
        ImGuizmo.SetImGuiContext(guiContext);
        ImPlot.SetImGuiContext(guiContext);
        ImPlot.SetCurrentContext(plotContext);
    }

    private void OnKeyChar(IKeyboard keyboard, char character)
    {
        pressedchars.Add(character);
    }

    private void WindowResized(Vector2D<int> size)
    {
        width = size.X;
        height = size.Y;
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(width, height);
        if (width > 0 && height > 0) io.DisplayFramebufferScale = new Vector2(view.FramebufferSize.X / width, view.FramebufferSize.Y / height);
        io.DeltaTime = deltaSeconds;
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
        opengl.Enable(GLEnum.Blend);
        opengl.BlendEquation(GLEnum.FuncAdd);
        opengl.BlendFuncSeparate(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha, GLEnum.One, GLEnum.OneMinusSrcAlpha);
        opengl.Disable(GLEnum.CullFace);
        opengl.Disable(GLEnum.DepthTest);
        opengl.Disable(GLEnum.StencilTest);
        opengl.Enable(GLEnum.ScissorTest);

        float L = drawDataPtr.DisplayPos.X;
        float R = drawDataPtr.DisplayPos.X + drawDataPtr.DisplaySize.X;
        float T = drawDataPtr.DisplayPos.Y;
        float B = drawDataPtr.DisplayPos.Y + drawDataPtr.DisplaySize.Y;

        Span<float> orthoProjection = 
        [
            2.0f / (R - L), 0.0f, 0.0f, 0.0f,
            0.0f, 2.0f / (T - B), 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            (R + L) / (L - R), (T + B) / (B - T), 0.0f, 1.0f,
        ];

        shader.UseShader();
        opengl.Uniform1(alocTex, 0);
        opengl.UniformMatrix4(alocProj, 1, false, orthoProjection);
        opengl.BindSampler(0, 0);
        
        vao = opengl.GenVertexArray();
        opengl.BindVertexArray(vao);

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
        if (framebufferWidth <= 0 || framebufferHeight <= 0) return;

        opengl.GetInteger(GLEnum.ActiveTexture, out int lastActiveTexture);
        opengl.ActiveTexture(GLEnum.Texture0);

        opengl.GetInteger(GLEnum.CurrentProgram, out int lastProgram);
        opengl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);
        opengl.GetInteger(GLEnum.SamplerBinding, out int lastSampler);
        opengl.GetInteger(GLEnum.ArrayBufferBinding, out int lastArrayBuffer);
        opengl.GetInteger(GLEnum.VertexArrayBinding, out int lastVertexArrayObject);

        Span<int> lastPolygonMode = stackalloc int[2];
        opengl.GetInteger(GLEnum.PolygonMode, lastPolygonMode);
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

        Vector2 clipOff = drawDataPtr->DisplayPos;
        Vector2 clipScale = drawDataPtr->FramebufferScale;

        for (int n = 0; n < drawDataPtr->CmdListsCount; n++)
        {
            ImDrawListPtr cmdListPtr = drawDataPtr->CmdLists.Data[n];

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
                    opengl.Scissor((int) clipRect.X, (int) (framebufferHeight - clipRect.W), (uint) (clipRect.Z - clipRect.X), (uint) (clipRect.W - clipRect.Y));
                    opengl.BindTexture(GLEnum.Texture2D, (uint)cmdPtr.TextureId.Handle);
                    opengl.DrawElementsBaseVertex(GLEnum.Triangles, cmdPtr.ElemCount, GLEnum.UnsignedShort, (void*) (cmdPtr.IdxOffset * sizeof(ushort)), (int) cmdPtr.VtxOffset);
                }
            }
        }

        opengl.DeleteVertexArray(vao);
        vao = 0;

        opengl.UseProgram((uint) lastProgram);
        opengl.BindTexture(GLEnum.Texture2D, (uint) lastTexture);
        opengl.BindSampler(0, (uint) lastSampler);
        opengl.ActiveTexture((GLEnum) lastActiveTexture);
        opengl.BindVertexArray((uint) lastVertexArrayObject);
        opengl.BindBuffer(GLEnum.ArrayBuffer, (uint) lastArrayBuffer);
        opengl.BlendEquationSeparate((GLEnum) lastBlendEquationRgb, (GLEnum) lastBlendEquationAlpha);
        opengl.BlendFuncSeparate((GLEnum) lastBlendSrcRgb, (GLEnum) lastBlendDstRgb, (GLEnum) lastBlendSrcAlpha, (GLEnum) lastBlendDstAlpha);

        if (lastEnableBlend) opengl.Enable(GLEnum.Blend);
        else opengl.Disable(GLEnum.Blend);

        if (lastEnableCullFace) opengl.Enable(GLEnum.CullFace);
        else opengl.Disable(GLEnum.CullFace);

        if (lastEnableDepthTest) opengl.Enable(GLEnum.DepthTest);
        else opengl.Disable(GLEnum.DepthTest);

        if (lastEnableStencilTest) opengl.Enable(GLEnum.StencilTest);
        else opengl.Disable(GLEnum.StencilTest);

        if (lastEnableScissorTest) opengl.Enable(GLEnum.ScissorTest);
        else opengl.Disable(GLEnum.ScissorTest);

        opengl.Scissor(lastScissorBox[0], lastScissorBox[1], (uint) lastScissorBox[2], (uint) lastScissorBox[3]);
    }

    private void CreateDeviceResources()
    {
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

        opengl.BindTexture(GLEnum.Texture2D, (uint) lastTexture);
        opengl.BindBuffer(GLEnum.ArrayBuffer, (uint) lastArrayBuffer);
        opengl.BindVertexArray((uint) lastVertexArray);
    }

    private unsafe void RecreateFontDeviceTexture()
    {
        var io = ImGui.GetIO();
        byte* pixels;
        int width;
        int height;
        ImGui.GetTexDataAsRGBA32(io.Fonts, &pixels, &width, &height, null);
        opengl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);
        fontTexture = new ImGuiTexture(opengl, width, height, (nint)pixels);
        fontTexture.Bind();
        fontTexture.SetMagFilter(TextureMagFilter.Linear);
        fontTexture.SetMinFilter(TextureMinFilter.Linear);
        io.Fonts.SetTexID((IntPtr) fontTexture.GlTexture);
        opengl.BindTexture(GLEnum.Texture2D, (uint) lastTexture);
    }
}

struct UniformFieldInfo
{
    public int Location;
    public string Name;
    public int Size;
    public UniformType Type;
}

class ImGuiShader
{
    public uint Program { get; private set; }
    private readonly Dictionary<string, int> _uniformToLocation = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _attribLocation = new Dictionary<string, int>();
    private bool _initialized = false;
    private GL _gl;
    private (ShaderType Type, string Path)[] _files;

    public ImGuiShader(GL gl, string vertexShader, string fragmentShader)
    {
        _gl = gl;
        _files = new[]{
            (ShaderType.VertexShader, vertexShader),
            (ShaderType.FragmentShader, fragmentShader),
        };
        Program = CreateProgram(_files);
    }
    public void UseShader()
    {
        _gl.UseProgram(Program);
    }

    public void Dispose()
    {
        if (_initialized)
        {
            _gl.DeleteProgram(Program);
            _initialized = false;
        }
    }

    public UniformFieldInfo[] GetUniforms()
    {
        _gl.GetProgram(Program, GLEnum.ActiveUniforms, out var uniformCount);

        UniformFieldInfo[] uniforms = new UniformFieldInfo[uniformCount];

        for (int i = 0; i < uniformCount; i++)
        {
            string name = _gl.GetActiveUniform(Program, (uint) i, out int size, out UniformType type);

            UniformFieldInfo fieldInfo;
            fieldInfo.Location = GetUniformLocation(name);
            fieldInfo.Name = name;
            fieldInfo.Size = size;
            fieldInfo.Type = type;

            uniforms[i] = fieldInfo;
        }

        return uniforms;
    }

    public int GetUniformLocation(string uniform)
    {
        if (_uniformToLocation.TryGetValue(uniform, out int location) == false)
        {
            location = _gl.GetUniformLocation(Program, uniform);
            _uniformToLocation.Add(uniform, location);
        }

        return location;
    }

    public int GetAttribLocation(string attrib)
    {
        if (_attribLocation.TryGetValue(attrib, out int location) == false)
        {
            location = _gl.GetAttribLocation(Program, attrib);
            _attribLocation.Add(attrib, location);
        }

        return location;
    }

    private uint CreateProgram(params (ShaderType Type, string source)[] shaderPaths)
    {
        var program = _gl.CreateProgram();

        Span<uint> shaders = stackalloc uint[shaderPaths.Length];
        for (int i = 0; i < shaderPaths.Length; i++)
        {
            shaders[i] = CompileShader(shaderPaths[i].Type, shaderPaths[i].source);
        }

        foreach (var shader in shaders)
            _gl.AttachShader(program, shader);

        _gl.LinkProgram(program);

        _gl.GetProgram(program, GLEnum.LinkStatus, out var success);

        foreach (var shader in shaders)
        {
            _gl.DetachShader(program, shader);
            _gl.DeleteShader(shader);
        }

        _initialized = true;

        return program;
    }

    private uint CompileShader(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var success);
        return shader;
    }
}

public enum TextureCoordinate
{
    S = TextureParameterName.TextureWrapS,
    T = TextureParameterName.TextureWrapT,
    R = TextureParameterName.TextureWrapR
}

public class ImGuiTexture : IDisposable
{
    public const SizedInternalFormat Srgb8Alpha8 = (SizedInternalFormat) GLEnum.Srgb8Alpha8;
    public const SizedInternalFormat Rgb32F = (SizedInternalFormat) GLEnum.Rgb32f;

    public const GLEnum MaxTextureMaxAnisotropy = (GLEnum) 0x84FF;

    public static float? MaxAniso;
    private readonly GL _gl;
    public readonly string Name;
    public readonly uint GlTexture;
    public readonly uint Width, Height;
    public readonly uint MipmapLevels;
    public readonly SizedInternalFormat InternalFormat;

    public unsafe ImGuiTexture(GL gl, int width, int height, IntPtr data, bool generateMipmaps = false, bool srgb = false)
    {
        _gl = gl;
        MaxAniso ??= gl.GetFloat(MaxTextureMaxAnisotropy);
        Width = (uint) width;
        Height = (uint) height;
        InternalFormat = srgb ? Srgb8Alpha8 : SizedInternalFormat.Rgba8;
        MipmapLevels = (uint) (generateMipmaps == false ? 1 : (int) Math.Floor(Math.Log(Math.Max(Width, Height), 2)));

        GlTexture = _gl.GenTexture();
        Bind();

        PixelFormat pxFormat = PixelFormat.Bgra;

        _gl.TexStorage2D(GLEnum.Texture2D, MipmapLevels, InternalFormat, Width, Height);
        _gl.TexSubImage2D(GLEnum.Texture2D, 0, 0, 0, Width, Height, pxFormat, PixelType.UnsignedByte, (void*) data);

        if (generateMipmaps) _gl.GenerateTextureMipmap(GlTexture);
        SetWrap(TextureCoordinate.S, TextureWrapMode.Repeat);
        SetWrap(TextureCoordinate.T, TextureWrapMode.Repeat);

        int value = (int)(MipmapLevels - 1);
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMaxLevel, ref value);
    }

    public void Bind()
    {
        _gl.BindTexture(GLEnum.Texture2D, GlTexture);
    }

    public void SetMinFilter(TextureMinFilter filter)
    {
        int value = (int)filter;
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMinFilter, ref value);
    }

    public void SetMagFilter(TextureMagFilter filter)
    {
        int value = (int)filter;
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMagFilter, ref value);
    }

    public void SetAnisotropy(float level)
    {
        const TextureParameterName textureMaxAnisotropy = (TextureParameterName) 0x84FE;
        _gl.TexParameter(GLEnum.Texture2D, (GLEnum) textureMaxAnisotropy, Clamp(level, 1, MaxAniso.GetValueOrDefault()));
    }

    public static float Clamp(float value, float min, float max)
    {
        return value < min ? min : value > max ? max : value;
    }

    public void SetLod(int basee, int min, int max)
    {
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureLodBias, ref basee);
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMinLod, ref min);
        _gl.TexParameterI(GLEnum.Texture2D, TextureParameterName.TextureMaxLod, ref max);
    }

    public void SetWrap(TextureCoordinate coord, TextureWrapMode mode)
    {
        int value = (int)mode;
        _gl.TexParameterI(GLEnum.Texture2D, (TextureParameterName) coord, ref value);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(GlTexture);
    }
}