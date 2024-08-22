using System;

namespace Concrete;

public static class SceneManager
{
    public static Scene loadedScene = null;
    public static PlayerState playerState = PlayerState.stopped;

    public static void LoadScene(Scene scene)
    {
        loadedScene = scene;
    }

    public static void Play()
    {
        Serialization.SaveScene("scene.bin", loadedScene);
        Start();
        playerState = PlayerState.playing;
    }

    public static void Pause()
    {
        playerState = PlayerState.paused;
    }

    public static void Continue()
    {
        playerState = PlayerState.playing;
    }

    public static void Stop()
    {
        playerState = PlayerState.stopped;
        loadedScene = Serialization.LoadScene("scene.bin");
    }

    public static void Start()
    {
        loadedScene?.Start();
    }

    public static void TryUpdate(float deltaTime)
    {
        if (playerState != PlayerState.playing) return;
        loadedScene?.Update(deltaTime);
    }

    public static void Render(float deltaTime, Perspective projection)
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