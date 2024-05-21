using System.Numerics;

namespace Project;

public class Camera : Component
{
    public Matrix4x4 view;
    public Matrix4x4 proj;
    public float fov;

    public Camera(float fov)
    {
        this.fov = fov;
    }

    public override void Start()
    {

    }

    public override void Update(float deltaTime)
    {

    }

    public override void Render(float deltaTime)
    {

    }
}