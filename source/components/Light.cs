using System.Numerics;

namespace GameEngine;

public class Light : Component
{
    [Show] public float brightness = 1;
    [Show] public Vector3 color = Vector3.One;
}

public class PointLight : Light
{
    [Show] public float range = 10;

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
    [Show] public float range = 4;
    [Show] public float angle = 30;
    [Show] public float softness = 0.5f;

    public override void Start()
    {
        Engine.spotLights.Add(this);
    }
}