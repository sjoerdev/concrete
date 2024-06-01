using System.Numerics;
using Silk.NET.OpenGL;

namespace GameEngine;

public class Shader
{
    private GL opengl;
    private uint handle;

    public Shader(string vertPath, string fragPath)
    {
        opengl = Engine.opengl;
        handle = CompileProgram(vertPath, fragPath);
    }

    public void Use()
    {
        opengl.UseProgram(handle);
    }

    public void SetFloat(string name, float value)
    {
        opengl.Uniform1(opengl.GetUniformLocation(handle, name), value);
    }

    public void SetVector3(string name, Vector3 value)
    {
        opengl.Uniform3(opengl.GetUniformLocation(handle, name), value.X, value.Y, value.Z);
    }

    public unsafe void SetMatrix4(string name, Matrix4x4 value)
    {
        opengl.UseProgram(handle);
        opengl.UniformMatrix4(opengl.GetUniformLocation(handle, name), 1, false, (float*)&value);
    }

    public unsafe void SetTexture(string name, uint value, uint unit)
    {
        opengl.UseProgram(handle);
        opengl.ActiveTexture(GLEnum.Texture0);
        opengl.BindTexture(GLEnum.Texture2D, value);
        opengl.Uniform1(opengl.GetUniformLocation(handle, name), unit);
    }

    private uint CompileProgram(string vertPath, string fragPath)
    {
        string vertCode = File.ReadAllText(vertPath);
        string fragCode = File.ReadAllText(fragPath);

        uint vertex = CompileShader(GLEnum.VertexShader, vertCode);
        uint fragment = CompileShader(GLEnum.FragmentShader, fragCode);
        
        uint program = opengl.CreateProgram();
        opengl.AttachShader(program, vertex);
        opengl.AttachShader(program, fragment);
        opengl.LinkProgram(program);
        
        opengl.GetProgram(program, GLEnum.LinkStatus, out int status);
        if (status == 0) throw new Exception(opengl.GetProgramInfoLog(program));

        opengl.DeleteShader(vertex);
        opengl.DeleteShader(fragment);

        return program;
    }

    private uint CompileShader(GLEnum type, string source)
    {
        uint shader = opengl.CreateShader(type);
        opengl.ShaderSource(shader, source);
        opengl.CompileShader(shader);
        
        opengl.GetShader(shader, GLEnum.CompileStatus, out int status);
        if (status == 0) throw new Exception(opengl.GetShaderInfoLog(shader));

        return shader;
    }
}
