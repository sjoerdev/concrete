using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Assimp;
using Silk.NET.Core.Native;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameEngine;

public unsafe class MeshRenderer : Component
{
    private GL opengl;
    public string modelPath;

    Mesh mesh;
    Shader shader;
    uint mainTexture;

    public override void Start()
    {
        opengl = Engine.opengl;
        shader = new Shader("resources/shaders/default-vert.glsl", "resources/shaders/default-frag.glsl");
        LoadModelFile(modelPath);
    }

    public override void Update(float deltaTime)
    {
        // update stuff
    }

    public override void Render(float deltaTime)
    {
        shader.Use();
        shader.SetMatrix4("model", gameObject.transform.GetWorldModelMatrix());
        shader.SetMatrix4("view", Engine.activeCamera.view);
        shader.SetMatrix4("proj", Engine.activeCamera.proj);
        shader.SetTexture("tex", mainTexture, 0);
        SetLights();
        RenderMesh(mesh);
    }

    private void SetLights()
    {
        var directionalLights = Engine.directionalLights;
        var pointLights = Engine.pointLights;
        var spotLights = Engine.spotLights;

        // set directional lights
        for (int i = 0; i < directionalLights.Count; i++)
        {
            var light = directionalLights[i];
            shader.SetVector3($"dirLights[{i}].direction", light.gameObject.transform.Forward());
            shader.SetFloat($"dirLights[{i}].brightness", light.brightness);
            shader.SetVector3($"dirLights[{i}].color", light.color);
        }

        // set point lights
        for (int i = 0; i < pointLights.Count; i++)
        {
            var light = pointLights[i];
            shader.SetVector3($"pointLights[{i}].position", light.gameObject.transform.worldPosition);
            shader.SetFloat($"pointLights[{i}].brightness", light.brightness);
            shader.SetVector3($"pointLights[{i}].color", light.color);
            shader.SetFloat($"pointLights[{i}].range", light.range);
        }

        // set spotlights
        for (int i = 0; i < spotLights.Count; i++)
        {
            var light = spotLights[i];
            shader.SetVector3($"spotLights[{i}].position", light.gameObject.transform.worldPosition);
            shader.SetVector3($"spotLights[{i}].direction", light.gameObject.transform.Forward());
            shader.SetFloat($"spotLights[{i}].brightness", light.brightness);
            shader.SetVector3($"spotLights[{i}].color", light.color);
            shader.SetFloat($"spotLights[{i}].range", light.range);
            shader.SetFloat($"spotLights[{i}].angle", light.angle);
            shader.SetFloat($"spotLights[{i}].softness", light.softness);
        }

        // set view position
        shader.SetVector3("viewpos", Engine.activeCamera.gameObject.transform.worldPosition);
    }

