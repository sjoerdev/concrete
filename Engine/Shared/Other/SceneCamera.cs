using System.Numerics;

namespace Concrete;

public class SceneCamera
{
    public Matrix4x4 view => Matrix4x4.CreateLookAt(position, position + forward, up);
    public Matrix4x4 proj => Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * fov / 180f, (float)SceneRenderWindow.framebuffer.size.X / (float)SceneRenderWindow.framebuffer.size.Y, 0.1f, 1000f);

    public float fov = 90;

    public Vector3 position = new(-0.4f, 1.6f, 1.6f);
    public Vector3 rotation = new(10, 155, 0);

    public Vector3 forward => LocalDirection(Vector3.UnitZ);
    public Vector3 up => LocalDirection(Vector3.UnitY);
    public Vector3 right => LocalDirection(Vector3.UnitX);

    private Vector2 lastMousePos;

    public void ApplyMovement(float deltaTime)
    {
        // position
        var movedir = new Vector3();
        if (IPlatform.Current.IsKeyPressed(PlatformKey.W)) movedir += forward;
        if (IPlatform.Current.IsKeyPressed(PlatformKey.A)) movedir += right;
        if (IPlatform.Current.IsKeyPressed(PlatformKey.S)) movedir -= forward;
        if (IPlatform.Current.IsKeyPressed(PlatformKey.D)) movedir -= right;
        if (IPlatform.Current.IsKeyPressed(PlatformKey.Space)) movedir += up;
        if (IPlatform.Current.IsKeyPressed(PlatformKey.ControlLeft)) movedir -= up;
        if (IPlatform.Current.IsKeyPressed(PlatformKey.ShiftLeft)) movedir *= 2;
        position += movedir * deltaTime;

        // rotation
        var lookSpeed = 0.12f;
        if (IPlatform.Current.IsMouseButtonPressed(1))
        {
            var mouseDelta = lastMousePos - IPlatform.Current.GetMousePosition();
            rotation += new Vector3(-mouseDelta.Y, mouseDelta.X, 0) * lookSpeed;
        }
        lastMousePos = IPlatform.Current.GetMousePosition();
    }

    private Vector3 LocalDirection(Vector3 worldDirection)
    {
        var toRadians = MathF.PI / 180.0f;
        var quaternion = Quaternion.CreateFromYawPitchRoll(rotation.Y * toRadians, rotation.X * toRadians, rotation.Z * toRadians);
        var direction = Vector3.Transform(worldDirection, quaternion);
        return direction;
    }
}