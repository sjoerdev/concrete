using System.Numerics;
using Silk.NET.OpenGL;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp.PixelFormats;

namespace Concrete;

public static class ModelReader
{
    public static Mesh[] GetMeshes(string filePath)
    {
        List<Mesh> cmeshes = [];
        var model = ModelRoot.Load(filePath);

        foreach (var node in model.LogicalNodes)
        {
            var mesh = node.Mesh;
            if (mesh != null)
            {
                foreach (var prim in mesh.Primitives)
                {
                    var cmesh = PrimToMesh(prim);
                    cmesh.offset = node.WorldMatrix;
                    cmeshes.Add(cmesh);
                }
            }
        }
        
        return cmeshes.ToArray();
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
        if (baseColorTexture != null) material.albedoTexture = LoadTexture(baseColorTexture.PrimaryImage, 0);

        // read roughness
        var metallicRoughnessTexture = gltfMaterial.FindChannel("MetallicRoughness")?.Texture;
        if (metallicRoughnessTexture != null) material.roughnessTexture = LoadTexture(metallicRoughnessTexture.PrimaryImage, 1);

        cmesh.material = material;

        cmesh.SetupBuffers();
        return cmesh;
    }

    private static unsafe uint LoadTexture(Image gltf_image, int unit)
    {
        // imagesharp
        var bytes = gltf_image.Content.Content.Span;
        var is_image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
        int width = is_image.Width;
        int height = is_image.Height;
        byte[] rawdata = new byte[width * height * 4];
        is_image.CopyPixelDataTo(rawdata);

        // create texture
        uint texture = IPlatform.Current.GetGL().GenTexture();
        IPlatform.Current.GetGL().ActiveTexture(TextureUnit.Texture0 + unit);
        IPlatform.Current.GetGL().BindTexture(GLEnum.Texture2D, texture);
        IPlatform.Current.GetGL().TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
        IPlatform.Current.GetGL().TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
        IPlatform.Current.GetGL().TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
        IPlatform.Current.GetGL().TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
        fixed (void* pointer = rawdata) IPlatform.Current.GetGL().TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, GLEnum.UnsignedByte, pointer);
        IPlatform.Current.GetGL().BindTexture(GLEnum.Texture2D, 0);

        return texture;
    }
}