using System;

namespace Concrete;

public class GameObject
{
    [Include] public int id;
    [Include] public string name;
    [Include] public bool enabled;
    [Include] public Transform transform;
    [Include] public List<Component> components = [];

    public GameObject()
    {
        // do nothing
    }

    public static GameObject Create()
    {
        var scene = SceneManager.loadedScene;
        var gameObject = new GameObject();
        gameObject.transform = gameObject.AddComponent<Transform>();
        gameObject.id = gameObject.GenerateID();
        gameObject.name = $"GameObject ({scene.gameObjects.Count})";
        gameObject.enabled = true;
        scene.gameObjects.Add(gameObject);
        return gameObject;
    }

    public int GenerateID()
    {
        var random = new Random();
        string digits = "";
        for (int i = 0; i < 8; i++) digits += random.Next(0, 10).ToString();
        return int.Parse(digits);
    }

    public T AddComponent<T>() where T : Component, new()
    {
        T component = new T();
        component.gameObject = this;
        components.Add(component);
        return component;
    }

    public Component AddComponentOfType(Type type)
    {
        var component = (Component)Activator.CreateInstance(type);
        component.gameObject = this;
        components.Add(component);
        return component;
    }

    public T GetComponent<T>() where T : Component
    {
        return components.OfType<T>().FirstOrDefault();
    }

    public void Start()
    {
        if (!enabled) return;
        foreach (var component in components) component.Start();
    }

    public void Update(float deltaTime)
    {
        if (!enabled) return;
        foreach (var component in components) component.Update(deltaTime);
    }

    public void Render(float deltaTime, Perspective projection)
    {
        if (!enabled) return;
        foreach (var component in components) component.Render(deltaTime, projection);
    }
}