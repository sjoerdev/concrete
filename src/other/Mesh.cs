using System.Numerics;
using Silk.NET.OpenGL;

namespace Concrete;

public class Mesh
{
    public Matrix4x4 offset;
    
    public List<Vertex> vertices = [];
    public List<uint> indices = [];

    public uint vao;
    public uint vbo;
    public uint ebo;

    public Material material;

    public unsafe void Render()
    {
        var opengl = Engine.opengl;
        opengl.BindVertexArray(vao);
        opengl.DrawElements(GLEnum.Triangles, (uint)indices.Count, DrawElementsType.UnsignedInt, null);
        opengl.BindVertexArray(0);
    }

    public unsafe void SetupBuffers()
    {
        // get opengl context
        var opengl = Engine.opengl;

        // create buffers
        vao = opengl.GenVertexArray();
        vbo = opengl.GenBuffer();
        ebo = opengl.GenBuffer();

        // bind buffers
        opengl.BindVertexArray(vao);
        opengl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);

        // convert mesh data
        var verticesAsFloats = VerticesAsFloats();
        var indicesAsArray = indices.ToArray();

        // set buffers
        fixed (void* ptr = &verticesAsFloats[0]) opengl.BufferData(GLEnum.ArrayBuffer, (uint)(verticesAsFloats.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        fixed (void* ptr = &indicesAsArray[0]) opengl.BufferData(GLEnum.ElementArrayBuffer, (uint)(indicesAsArray.Length * sizeof(uint)), ptr, GLEnum.StaticDraw);
        
        // atribute arrays
        opengl.EnableVertexAttribArray(0);
        opengl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)0);
        opengl.EnableVertexAttribArray(1);
        opengl.VertexAttribPointer(1, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
        opengl.EnableVertexAttribArray(2);
        opengl.VertexAttribPointer(2, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
        opengl.EnableVertexAttribArray(3);
        opengl.VertexAttribPointer(3, 4, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(8 * sizeof(float)));
        opengl.EnableVertexAttribArray(4);
        opengl.VertexAttribPointer(4, 4, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(12 * sizeof(float)));
        
        // unbind mesh
        opengl.BindVertexArray(0);
    }

    float[] VerticesAsFloats()
    {
        List<float> list = [];
        foreach (var vertex in vertices)
        {
            list.Add(vertex.position.X);
            list.Add(vertex.position.Y);
            list.Add(vertex.position.Z);
            list.Add(vertex.normal.X);
            list.Add(vertex.normal.Y);
            list.Add(vertex.normal.Z);
            list.Add(vertex.uv.X);
            list.Add(vertex.uv.Y);
            list.Add(vertex.joints.X);
            list.Add(vertex.joints.Y);
            list.Add(vertex.joints.Z);
            list.Add(vertex.joints.W);
            list.Add(vertex.weights.X);
            list.Add(vertex.weights.Y);
            list.Add(vertex.weights.Z);
            list.Add(vertex.weights.W);
        }
        return list.ToArray();
    }
}

public struct Vertex
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;
    public Vector4 joints;
    public Vector4 weights;
}

public class Material
{
    public Vector4 color = Vector4.One;
    public uint? albedoTexture = null;
    public uint? roughnessTexture = null;
}