using System;

namespace Concrete;

public class Component
{
    [Include] public GameObject gameObject;

    public virtual void Start()
    {
        // can be overridden
    }

    public virtual void Update(float deltaTime)
    {
        // can be overridden
    }

    public virtual void Render(float deltaTime, Perspective projection)
    {
        // can be overridden
    }
}
