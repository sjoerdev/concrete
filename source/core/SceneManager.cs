using System;

namespace Concrete;

public class SceneManager
{
    public Scene loadedScene = null;
    public PlayerState playerState = PlayerState.stopped;

    public void LoadScene(Scene scene)
    {
        loadedScene = scene;
    }

    public void Play()
    {
        Serialization.SaveScene("scene.bin", loadedScene);
        Start();
        playerState = PlayerState.playing;
    }

    public void Pause()
    {
        playerState = PlayerState.paused;
    }

    public void Continue()
    {
        playerState = PlayerState.playing;
    }

    public void Stop()
    {
        playerState = PlayerState.stopped;
        loadedScene = Serialization.LoadScene("scene.bin");
    }

    public void Start()
    {
        loadedScene?.Start();
    }

    public void TryUpdate(float deltaTime)
    {
        if (playerState != PlayerState.playing) return;
        loadedScene?.Update(deltaTime);
    }

    public void Render(float deltaTime, Projection projection)
    {
        loadedScene?.Render(deltaTime, projection);
    }
}

public enum PlayerState
{
    stopped,
    playing,
    paused
}