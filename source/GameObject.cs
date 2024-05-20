using System.Collections.Generic;

namespace Project;

public class GameObject
{
    public List<Component> components = [];
    public Transform transform;

    public GameObject()
    {
        transform = new Transform();
        AddComponent(transform);
    }

    public void AddComponent(Component component)
    {
        component.gameObject = this;
        components.Add(component);
    }

    public T GetComponent<T>() where T : Component
    {
        return components.OfType<T>().FirstOrDefault();
    }

    public void Start()
    {
        foreach (var component in components)
        {
            component.Start();
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var component in components)
        {
            component.Update(deltaTime);
        }
    }
}
