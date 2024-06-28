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
        loadedScene?.Start();
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void Update(float deltaTime)
    {
        if (isPlaying) loadedScene?.Update(deltaTime);
    }

    public void Render(float deltaTime, Projection projection)
    {
        loadedScene?.Render(deltaTime, projection);
    }
}