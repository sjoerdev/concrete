using System.Numerics;

namespace GameEngine;

public class Transform : Component
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public Transform()
    {
        position = Vector3.Zero;
        rotation = Vector3.Zero;
        scale = Vector3.One;
    }

    public override void Start()
    {
        // Initialization logic for Transform
    }

    public override void Update(float deltaTime)
    {
        // Update logic for Transform, if any
    }

    public override void Render(float deltaTime)
    {
        // Render logic for Transform, if any
    }

    public Vector3 Forward()
    {
        Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        return Vector3.Transform(Vector3.UnitZ, rotationMatrix);
    }

    public Vector3 Up()
    {
        Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        return Vector3.Transform(Vector3.UnitY, rotationMatrix);
    }

    public Vector3 Right()
    {
        Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        return Vector3.Transform(Vector3.UnitX, rotationMatrix);
    }

    public Matrix4x4 GetModelMatrix()
    {
        Matrix4x4 scale = Matrix4x4.CreateScale(this.scale);
        Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(this.rotation.Y, this.rotation.X, this.rotation.Z);
        Matrix4x4 translation = Matrix4x4.CreateTranslation(position);
        Matrix4x4 model = scale * rotation * translation;
        return model;
    }
}