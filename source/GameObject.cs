using System.Collections.Generic;
using System.Linq;

public class GameObject
{
    public List<Component> Components = new List<Component>();
    public Transform Transform;

    public GameObject()
    {
        Transform = new Transform();
        AddComponent(Transform);
    }

    public void AddComponent(Component component)
    {
        component.gameObject = this;
        Components.Add(component);
    }

    public T GetComponent<T>() where T : Component
    {
        return Components.OfType<T>().FirstOrDefault();
    }

    public void Start()
    {
        foreach (var component in Components)
        {
            component.Start();
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var component in Components)
        {
            component.Update(deltaTime);
        }
    }
}
