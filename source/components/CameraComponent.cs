using System.Numerics;

namespace GameEngine;

public class CameraComponent : Component
{
    public Camera camera = new();

    [Show]
    public float fov
    {
        get => camera.fov;
        set => camera.fov = value;
    }

    public void SetActive()
    {
        Engine.sceneManager.activeCamera = camera;
    }

    public override void Update(float deltaTime)
    {
        float aspect = 1; // TODO: make it use the aspect ratio of the game window
        camera.Update(aspect, gameObject.transform);
    }
}