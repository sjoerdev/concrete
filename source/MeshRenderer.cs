using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Assimp;

namespace GameEngine;

public unsafe class MeshRenderer : Component
{
    private GL opengl;

    public Shader shader;
    public string modelPath;
    public uint indicesCount;

    uint vao;
    uint vbo;
    uint ebo;

    public override void Start()
    {
        opengl = Engine.opengl;
        ReadModel(modelPath);
        shader = new Shader("resources/shaders/default-vert.glsl", "resources/shaders/default-frag.glsl");
    }

    public override void Update(float deltaTime)
    {
        // update stuff
    }

    public override void Render(float deltaTime)
    {
        shader.Use();
        shader.SetMatrix4("model", gameObject.transform.GetModelMatrix());
        shader.SetMatrix4("view", Engine.activeCamera.view);
        shader.SetMatrix4("proj", Engine.activeCamera.proj);
        
        opengl.BindVertexArray(vao);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
        
        opengl.DrawElements(GLEnum.Triangles, indicesCount, DrawElementsType.UnsignedInt, null);

        opengl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
        opengl.BindVertexArray(0);
    }

    private void ReadModel(string modelPath)
    {
        // get vertex data
        ReadVertices(modelPath, out Vertex[] vertices, out uint[] indices);
        var vertexfloats = VertexFloats(vertices);
        indicesCount = (uint)indices.Length;

        // generate vao, vbo, ebo
        vao = opengl.GenVertexArray();
        vbo = opengl.GenBuffer();
        ebo = opengl.GenBuffer();

        // bind vao, vbo, ebo
        opengl.BindVertexArray(vao);
        opengl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);

        // set buffers
        fixed (void* ptr = &vertexfloats[0]) opengl.BufferData(GLEnum.ArrayBuffer, (uint)(vertexfloats.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        fixed (void* ptr = &indices[0]) opengl.BufferData(GLEnum.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), ptr, GLEnum.StaticDraw);
        
        // atribute arrays
        opengl.EnableVertexAttribArray(0);
        opengl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)0);
        opengl.EnableVertexAttribArray(1);
        opengl.VertexAttribPointer(1, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
        opengl.EnableVertexAttribArray(2);
        opengl.VertexAttribPointer(2, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
        
        // unbind
        opengl.BindBuffer(GLEnum.ArrayBuffer, 0);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
        opengl.BindVertexArray(0);
    }

    private void ReadVertices(string path, out Vertex[] vertices, out uint[] indices)
    {
        var tempVertices = new List<Vertex>();
        var tempIndices = new List<uint>();

        var assimp = Assimp.GetApi();
        var scene = assimp.ImportFile(path, (uint)PostProcessSteps.Triangulate | (uint)PostProcessSteps.JoinIdenticalVertices);
        if (scene == null) throw new Exception("Error loading model.");
        
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

    private float[] VertexFloats(Vertex[] vertices)
    {
        List<float> list = new List<float>();
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

    private struct Vertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
        public Vector3 position = position;
        public Vector3 normal = normal;
        public Vector2 uv = uv;
    }
}