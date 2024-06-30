using System.Numerics;

namespace GameEngine;

public class Camera : Component
{
    public static Camera main = null;

    public Projection projection => Project();
    [Show] public float fov = 90;

    public Camera()
    {
        if (main == null) main = this;
    }

    public Projection Project()
    {
        var temp_projection = new Projection();
        var resolution = Engine.editor.gameWindowFramebuffer.size;
        float aspect = (float)resolution.X / (float)resolution.Y;
        temp_projection.proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * fov / 180f, aspect, 0.1f, 1000f);
        temp_projection.view = Matrix4x4.CreateLookAt(gameObject.transform.worldPosition, gameObject.transform.worldPosition + gameObject.transform.forward, gameObject.transform.up);
        return temp_projection;
    }
}