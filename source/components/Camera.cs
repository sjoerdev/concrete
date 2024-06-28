using System.Numerics;

namespace GameEngine;

public class Camera : Component
{
    public Projection projection = new();
    [Show] public float fov = 90;

    public override void Update(float deltaTime)
    {
        float aspect = 1;
        projection.proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * fov / 180f, aspect, 0.1f, 1000f);
        projection.view = Matrix4x4.CreateLookAt(gameObject.transform.worldPosition, gameObject.transform.worldPosition + gameObject.transform.forward, gameObject.transform.up);
    }
}