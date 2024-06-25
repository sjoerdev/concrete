using System.Numerics;

namespace GameEngine;

public class Transform : Component
{
    private Transform currentParent = null;
    public List<Transform> children = [];

    private Vector3 currentLocalPosition = Vector3.Zero;
    private Vector3 currentWorldPosition = Vector3.Zero;
    private Quaternion currentLocalQuaternion = Quaternion.Identity;
    private Quaternion currentWorldQuaternion = Quaternion.Identity;
    private Vector3 currentLocalEulerAngles = Vector3.Zero;
    private Vector3 currentWorldEulerAngles = Vector3.Zero;
    private Vector3 currentLocalScale = Vector3.One;
    private Vector3 currentWorldScale = Vector3.One;

    private readonly float toDegrees = 180.0f / MathF.PI;
    private readonly float toRadians = MathF.PI / 180.0f;

    public Transform parent
    {
        get => currentParent;
        set
        {
            if (currentParent == value) return;
            currentParent?.children.Remove(this);
            currentParent = value;
            currentParent?.children.Add(this);
            UpdateLocalPosition();
            UpdateLocalRotation();
            UpdateLocalScale();
        }
    }

    [Show("position")]
    public Vector3 localPosition
    {
        get => currentLocalPosition;
        set
        {
            currentLocalPosition = value;
            UpdateWorldPosition();
        }
    }

    public Vector3 worldPosition
    {
        get => currentWorldPosition;
        set
        {
            currentWorldPosition = value;
            UpdateLocalPosition();
        }
    }

    public Quaternion localQuaternion
    {
        get => currentLocalQuaternion;
        set
        {
            currentLocalQuaternion = value;
            UpdateWorldRotation();
        }
    }

    public Quaternion worldQuaternion
    {
        get => currentWorldQuaternion;
        set
        {
            currentWorldQuaternion = value;
            UpdateLocalRotation();
        }
    }

    [Show("rotation")]
    public Vector3 localEulerAngles
    {
        get => currentLocalEulerAngles;
        set
        {
            currentLocalEulerAngles = value;
            currentLocalQuaternion = Quaternion.CreateFromYawPitchRoll(value.Y * toRadians, value.X * toRadians, value.Z * toRadians);
            UpdateWorldRotation();
        }
    }

    public Vector3 worldEulerAngles
    {
        get => currentWorldEulerAngles;
        set
        {
            currentWorldEulerAngles = value;
            currentWorldQuaternion = Quaternion.CreateFromYawPitchRoll(value.Y * toRadians, value.X * toRadians, value.Z * toRadians);
            UpdateLocalRotation();
        }
    }

    [Show("scale")]
    public Vector3 localScale
    {
        get => currentLocalScale;
        set
        {
            currentLocalScale = value;
            UpdateWorldScale();
        }
    }

    public Vector3 worldScale
    {
        get => currentWorldScale;
        set
        {
            currentWorldScale = value;
            UpdateLocalScale();
        }
    }

    public void UpdateWorldPosition()
    {
        if (currentParent != null) currentWorldPosition = Vector3.Transform(currentLocalPosition, currentParent.worldQuaternion) * currentParent.worldScale + currentParent.worldPosition;
        else currentWorldPosition = currentLocalPosition;
        foreach (var child in children)
        {
            child.UpdateWorldPosition();
            child.UpdateWorldRotation();
            child.UpdateWorldScale();
        }
    }

    private void UpdateLocalPosition()
    {
        if (currentParent != null) currentLocalPosition = Vector3.Transform(currentWorldPosition - currentParent.worldPosition, Quaternion.Inverse(currentParent.worldQuaternion)) / currentParent.worldScale;
        else currentLocalPosition = currentWorldPosition;
        foreach (var child in children)
        {
            child.UpdateWorldPosition();
            child.UpdateWorldRotation();
            child.UpdateWorldScale();
        }
    }

    public void UpdateWorldRotation()
    {
        if (currentParent != null) currentWorldQuaternion = currentParent.worldQuaternion * currentLocalQuaternion;
        else currentWorldQuaternion = currentLocalQuaternion;
        currentWorldEulerAngles = GetEulerAnglesFromQuaternion(currentWorldQuaternion);
        foreach (var child in children)
        {
            child.UpdateWorldPosition();
            child.UpdateWorldRotation();
            child.UpdateWorldScale();
        }
    }

    private void UpdateLocalRotation()
    {
        if (currentParent != null) currentLocalQuaternion = currentWorldQuaternion * Quaternion.Inverse(currentParent.worldQuaternion);
        else currentLocalQuaternion = currentWorldQuaternion;
        currentLocalEulerAngles = GetEulerAnglesFromQuaternion(currentLocalQuaternion);
        foreach (var child in children)
        {
            child.UpdateWorldPosition();
            child.UpdateWorldRotation();
            child.UpdateWorldScale();
        }
    }

    public void UpdateWorldScale()
    {
        if (currentParent != null) currentWorldScale = currentLocalScale * currentParent.worldScale;
        else currentWorldScale = currentLocalScale;
        foreach (var child in children)
        {
            child.UpdateWorldPosition();
            child.UpdateWorldRotation();
            child.UpdateWorldScale();
        }
    }

    private void UpdateLocalScale()
    {
        if (currentParent != null) currentLocalScale = currentWorldScale / currentParent.worldScale;
        else currentLocalScale = currentWorldScale;
        foreach (var child in children)
        {
            child.UpdateWorldPosition();
            child.UpdateWorldRotation();
            child.UpdateWorldScale();
        }
    }

    public Vector3 forward => Vector3.Transform(Vector3.UnitZ, worldQuaternion);

    public Vector3 up => Vector3.Transform(Vector3.UnitY, worldQuaternion);

    public Vector3 right => Vector3.Transform(Vector3.UnitX, worldQuaternion);

    public Matrix4x4 GetWorldModelMatrix()
    {
        var translation = Matrix4x4.CreateTranslation(worldPosition);
        var rotation = Matrix4x4.CreateFromQuaternion(worldQuaternion);
        var scale = Matrix4x4.CreateScale(worldScale);
        return scale * rotation * translation;
    }

    private Vector3 GetEulerAnglesFromQuaternion(Quaternion quaternion)
    {
        Vector3 angles;

        float sinr = 2 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
        float cosr = 1 - 2 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
        angles.X = MathF.Atan2(sinr, cosr);

        float sinp = 2 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
        if (MathF.Abs(sinp) >= 1) angles.Y = MathF.CopySign(MathF.PI / 2, sinp);
        else angles.Y = MathF.Asin(sinp);

        float siny = 2 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
        float cosy = 1 - 2 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
        angles.Z = MathF.Atan2(siny, cosy);

        return angles * toDegrees;
    }
}