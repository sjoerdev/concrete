using System.Numerics;
using Silk.NET.Input;

namespace GameEngine;

public class Camera : Component
{
    public Matrix4x4 view;
    public Matrix4x4 proj;
    public float fov = 90;

    private Vector2 lastMousePos;

    public void SetActive()
    {
        Engine.activeCamera = this;
    }

    public override void Start()
    {
        float aspect = (float)Engine.window.Size.X / (float)Engine.window.Size.Y;
        proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * fov / 180f, aspect, 0.1f, 1000f);
    }

    public override void Update(float deltaTime)
    {
        view = Matrix4x4.CreateLookAt(gameObject.transform.position, gameObject.transform.position + gameObject.transform.Forward(), gameObject.transform.Up());
        ApplyMovement(deltaTime);
    }

    public override void Render(float deltaTime)
    {
        // rendering
    }

    private void ApplyMovement(float deltaTime)
    {
        // change position
        var keyboard = Engine.input.Keyboards[0];
        var movedir = new Vector3();
        if (keyboard.IsKeyPressed(Key.W)) movedir += gameObject.transform.Forward();
        if (keyboard.IsKeyPressed(Key.A)) movedir += gameObject.transform.Right();
        if (keyboard.IsKeyPressed(Key.S)) movedir -= gameObject.transform.Forward();
        if (keyboard.IsKeyPressed(Key.D)) movedir -= gameObject.transform.Right();
        if (keyboard.IsKeyPressed(Key.Space)) movedir += gameObject.transform.Up();
        if (keyboard.IsKeyPressed(Key.ControlLeft)) movedir -= gameObject.transform.Up();
        if (keyboard.IsKeyPressed(Key.ShiftLeft)) movedir *= 2;
        gameObject.transform.position += movedir * deltaTime;

        // change rotation
        var mouse = Engine.input.Mice[0];
        var lookSpeed = 0.4f;
        if (mouse.IsButtonPressed(MouseButton.Right))
        {
            var mouseDelta = lastMousePos - mouse.Position;
            gameObject.transform.rotation += new Vector3(-mouseDelta.Y, mouseDelta.X, 0) * deltaTime * lookSpeed;
        }
        lastMousePos = mouse.Position;
    }
}