using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Assimp;
using Silk.NET.Core.Native;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Concrete;

public unsafe class ModelRenderer : Component
{
    [Include]
    public string modelPath
    {
        get => currentModelPath;
        set
        {
            currentModelPath = value;
            model = ExtractModel(currentModelPath);
        }
    }
    private string currentModelPath;

    private GL opengl = Engine.opengl;
    private Shader shader = Shader.Default;
    private Model model;

    public override void Render(float deltaTime, Projection projection)
    {
        shader.Use();

        shader.SetMatrix4("model", gameObject.transform.GetWorldModelMatrix());
        shader.SetMatrix4("view", projection.view);
        shader.SetMatrix4("proj", projection.proj);
        
        shader.SetLights(SceneManager.loadedScene.FindActiveLights());
        
        foreach (var mesh in model.meshes)
        {
            var material = model.materials[(int)mesh.materialIndex];
            var hasAlbedo = material.albedoTexture != null;
            shader.SetVector4("matColor", material.color);
            shader.SetBool("matHasAlbedoTexture", hasAlbedo);
            if (hasAlbedo) shader.SetTexture("matAlbedoTexture", (uint)material.albedoTexture, 0);
            mesh.Render();
        }
    }

    private Model ExtractModel(string path)
    {
        var assimp = Assimp.GetApi();
        var assimpScene = assimp.ImportFile(path, (uint)PostProcessSteps.Triangulate | (uint)PostProcessSteps.JoinIdenticalVertices);
        var tempModel = new Model();
        
        // meshes
        for (uint i = 0; i < assimpScene->MNumMeshes; i++)
        {
            var assimpMesh = assimpScene->MMeshes[i];
            var tempMesh = new Mesh();

            // vertices
            for (uint j = 0; j < assimpMesh->MNumVertices; j++)
            {
                var position = assimpMesh->MVertices != null ? assimpMesh->MVertices[j] : Vector3.Zero;
                var normal = assimpMesh->MNormals != null ? assimpMesh->MNormals[j] : Vector3.Zero;
                var uv = assimpMesh->MTextureCoords[0] != null ? new Vector2(assimpMesh->MTextureCoords[0][j].X, assimpMesh->MTextureCoords[0][j].Y) : Vector2.Zero;

                var tempVertex = new Vertex
                {
                    position = position,
                    normal = normal,
                    uv = uv
                };

                tempMesh.vertices.Add(tempVertex);
            }

            // indices
            for (uint j = 0; j < assimpMesh->MNumFaces; j++)
            {
                var face = assimpMesh->MFaces[j];
                for (uint k = 0; k < face.MNumIndices; k++)
                {
                    var tempIndex = face.MIndices[k];
                    tempMesh.indices.Add(tempIndex);
                }
            }

            // material index
            tempMesh.materialIndex = assimpMesh->MMaterialIndex;

            tempMesh.GenerateBuffers();
            tempModel.meshes.Add(tempMesh);
        }

        // materials
        for (int i = 0; i < assimpScene->MNumMaterials; i++)
        {
            var assimpMaterial = assimpScene->MMaterials[i];
            var tempMaterial = new Material();

            // albedo color
            assimp.GetMaterialColor(assimpMaterial, Assimp.MatkeyColorDiffuse, 0, 0, ref tempMaterial.color);

            // albedo texture
            var assimpAlbedoTexture = FindTexOfType(TextureType.Diffuse, assimp, assimpScene, assimpMaterial);
            if (assimpAlbedoTexture != null) tempMaterial.albedoTexture = GenAssimpTex(assimpAlbedoTexture);

            tempModel.materials.Add(tempMaterial);
        }

        assimp.ReleaseImport(assimpScene);

        return tempModel;
    }

    private uint? GenAssimpTex(Silk.NET.Assimp.Texture* assimpTexture)
    {
        uint? result = null;
        var format = SilkMarshal.PtrToString((nint)(&assimpTexture->AchFormatHint[0]));
        if (format == "jpg")
        {
            var textureData = new Span<byte>(assimpTexture->PcData, (int)assimpTexture->MWidth);

            uint tempTexture = opengl.GenTexture();
            opengl.ActiveTexture(TextureUnit.Texture0);
            opengl.BindTexture(TextureTarget.Texture2D, tempTexture);

            var image = Image.Load<Rgba32>(textureData);
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            
            int width = image.Width;
            int height = image.Height;
            byte[] rawdata = new byte[width * height * 4];
            image.CopyPixelDataTo(rawdata);
            
            fixed (void* ptr = rawdata) opengl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, GLEnum.Rgba, PixelType.UnsignedByte, ptr);

            opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
            opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
            opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
            opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
            opengl.GenerateMipmap(GLEnum.Texture2D);
            opengl.BindTexture(GLEnum.Texture2D, 0);

            result = tempTexture;
        }
        return result;
    }

    private Silk.NET.Assimp.Texture* FindTexOfType(TextureType textureType, Assimp assimp, Silk.NET.Assimp.Scene* assimpScene, Silk.NET.Assimp.Material* assimpMaterial)
    {
        AssimpString assimpString = new();
        assimp.GetMaterialTexture(assimpMaterial, textureType, 0, &assimpString, null, null, null, null, null, null);
        string texpath = assimpString.ToString();
        bool embedded = texpath.StartsWith("*");
        Silk.NET.Assimp.Texture* assimpTexture = null;
        if (embedded) assimpTexture = assimpScene->MTextures[int.Parse(texpath.Substring(1))];
        return assimpTexture;
    }
}