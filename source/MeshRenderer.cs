using System;
using Silk.NET.OpenGL;
using Silk.NET.Assimp;

namespace Project;

public unsafe class MeshRenderer : Component
{
    private GL opengl;

    public Shader shader;
    public string modelPath;
    uint vertexCount;
    uint vao;

    public MeshRenderer(string modelPath)
    {
        this.modelPath = modelPath;
    }

    public override void Start()
    {
        opengl = Game.opengl;
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
        shader.SetMatrix4("view", Camera.main.view);
        shader.SetMatrix4("proj", Camera.main.proj);
        
        opengl.BindVertexArray(vao);
        opengl.DrawArrays(GLEnum.Triangles, 0, vertexCount / 3);
        opengl.BindVertexArray(0);
    }

    uint VertexArrayFromModel(string path)
    {
        float[] vertices = LoadModel(path);
        vertexCount = (uint)vertices.Length;
        uint vertexArray = opengl.GenVertexArray();
        opengl.BindVertexArray(vertexArray);
        uint vertexBuffer = opengl.GenBuffer();
        opengl.BindBuffer(GLEnum.ArrayBuffer, vertexBuffer);
        fixed (void* ptr = vertices) opengl.BufferData(GLEnum.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        opengl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), null);
        opengl.EnableVertexAttribArray(0);
        opengl.BindBuffer(GLEnum.ArrayBuffer, 0);
        opengl.BindVertexArray(0);
        return vertexArray;
    }

    private float[] LoadModel(string path)
    {
        var assimp = Assimp.GetApi();
        var scene = assimp.ImportFile(path, (uint)PostProcessSteps.Triangulate | (uint)PostProcessSteps.JoinIdenticalVertices);
        if (scene == null || scene->MRootNode == null) throw new Exception("Error loading model.");

        var vertices = new List<float>();
        for (uint i = 0; i < scene->MNumMeshes; i++)
        {
            var mesh = scene->MMeshes[i];
            for (uint j = 0; j < mesh->MNumVertices; j++)
            {
                var vertex = mesh->MVertices[j];
                vertices.Add(vertex.X);
                vertices.Add(vertex.Y);
                vertices.Add(vertex.Z);
            }
        }
        assimp.ReleaseImport(scene);
        return vertices.ToArray();
    }
}