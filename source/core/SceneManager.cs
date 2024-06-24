using System;

namespace GameEngine;

public class SceneManager
{
    public Scene activeScene = null;
    public List<Scene> scenes = [];
    public Camera activeCamera = null;
    public List<DirectionalLight> directionalLights = [];
    public List<PointLight> pointLights = [];
    public List<SpotLight> spotLights = [];

    public void StartActiveScene()
    {
        activeScene?.Start();
    }

    public void UpdateActiveScene(float deltaTime)
    {
        activeScene?.Update(deltaTime);
    }

    public void RenderActiveScene(float deltaTime)
    {
        activeScene?.Render(deltaTime);
    }
}