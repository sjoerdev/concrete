using System.Numerics;
using Silk.NET.OpenGL;

namespace Concrete;

public class Model
{
    public List<Mesh> meshes = [];
    public List<Material> materials = [];
}

public class Mesh
{
    public List<Vertex> vertices = [];
    public List<uint> indices = [];

    public MeshBuffers buffers;

    public uint materialIndex;

    public unsafe void GenerateBuffers()
    {
        // get opengl context
        var opengl = Engine.opengl;

        // create buffers
        buffers.vao = opengl.GenVertexArray();
        buffers.vbo = opengl.GenBuffer();
        buffers.ebo = opengl.GenBuffer();

        // bind buffers
        opengl.BindVertexArray(buffers.vao);
        opengl.BindBuffer(GLEnum.ArrayBuffer, buffers.vbo);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, buffers.ebo);

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
        
        // unbind mesh
        opengl.BindVertexArray(0);
    }

    public unsafe void Render()
    {
        var opengl = Engine.opengl;
        opengl.BindVertexArray(buffers.vao);
        opengl.DrawElements(GLEnum.Triangles, (uint)indices.Count, DrawElementsType.UnsignedInt, null);
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
        }
        return list.ToArray();
    }
}

public struct Vertex
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;
}

public struct MeshBuffers
{
    public uint vao;
    public uint vbo;
    public uint ebo;
}

public class Material
{
    public Vector4 color = Vector4.One;
    public uint? albedoTexture = null;
    public uint? roughnessTexture = null;
}