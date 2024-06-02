using System.Numerics;

namespace GameEngine;

public class Light : Component
{
    public float brightness = 1;
    public Vector3 color = Vector3.One;
}

public class PointLight : Light
{
    public float range = 10;

    public override void Start()
    {
        Engine.pointLights.Add(this);
    }
}

public class DirectionalLight : Light
{
    public override void Start()
    {
        Engine.directionalLights.Add(this);
    }
}

public class SpotLight : Light
{
    public float range = 4;
    public float angle = 30;
    public float softness = 0.5f;

    public override void Start()
    {
        Engine.spotLights.Add(this);
    }
}