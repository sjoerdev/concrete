using System;

namespace Project;

public class Component
{
    public GameObject gameObject;

    public virtual void Start()
    {
        // should be overridden
    }

    public virtual void Update(float deltaTime)
    {
        // should be overridden
    }
}
