using System.Numerics;

namespace Concrete;

public class MeshRenderer : Component
{
    private Mesh[] meshes = [];
    private Shader shader = Shader.Default;

    [Include]
    public string modelPath
    {
        get => currentModelPath;
        set
        {
            currentModelPath = value;
            meshes = Extractor.GetMeshes(currentModelPath);
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