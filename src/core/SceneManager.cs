using System;

namespace Concrete;

public static class SceneManager
{
    public static Scene loadedScene = null;
    public static PlayState playState = PlayState.stopped;

    public static void StartPlaying()
    {
        Serialization.SaveScene("scene.bin", loadedScene);
        StartScene();
        playState = PlayState.playing;
    }

    public static void PausePlaying()
    {
        playState = PlayState.paused;
    }

    public static void ContinuePlaying()
    {
        playState = PlayState.playing;
    }

    public static void StopPlaying()
    {
        playState = PlayState.stopped;
        loadedScene = Serialization.LoadScene("scene.bin");
    }

    public static void LoadScene(Scene scene) => loadedScene = scene;
    public static void StartScene() => loadedScene?.Start();
    public static void UpdateScene(float deltaTime) => loadedScene?.Update(deltaTime);
    public static void RenderScene(float deltaTime, Perspective perspective) => loadedScene?.Render(deltaTime, perspective);
}

public enum PlayState
{
    stopped,
    playing,
    paused
}