using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Assimp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Concrete;

public unsafe static class Extractor
{
    static Assimp assimp = Assimp.GetApi();

    public static List<Mesh> Load(string path)
    {
        var assimpScene = assimp.ImportFile(path, (uint)PostProcessSteps.Triangulate | (uint)PostProcessSteps.JoinIdenticalVertices);

        // materials
        var tempMaterials = new List<Material>();
        for (int i = 0; i < assimpScene->MNumMaterials; i++)
        {
            var assimpMaterial = assimpScene->MMaterials[i];
            var tempMaterial = new Material();

            // albedo color
            assimp.GetMaterialColor(assimpMaterial, Assimp.MatkeyColorDiffuse, 0, 0, ref tempMaterial.color);

            // albedo texture
            var assimpAlbedoTexture = FindTexOfType(TextureType.Diffuse, assimpScene, assimpMaterial);
            if (assimpAlbedoTexture != null) tempMaterial.albedoTexture = GenAssimpTex(assimpAlbedoTexture, 2);

            // roughness texture
            var assimpRoughnessTexture = FindTexOfType(TextureType.DiffuseRoughness, assimpScene, assimpMaterial);
            if (assimpRoughnessTexture != null) tempMaterial.roughnessTexture = GenAssimpTex(assimpRoughnessTexture, 3);

            tempMaterials.Add(tempMaterial);
        }
        
        // meshes
        var tempMeshes = new List<Mesh>();
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
            int index = (int)assimpMesh->MMaterialIndex;
            tempMesh.material = tempMaterials[index];

            tempMesh.SetupBuffers();
            tempMeshes.Add(tempMesh);
        }

        assimp.ReleaseImport(assimpScene);
        return tempMeshes;
    }

    private static uint GenAssimpTex(Silk.NET.Assimp.Texture* assimpTexture, int unit)
    {
        var opengl = Engine.opengl;
        
        opengl.ActiveTexture(TextureUnit.Texture0 + unit);

        uint tempTexture = opengl.GenTexture();
        opengl.BindTexture(TextureTarget.Texture2D, tempTexture);

        var textureData = new Span<byte>(assimpTexture->PcData, (int)assimpTexture->MWidth);
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

        return tempTexture;
    }

    private static Silk.NET.Assimp.Texture* FindTexOfType(TextureType textureType, Silk.NET.Assimp.Scene* assimpScene, Silk.NET.Assimp.Material* assimpMaterial)
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