    private void RenderMesh(Mesh meshToRender)
    {
        opengl.BindVertexArray(meshToRender.vao);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, meshToRender.ebo);
        opengl.DrawElements(GLEnum.Triangles, meshToRender.indicesCount, DrawElementsType.UnsignedInt, null);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
        opengl.BindVertexArray(0);
    }

    private void LoadModelFile(string modelPath)
    {
        // generate the assimp data
        var assimpdata = GenerateAssimpData(modelPath);

        // generate mesh
        mesh = new Mesh(opengl.GenVertexArray(), opengl.GenBuffer(), opengl.GenBuffer(), (uint)assimpdata.indices.Length);

        // set main texture
        mainTexture = assimpdata.texture;

        // cast vertices data to floats
        var vertices = ToFloats(assimpdata.vertices);

        // bind vao, vbo, ebo
        opengl.BindVertexArray(mesh.vao);
        opengl.BindBuffer(GLEnum.ArrayBuffer, mesh.vbo);
        opengl.BindBuffer(GLEnum.ElementArrayBuffer, mesh.ebo);

        // set buffers
        fixed (void* ptr = &vertices[0]) opengl.BufferData(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        fixed (void* ptr = &assimpdata.indices[0]) opengl.BufferData(GLEnum.ElementArrayBuffer, (uint)(assimpdata.indices.Length * sizeof(uint)), ptr, GLEnum.StaticDraw);
        
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

    private AssimpData GenerateAssimpData(string path)
    {
        var tempVertices = new List<Vertex>();
        var tempIndices = new List<uint>();
        uint tempTexture = 0;

        var assimp = Assimp.GetApi();
        var scene = assimp.ImportFile(path, (uint)PostProcessSteps.Triangulate | (uint)PostProcessSteps.JoinIdenticalVertices);
        if (scene == null) throw new Exception("Error loading model.");
        
        for (uint i = 0; i < scene->MNumMeshes; i++)
        {
            var mesh = scene->MMeshes[i];

            // vertices
            for (uint j = 0; j < mesh->MNumVertices; j++)
            {
                var position = mesh->MVertices != null ? mesh->MVertices[j] : Vector3.Zero;
                var normal = mesh->MNormals != null ? mesh->MNormals[j] : Vector3.Zero;
                var uv = mesh->MTextureCoords[0] != null ? new Vector2(mesh->MTextureCoords[0][j].X, mesh->MTextureCoords[0][j].Y) : Vector2.Zero;
                tempVertices.Add(new Vertex(position, normal, uv));
            }

            // indices
            for (uint j = 0; j < mesh->MNumFaces; j++)
            {
                var face = mesh->MFaces[j];
                for (uint k = 0; k < face.MNumIndices; k++)
                {
                    tempIndices.Add(face.MIndices[k]);
                }
            }
        }

        for (int i = 0; i < scene->MNumMaterials; i++)
        {
            var material = scene->MMaterials[i];
            var numTextures = assimp.GetMaterialTextureCount(material, TextureType.Diffuse);

            // textures
            for (uint j = 0; j < numTextures; j++)
            {
                var texture = scene->MTextures[j];
                var format = SilkMarshal.PtrToString((nint)(&texture->AchFormatHint[0]));

                if (format == "jpg")
                {
                    var textureData = new Span<byte>(texture->PcData, (int)texture->MWidth);

                    tempTexture = opengl.GenTexture();
                    opengl.ActiveTexture(TextureUnit.Texture0);
                    opengl.BindTexture(TextureTarget.Texture2D, tempTexture);

                    var image = Image.Load<Rgba32>(textureData);
                    image.Mutate(x => x.Flip(FlipMode.Vertical));
                    
                    int width = image.Width;
                    int height = image.Height;
                    byte[] rawdata = new byte[width * height * 4];
                    image.CopyPixelDataTo(rawdata);
                    
                    fixed (void* ptr = rawdata)
                    {
                        opengl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, GLEnum.Rgba, PixelType.UnsignedByte, ptr);
                    }

                    opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
                    opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
                    opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
                    opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
                    opengl.GenerateMipmap(GLEnum.Texture2D);
                    opengl.BindTexture(GLEnum.Texture2D, 0);
                }
            }
        }


        assimp.ReleaseImport(scene);
        return new AssimpData(tempVertices.ToArray(), tempIndices.ToArray(), tempTexture);
    }

    private float[] ToFloats(Vertex[] vertices)
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

    private struct Mesh(uint vao, uint vbo, uint ebo, uint indicesCount)
    {
        public uint vao = vao;
        public uint vbo = vbo;
        public uint ebo = ebo;
        public uint indicesCount = indicesCount;
    }

    private struct AssimpData(Vertex[] vertices, uint[] indices, uint texture)
    {
        public Vertex[] vertices = vertices;
        public uint[] indices = indices;
        public uint texture = texture;
    }

    private struct Vertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
        public Vector3 position = position;
        public Vector3 normal = normal;
        public Vector2 uv = uv;
    }
}