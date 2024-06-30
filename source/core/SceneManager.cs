using System;

namespace GameEngine;

public class SceneManager
{
    public bool isPlaying = false;
    public Scene loadedScene = null;

    public void LoadScene(Scene scene)
    {
        loadedScene = scene;
    }

    public void Play()
    {
        if (isPlaying) return;
        Start();
        isPlaying = true;
        Console.WriteLine("play");
    }

    public void Stop()
    {
        if (!isPlaying) return;
        isPlaying = false;
        Console.WriteLine("stop");
    }

    public void Start()
    {
        loadedScene?.Start();
    }

    public void TryUpdate(float deltaTime)
    {
        if (!isPlaying) return;
        loadedScene?.Update(deltaTime);
    }

    public void Render(float deltaTime, Projection projection)
    {
        loadedScene?.Render(deltaTime, projection);
    }
}