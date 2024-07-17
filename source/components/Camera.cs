using System.Numerics;

namespace Concrete;

public class Camera : Component
{
    [Show] public float fov = 90;

    public Projection Project()
    {
        var proj = new Projection();
        var resolution = Engine.editor.gameWindowFramebuffer.size;
        float aspect = (float)resolution.X / (float)resolution.Y;
        proj.proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * fov / 180f, aspect, 0.1f, 1000f);
        proj.view = Matrix4x4.CreateLookAt(gameObject.transform.worldPosition, gameObject.transform.worldPosition + gameObject.transform.forward, gameObject.transform.up);
        return proj;
    }
}