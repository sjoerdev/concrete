using System.Numerics;
using Silk.NET.OpenGL;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp.PixelFormats;

namespace Concrete;

public class Extractor
{
    public static Mesh[] GetMeshes(string filePath)
    {
        var model = ModelRoot.Load(filePath);

        var gmesh = model.LogicalMeshes[0];
        Mesh[] cmeshes = new Mesh[gmesh.Primitives.Count];
        for (int i = 0; i < gmesh.Primitives.Count; i++)
        {
            var gprim = gmesh.Primitives[i];
            var cmesh = PrimToMesh(gprim);
            cmeshes[i] = cmesh;
        }

        return cmeshes;
    }

    private static Mesh PrimToMesh(MeshPrimitive gprim)
    {
        if (gprim == null) return null;
        var cmesh = new Mesh();

        // read vertices
        var positions = gprim.GetVertexAccessor("POSITION").AsVector3Array();
        var normals = gprim.GetVertexAccessor("NORMAL").AsVector3Array();
        var uvs = gprim.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();
        var joints = gprim.GetVertexAccessor("JOINTS_0")?.AsVector4Array();
        var weights = gprim.GetVertexAccessor("WEIGHTS_0")?.AsVector4Array();

        for (int i = 0; i < positions.Count; i++)
        {
            var vertex = new Vertex
            {
                position = positions[i],
                normal = normals[i],
                uv = uvs?[i] ?? Vector2.Zero,
                joints = joints?[i] ?? -Vector4.One,
                weights = weights?[i] ?? -Vector4.One,
            };
            cmesh.vertices.Add(vertex);
        }

        // read indices
        cmesh.indices = gprim.GetIndexAccessor().AsIndicesArray().ToList();

        // read material
        var gltfMaterial = gprim.Material;
        var material = new Material();

        // read basecolor
        material.color = gltfMaterial.FindChannel("BaseColor").Value.Color;

        // read albedo
        var baseColorTexture = gltfMaterial.FindChannel("BaseColor")?.Texture;
        if (baseColorTexture != null) material.albedoTexture = LoadTexture(baseColorTexture.PrimaryImage, 2);

        // read roughness
        var metallicRoughnessTexture = gltfMaterial.FindChannel("MetallicRoughness")?.Texture;
        if (metallicRoughnessTexture != null) material.roughnessTexture = LoadTexture(metallicRoughnessTexture.PrimaryImage, 3);

        cmesh.material = material;

        cmesh.SetupBuffers();
        return cmesh;
    }

    private static unsafe uint LoadTexture(Image gltf_image, int textureUnit)
    {
        // create texture
        Engine.opengl.ActiveTexture(TextureUnit.Texture0 + textureUnit);
        uint texture = Engine.opengl.GenTexture();
        Engine.opengl.BindTexture(GLEnum.Texture2D, texture);
        Engine.opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
        Engine.opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
        Engine.opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
        Engine.opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);

        // imagesharp
        var bytes = gltf_image.Content.Content.Span;
        var is_image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
        int width = is_image.Width;
        int height = is_image.Height;
        byte[] rawdata = new byte[width * height * 4];
        is_image.CopyPixelDataTo(rawdata);

        // send rawdata
        fixed (void* pointer = rawdata) Engine.opengl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, GLEnum.UnsignedByte, pointer);

        // unbind and return
        Engine.opengl.BindTexture(GLEnum.Texture2D, 0);
        return texture;
    }
}