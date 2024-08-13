using System.Numerics;
using Silk.NET.OpenGL;

namespace Concrete;

public class Shader
{
    private GL opengl;
    private uint handle;

    public static Shader Default => new("res/shaders/default-vert.glsl", "res/shaders/default-frag.glsl");

    public Shader(string vertPath, string fragPath)
    {
        opengl = Engine.opengl;
        handle = CompileProgram(vertPath, fragPath);
    }

    public void Use()
    {
        opengl.UseProgram(handle);
    }

    public void SetBool(string name, bool value)
    {
        opengl.Uniform1(opengl.GetUniformLocation(handle, name), value ? 1 : 0);
    }

    public void SetFloat(string name, float value)
    {
        opengl.Uniform1(opengl.GetUniformLocation(handle, name), value);
    }

    public void SetVector3(string name, Vector3 value)
    {
        opengl.Uniform3(opengl.GetUniformLocation(handle, name), value.X, value.Y, value.Z);
    }

    public void SetVector4(string name, Vector4 value)
    {
        opengl.Uniform4(opengl.GetUniformLocation(handle, name), value.X, value.Y, value.Z, value.W);
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

    public void SetLights(List<Light> lights)
    {
        var directionalLights = new List<DirectionalLight>();
        var pointLights = new List<PointLight>();
        var spotLights = new List<SpotLight>();

        foreach (var light in lights)
        {
            if (light is DirectionalLight directional) directionalLights.Add(directional);
            if (light is PointLight point) pointLights.Add(point);
            if (light is SpotLight spot) spotLights.Add(spot);
        }

        // clear directional lights
        for (int i = 0; i < 16; i++)
        {
            SetVector3($"dirLights[{i}].direction", Vector3.Zero);
            SetVector3($"dirLights[{i}].color", Vector3.Zero);
            SetFloat($"dirLights[{i}].brightness", 0);
        }

        // clear point lights
        for (int i = 0; i < 16; i++)
        {
            SetVector3($"pointLights[{i}].position", Vector3.Zero);
            SetVector3($"pointLights[{i}].color", Vector3.Zero);
            SetFloat($"pointLights[{i}].brightness", 0);
            SetFloat($"pointLights[{i}].range", 0);
        }

        // clear spot lights
        for (int i = 0; i < 16; i++)
        {
            SetVector3($"spotLights[{i}].position", Vector3.Zero);
            SetVector3($"spotLights[{i}].direction", Vector3.Zero);
            SetVector3($"spotLights[{i}].color", Vector3.Zero);
            SetFloat($"spotLights[{i}].brightness", 0);
            SetFloat($"spotLights[{i}].range", 0);
            SetFloat($"spotLights[{i}].angle", 0);
            SetFloat($"spotLights[{i}].softness", 0);
        }

        // set directional lights
        for (int i = 0; i < directionalLights.Count; i++)
        {
            var light = directionalLights[i];
            SetVector3($"dirLights[{i}].direction", light.gameObject.transform.forward);
            SetFloat($"dirLights[{i}].brightness", light.brightness);
            SetVector3($"dirLights[{i}].color", light.color);
        }

        // set point lights
        for (int i = 0; i < pointLights.Count; i++)
        {
            var light = pointLights[i];
            SetVector3($"pointLights[{i}].position", light.gameObject.transform.worldPosition);
            SetFloat($"pointLights[{i}].brightness", light.brightness);
            SetVector3($"pointLights[{i}].color", light.color);
            SetFloat($"pointLights[{i}].range", light.range);
        }

        // set spot lights
        for (int i = 0; i < spotLights.Count; i++)
        {
            var light = spotLights[i];
            SetVector3($"spotLights[{i}].position", light.gameObject.transform.worldPosition);
            SetVector3($"spotLights[{i}].direction", light.gameObject.transform.forward);
            SetFloat($"spotLights[{i}].brightness", light.brightness);
            SetVector3($"spotLights[{i}].color", light.color);
            SetFloat($"spotLights[{i}].range", light.range);
            SetFloat($"spotLights[{i}].angle", light.angle);
            SetFloat($"spotLights[{i}].softness", light.softness);
        }
    }
}
