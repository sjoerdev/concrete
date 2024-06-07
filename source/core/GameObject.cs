using System;

namespace GameEngine;

public class GameObject
{
    public string name;
    public Transform transform;
    public List<Component> components = [];

    public GameObject()
    {
        name = new Random().Next(486734, 928472).ToString();
        transform = AddComponent<Transform>();
        Engine.activeScene.gameObjects.Add(this);
    }

    public GameObject(Scene scene)
    {
        transform = AddComponent<Transform>();
        scene.gameObjects.Add(this);
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
