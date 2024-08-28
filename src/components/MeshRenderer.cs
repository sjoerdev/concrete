using System.Numerics;
using SharpGLTF.Schema2;
using SharpGLTF.Runtime;
using SharpGLTF.Transforms;

namespace Concrete;

public class MeshRenderer : Component
{
    private Mesh[] meshes = [];
    private Shader shader = Shader.Default;

    private bool animated = false;
    private SceneInstance instance;

    [Include]
    public string modelPath
    {
        get => currentModelPath;
        set
        {
            currentModelPath = value;
            meshes = Extractor.GetMeshes(currentModelPath);
            instance = SceneTemplate.Create(ModelRoot.Load(currentModelPath).DefaultScene).CreateInstance();
            if (instance.GetDrawableInstance(0).Transform is SkinnedTransform) animated = true;
        }
    }
    private string currentModelPath;

    public override void Render(float deltaTime, Perspective perspective)
    {
        shader.Use();

        shader.SetMatrix4("model", gameObject.transform.GetWorldModelMatrix());
        shader.SetMatrix4("view", perspective.view);
        shader.SetMatrix4("proj", perspective.proj);
        
        shader.SetLights(SceneManager.loadedScene.FindActiveLights());

        if (animated)
        {
            var skinnedTransform = (SkinnedTransform)instance.GetDrawableInstance(0).Transform;
            Matrix4x4[] skinnedMatrices = skinnedTransform.SkinMatrices.ToArray();
            for (int i = 0; i < 100; i++) shader.SetMatrix4($"jointMatrices[{i}]", Matrix4x4.Identity);
            for (int i = 0; i < skinnedMatrices.Length; i++) shader.SetMatrix4($"jointMatrices[{i}]", skinnedMatrices[i]);
        }
        
        foreach (var mesh in meshes)
        {
            shader.SetVector4("matColor", mesh.material.color);
            var hasAlbedo = mesh.material.albedoTexture != null;
            shader.SetBool("matHasAlbedoTexture", hasAlbedo);
            if (hasAlbedo) shader.SetTexture("matAlbedoTexture", (uint)mesh.material.albedoTexture, 2);
            var hasRoughness = mesh.material.roughnessTexture != null;
            shader.SetBool("matHasRoughnessTexture", hasRoughness);
            if (hasRoughness) shader.SetTexture("matRoughnessTexture", (uint)mesh.material.roughnessTexture, 3);
            mesh.Render();
        }
    }
}