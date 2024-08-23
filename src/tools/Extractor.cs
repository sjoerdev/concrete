using System.Numerics;
using Silk.NET.OpenGL;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp.PixelFormats;

namespace Concrete;

public class Extractor
{
    public static List<Mesh> Load(string filePath)
    {
        var model = ModelRoot.Load(filePath);
        var meshList = new List<Mesh>();

        foreach (var scene in model.LogicalScenes)
        {
            foreach (var node in scene.VisualChildren)
            {
                ProcessNode(node, meshList);
            }
        }

        return meshList;
    }

    private static void ProcessNode(Node node, List<Mesh> meshList)
    {
        if (node.Mesh != null)
        {
            foreach (var primitive in node.Mesh.Primitives)
            {
                var mesh = new Mesh();

                // Extract vertices
                var positions = primitive.GetVertexAccessor("POSITION").AsVector3Array();
                var normals = primitive.GetVertexAccessor("NORMAL").AsVector3Array();
                var uvs = primitive.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();

                for (int i = 0; i < positions.Count; i++)
                {
                    mesh.vertices.Add(new Vertex
                    {
                        position = positions[i],
                        normal = normals[i],
                        uv = uvs?[i] ?? Vector2.Zero
                    });
                }

                // Extract indices
                mesh.indices = primitive.GetIndexAccessor().AsIndicesArray().ToList();

                // Extract material
                var gltfMaterial = primitive.Material;

                var material = new Material
                {
                    color = gltfMaterial.FindChannel("BaseColor").Value.Color
                };

                var baseColorTexture = gltfMaterial.FindChannel("BaseColor")?.Texture;
                if (baseColorTexture != null)
                {
                    material.albedoTexture = LoadTexture(baseColorTexture.PrimaryImage, 2);
                }

                var metallicRoughnessTexture = gltfMaterial.FindChannel("MetallicRoughness")?.Texture;
                if (metallicRoughnessTexture != null)
                {
                    material.roughnessTexture = LoadTexture(metallicRoughnessTexture.PrimaryImage, 3);
                }

                mesh.material = material;
                mesh.SetupBuffers();
                meshList.Add(mesh);
            }
        }

        // Recursively process child nodes
        foreach (var child in node.VisualChildren)
        {
            ProcessNode(child, meshList);
        }
    }

    private static unsafe uint LoadTexture(SharpGLTF.Schema2.Image gltf_image, int textureUnit)
    {
        Engine.opengl.ActiveTexture(TextureUnit.Texture0 + textureUnit);
        uint texture = Engine.opengl.GenTexture();
        Engine.opengl.BindTexture(GLEnum.Texture2D, texture);
        Engine.opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
        Engine.opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
        Engine.opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
        Engine.opengl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);

        var bytes = gltf_image.Content.Content.Span;
        var is_image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
        int width = is_image.Width;
        int height = is_image.Height;
        byte[] rawdata = new byte[width * height * 4];
        is_image.CopyPixelDataTo(rawdata);

        fixed (void* pointer = rawdata) Engine.opengl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, GLEnum.UnsignedByte, pointer);

        Engine.opengl.BindTexture(GLEnum.Texture2D, 0);
        return texture;
    }
}