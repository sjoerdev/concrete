using System.Numerics;

namespace Concrete;

public class Light : Component
{
    [Include] [Show] public float brightness = 1;
    [Include] [Show] public Vector3 color = Vector3.One;
}

public class PointLight : Light
{
    [Include] [Show] public float range = 10;
}

public class DirectionalLight : Light
{
    // no unique variables
}

public class SpotLight : Light
{
    [Include] [Show] public float range = 4;
    [Include] [Show] public float angle = 30;
    [Include] [Show] public float softness = 0.5f;
}