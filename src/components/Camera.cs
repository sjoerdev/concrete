using System.Numerics;

namespace Concrete;

public class Camera : Component
{
    [Include] [Show] public float fov = 90;

    public Perspective CalcPerspective()
    {
        var perspective = new Perspective();
        var resolution = Editor.gameWindowFramebuffer.size;
        float aspect = (float)resolution.X / (float)resolution.Y;
        perspective.proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * fov / 180f, aspect, 0.1f, 1000f);
        perspective.view = Matrix4x4.CreateLookAt(gameObject.transform.worldPosition, gameObject.transform.worldPosition + gameObject.transform.forward, gameObject.transform.up);
        return perspective;
    }
}