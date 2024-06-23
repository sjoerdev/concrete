using System.Numerics;
using Silk.NET.Input;

namespace GameEngine;

public class Camera : Component
{
    public Matrix4x4 view;
    public Matrix4x4 proj;
    [Show] public float fov = 90;

    private Vector2 lastMousePos;

    public void SetActive()
    {
        Engine.activeCamera = this;
    }

    public override void Start()
    {
        // do stuff
    }

    public override void Update(float deltaTime)
    {
        if (Engine.sceneWindowFocussed) ApplyMovement(deltaTime);
        float aspect = Engine.framebuffer.size.X / Engine.framebuffer.size.Y;
        proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * fov / 180f, aspect, 0.1f, 1000f);
        view = Matrix4x4.CreateLookAt(gameObject.transform.worldPosition, gameObject.transform.worldPosition + gameObject.transform.Forward(), gameObject.transform.Up());
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
        gameObject.transform.localPosition += movedir * deltaTime;

        // change rotation
        var mouse = Engine.input.Mice[0];
        var lookSpeed = 0.12f;
        if (mouse.IsButtonPressed(MouseButton.Right))
        {
            var mouseDelta = lastMousePos - mouse.Position;
            gameObject.transform.localEulerAngles += new Vector3(-mouseDelta.Y, mouseDelta.X, 0) * lookSpeed;
        }
        lastMousePos = mouse.Position;
    }
}