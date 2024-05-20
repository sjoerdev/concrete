using System.Numerics;

namespace Project;

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
}