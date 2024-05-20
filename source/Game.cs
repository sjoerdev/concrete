using System;

namespace Project;

class Entry
{
    static void Main()
    {
        Console.WriteLine("entry found");
        // Game game = new Game();
        // game.Start();
        // game.update
    }
}

class Game
{
    public Scene activeScene = null;
    public List<Scene> scenes = [];

    public void Start()
    {
        LoadScene("scene.xml");
        PlayScene(0);
    }

    public void Update(float deltaTime)
    {
        if (activeScene != null) activeScene.Update(deltaTime);
    }

    private void PlayScene(int index)
    {
        activeScene = scenes[index];
        activeScene.Start();
    }

    private void LoadScene(string path)
    {
        var scene = new Scene(path);
        scenes.Add(scene);
    }
}
