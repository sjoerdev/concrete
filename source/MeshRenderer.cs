using System;
using Silk.NET.OpenGL;
using Silk.NET.Assimp;
using System.Numerics;

namespace GameEngine;

public unsafe class MeshRenderer : Component
{
    private GL opengl;

    public Shader shader;
    public string modelPath;
    public uint indicesCount;
    uint vao;

    public override void Start()
    {
        opengl = Engine.opengl;
        vao = VertexArrayFromModel(modelPath);
        shader = new Shader("resources/shaders/default-vert.glsl", "resources/shaders/default-frag.glsl");
    }

    public override void Update(float deltaTime)
    {
        // update stuff
    }

    public override void Render(float deltaTime)
    {
        shader.Use();
        shader.SetMatrix4("view", Engine.activeCamera.view);
        shader.SetMatrix4("proj", Engine.activeCamera.proj);
        
        opengl.BindVertexArray(vao);
        opengl.DrawArrays(GLEnum.Triangles, 0, indicesCount / 3);
        opengl.BindVertexArray(0);
    }

    private uint VertexArrayFromModel(string modelPath)
    {
        LoadModel(modelPath, out Vertex[] vertices, out uint[] indices);

        indicesCount = (uint)indices.Length;

        List<float> vertbufferlist = [];
        foreach (var vertex in vertices)
        {
            vertbufferlist.Add(vertex.position.X);
            vertbufferlist.Add(vertex.position.Y);
            vertbufferlist.Add(vertex.position.Z);
        }
        float[] vertbuffer = vertbufferlist.ToArray();

        // generate vao, vbo, ebo
        uint temp_vao = opengl.GenVertexArray();
        uint temp_vbo = opengl.GenBuffer();
        uint temp_ebo = opengl.GenBuffer();

        // bind vao, vbo, ebo
        opengl.BindVertexArray(temp_vao);
        opengl.BindBuffer(GLEnum.ArrayBuffer, temp_vbo);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, temp_ebo);

        // set buffers
        fixed (void* ptr = &vertbuffer[0]) opengl.BufferData(GLEnum.ArrayBuffer, (uint)(vertbuffer.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        fixed (void* ptr = &indices[0]) opengl.BufferData(GLEnum.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), ptr, GLEnum.StaticDraw);
        
        // atribute arrays
        opengl.EnableVertexAttribArray(0);
        opengl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), null);
        
        // unbind
        opengl.BindBuffer(GLEnum.ArrayBuffer, 0);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
        opengl.BindVertexArray(0);

        return temp_vao;
    }

    private void LoadModel(string path, out Vertex[] vertices, out uint[] indices)
    {
        var assimp = Assimp.GetApi();
        var scene = assimp.ImportFile(path, (uint)PostProcessSteps.Triangulate | (uint)PostProcessSteps.JoinIdenticalVertices);
        if (scene == null) throw new Exception("Error loading model.");

        var tempVertices = new List<Vertex>();
        var tempIndices = new List<uint>();
        for (uint i = 0; i < scene->MNumMeshes; i++)
        {
            var mesh = scene->MMeshes[i];
            for (uint j = 0; j < mesh->MNumVertices; j++)
            {
                var position = mesh->MVertices[j];
                var normal = mesh->MNormals[j];
                var uv = Vector2.Zero;
                if (mesh->MTextureCoords[0] != null) uv = new Vector2(mesh->MTextureCoords[0][j].X, mesh->MTextureCoords[0][j].Y);
                tempVertices.Add(new Vertex(position, normal, uv));
            }
            for (uint j = 0; j < mesh->MNumFaces; j++)
            {
                var face = mesh->MFaces[j];
                for (uint k = 0; k < face.MNumIndices; k++)
                {
                    tempIndices.Add(face.MIndices[k]);
                }
            }
        }
        assimp.ReleaseImport(scene);
        vertices = tempVertices.ToArray();
        indices = tempIndices.ToArray();
    }
    
    private struct Vertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
        public Vector3 position = position;
        public Vector3 normal = normal;
        public Vector2 uv = uv;
    }
}