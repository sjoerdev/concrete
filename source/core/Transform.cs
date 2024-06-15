using System.Numerics;

namespace GameEngine
{
    public class Transform : Component
    {
        private Transform currentParent = null;
        public List<Transform> currentChildren = [];

        private Vector3 currentLocalPosition = Vector3.Zero;
        private Vector3 currentWorldPosition = Vector3.Zero;
        private Quaternion currentLocalQuaternion = Quaternion.Identity;
        private Quaternion currentWorldQuaternion = Quaternion.Identity;
        private Vector3 currentLocalEulerAngles = Vector3.Zero;
        private Vector3 currentWorldEulerAngles = Vector3.Zero;
        private Vector3 currentLocalScale = Vector3.One;
        private Vector3 currentWorldScale = Vector3.One;

        public Transform parent
        {
            get => currentParent;
            set
            {
                if (currentParent == value) return;
                currentParent?.currentChildren.Remove(this);
                currentParent = value;
                currentParent?.currentChildren.Add(this);
                UpdateLocalPosition();
                UpdateLocalRotation();
                UpdateLocalScale();
            }
        }

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
            get
            {
                UpdateWorldPosition();
                return currentWorldPosition;
            }
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
            get
            {
                UpdateWorldRotation();
                return currentWorldQuaternion;
            }
            set
            {
                currentWorldQuaternion = value;
                UpdateLocalRotation();
            }
        }

        public Vector3 localEulerAngles
        {
            get => currentLocalEulerAngles;
            set
            {
                currentLocalEulerAngles = value;
                currentLocalQuaternion = Quaternion.CreateFromYawPitchRoll(value.Y, value.X, value.Z);
                UpdateWorldRotation();
            }
        }

        public Vector3 worldEulerAngles
        {
            get
            {
                UpdateWorldRotation();
                return currentWorldEulerAngles;
            }
            set
            {
                currentWorldEulerAngles = value;
                currentWorldQuaternion = Quaternion.CreateFromYawPitchRoll(value.Y, value.X, value.Z);
                UpdateLocalRotation();
            }
        }

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
            get
            {
                UpdateWorldScale();
                return currentWorldScale;
            }
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
        }

        private void UpdateLocalPosition()
        {
            if (currentParent != null) currentLocalPosition = Vector3.Transform(currentWorldPosition - currentParent.worldPosition, Quaternion.Inverse(currentParent.worldQuaternion)) / currentParent.worldScale;
            else currentLocalPosition = currentWorldPosition;
        }

        public void UpdateWorldRotation()
        {
            if (currentParent != null) currentWorldQuaternion = currentLocalQuaternion * currentParent.worldQuaternion;
            else currentWorldQuaternion = currentLocalQuaternion;
            currentWorldEulerAngles = GetEulerAnglesFromQuaternion(currentWorldQuaternion);
        }

        private void UpdateLocalRotation()
        {
            if (currentParent != null) currentLocalQuaternion = currentWorldQuaternion * Quaternion.Inverse(currentParent.worldQuaternion);
            else currentLocalQuaternion = currentWorldQuaternion;
            currentLocalEulerAngles = GetEulerAnglesFromQuaternion(currentLocalQuaternion);
        }

        public void UpdateWorldScale()
        {
            if (currentParent != null) currentWorldScale = currentLocalScale * currentParent.worldScale;
            else currentWorldScale = currentLocalScale;
        }

        private void UpdateLocalScale()
        {
            if (currentParent != null) currentLocalScale = currentWorldScale / currentParent.worldScale;
            else currentLocalScale = currentWorldScale;
        }

        public Vector3 Forward() => Vector3.Transform(Vector3.UnitZ, worldQuaternion);

        public Vector3 Up() => Vector3.Transform(Vector3.UnitY, worldQuaternion);

        public Vector3 Right() => Vector3.Transform(Vector3.UnitX, worldQuaternion);

        public Matrix4x4 GetWorldModelMatrix()
        {
            var translation = Matrix4x4.CreateTranslation(worldPosition);
            var rotation = Matrix4x4.CreateFromQuaternion(worldQuaternion);
            var scale = Matrix4x4.CreateScale(worldScale);
            return scale * rotation * translation;
        }

        private Vector3 GetEulerAnglesFromQuaternion(Quaternion q)
        {
            Vector3 angles;

            float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = MathF.Atan2(sinr_cosp, cosr_cosp);

            float sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (MathF.Abs(sinp) >= 1) angles.Y = MathF.CopySign(MathF.PI / 2, sinp);
            else angles.Y = MathF.Asin(sinp);

            float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = MathF.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
    }
}