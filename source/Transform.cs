using System.Numerics;

public class Transform : Component
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Vector3.Zero;
        Scale = Vector3.One;
    }

    public override void Start()
    {
        // Initialization logic for Transform
    }

    public override void Update(float deltaTime)
    {
        // Update logic for Transform, if any
    }
}