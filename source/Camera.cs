using System.Numerics;

namespace Project;

public class Camera : Component
{
    public static Camera main;
    public Matrix4x4 view;
    public Matrix4x4 proj;
    public float fov = 90;

    public override void Start()
    {
        main = this;
        float aspect = (float)Game.window.Size.X / (float)Game.window.Size.Y;
        proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * fov / 180f, aspect, 0.1f, 1000f);
    }

    public override void Update(float deltaTime)
    {
        view = Matrix4x4.CreateLookAt(gameObject.transform.position, gameObject.transform.position + gameObject.transform.Forward(), gameObject.transform.Up());
    }

    public override void Render(float deltaTime)
    {
        // rendering
    }
}