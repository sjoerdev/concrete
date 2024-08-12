using System;
using Silk.NET.OpenGL;

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