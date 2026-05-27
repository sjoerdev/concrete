using System.Numerics;
using Silk.NET.OpenGL;

namespace Concrete;

public static unsafe class GridRenderer
{
    static GL opengl => IPlatform.Current.GetGL();

    static Shader shader = Shader.CreateGrid();

    static float halfGridSize = 500;

    static uint vao;
    static uint vbo;

    static float[] vertices =
    [
        -halfGridSize, 0.0f, -halfGridSize,
        halfGridSize, 0.0f, -halfGridSize,
        halfGridSize, 0.0f,  halfGridSize,
        -halfGridSize, 0.0f, -halfGridSize,
        halfGridSize, 0.0f,  halfGridSize,
        -halfGridSize, 0.0f,  halfGridSize
    ];
    
    static GridRenderer()
    {
        SetupBuffers();
    }

    static void SetupBuffers()
    {
        // create buffers
        vao = opengl.GenVertexArray();
        vbo = opengl.GenBuffer();

        // bind buffers
        opengl.BindVertexArray(vao);
        opengl.BindBuffer(GLEnum.ArrayBuffer, vbo);

        // set buffers
        fixed (void* ptr = &vertices[0]) opengl.BufferData(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), ptr, GLEnum.StaticDraw);

        // position attribute
        opengl.EnableVertexAttribArray(0);
        opengl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)0);

        // unbind buffers
        opengl.BindVertexArray(0);
    }
    
    public static void Render(float deltaTime, Matrix4x4 view, Matrix4x4 proj)
    {
        shader.Use();

        shader.SetVector3("cameraPosition", SceneWindow.sceneCamera.position);
        shader.SetMatrix4("viewProj", Matrix4x4.Multiply(view, proj));

        opengl.BindVertexArray(vao);
        opengl.DrawArrays(GLEnum.Triangles, 0, 6);
        opengl.BindVertexArray(0);
    }
}