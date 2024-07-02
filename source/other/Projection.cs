using System.Numerics;

namespace GameEngine;

public struct Projection()
{
    public Matrix4x4 view = Matrix4x4.Identity;
    public Matrix4x4 proj = Matrix4x4.Identity;
}