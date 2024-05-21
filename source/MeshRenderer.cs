using System;
using Silk.NET.OpenGL;

namespace Project;

public unsafe class MeshRenderer : Component
{
    private GL opengl;

    public Shader shader;
    public string modelPath;
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
        opengl.BindVertexArray(vao);
        opengl.DrawArrays(GLEnum.Triangles, 0, 3);
        opengl.BindVertexArray(0);
    }

    uint VertexArrayFromModel(string path)
    {
        float[] vertices = 
        [
            0.0f,  0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
           -0.5f, -0.5f, 0.0f,
        ];

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
}