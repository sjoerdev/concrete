using System;

namespace GameEngine;

public class GameObject
{
    public string name;
    public bool enabled;
    public Transform transform;
    public List<Component> components = [];

    public GameObject()
    {
        transform = AddComponent<Transform>();
        var scene = Engine.activeScene;
        scene.gameObjects.Add(this);
        name = "GameObject_" + scene.gameObjects.Count.ToString();
        enabled = true;
    }

    public GameObject(Scene scene)
    {
        transform = AddComponent<Transform>();
        scene.gameObjects.Add(this);
        name = "GameObject_" + scene.gameObjects.Count.ToString();
        enabled = true;
    }

    public T AddComponent<T>() where T : Component, new()
    {
        T component = new T();
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

    public void Render(float deltaTime)
    {
        if (!enabled) return;
        foreach (var component in components) component.Render(deltaTime);
    }
}
