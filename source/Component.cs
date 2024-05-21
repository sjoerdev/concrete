using System;

namespace Project;

public class Component
{
    public GameObject gameObject;

    public virtual void Start()
    {
        // can be overridden
    }

    public virtual void Update(float deltaTime)
    {
        // can be overridden
    }

    public virtual void Render(float deltaTime)
    {
        // can be overridden
    }
}
