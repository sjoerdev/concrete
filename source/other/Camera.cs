using System.Numerics;

namespace GameEngine;

public class Camera
{
    public float fov = 90;
    public Matrix4x4 view = Matrix4x4.Identity;
    public Matrix4x4 proj = Matrix4x4.Identity;
    public Vector3 position = Vector3.Zero;
    public Vector3 rotation = Vector3.Zero;
    public Vector3 forward => LocalDirection(Vector3.UnitZ);
    public Vector3 up => LocalDirection(Vector3.UnitY);
    public Vector3 right => LocalDirection(Vector3.UnitX);

    public void Update(float aspect, Transform transform = null)
    {
        if (transform != null)
        {
            position = transform.worldPosition;
            rotation = transform.worldEulerAngles;
        }

        proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * fov / 180f, aspect, 0.1f, 1000f);
        view = Matrix4x4.CreateLookAt(position, position + forward, up);
    }

    private Vector3 LocalDirection(Vector3 worldDirection)
    {
        var toRadians = MathF.PI / 180.0f;
        var quaternion = Quaternion.CreateFromYawPitchRoll(rotation.Y * toRadians, rotation.X * toRadians, rotation.Z * toRadians);
        var direction = Vector3.Transform(worldDirection, quaternion);
        return direction;
    }
}