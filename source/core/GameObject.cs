using System;

namespace GameEngine;

public class GameObject
{
    public string name;
    public Transform transform;
    public List<Component> components = [];

    public GameObject()
    {
        transform = AddComponent<Transform>();
        var scene = Engine.activeScene;
        scene.gameObjects.Add(this);
        name = "GameObject_" + scene.gameObjects.Count.ToString();
    }

    public GameObject(Scene scene)
    {
        transform = AddComponent<Transform>();
        scene.gameObjects.Add(this);
        name = "GameObject_" + scene.gameObjects.Count.ToString();
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
        foreach (var component in components) component.Start();
    }

    public void Update(float deltaTime)
    {
        foreach (var component in components) component.Update(deltaTime);
    }

    public void Render(float deltaTime)
    {
        foreach (var component in components) component.Render(deltaTime);
    }
}
