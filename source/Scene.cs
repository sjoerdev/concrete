using System.Numerics;
using System.Xml.Linq;
using Silk.NET.GLFW;

namespace Project;

public class Scene
{
    public List<GameObject> gameObjects = [];

    public Scene(string filePath = null)
    {
        Game.scenes.Add(this);
        if (filePath != null) LoadFromFile(filePath);
    }

    public void Activate()
    {
        Game.activeScene = this;
    }

    public void Start()
    {
        foreach (var gameObject in gameObjects) gameObject.Start();
    }

    public void Update(float deltaTime)
    {
        foreach (var gameObject in gameObjects) gameObject.Update(deltaTime);
    }

    public void Render(float deltaTime)
    {
        foreach (var gameObject in gameObjects) gameObject.Render(deltaTime);
    }

    public GameObject CreateGameObject()
    {
        var gameObject = new GameObject();
        gameObjects.Add(gameObject);
        return gameObject;
    }

    private void LoadFromFile(string filePath)
    {
        foreach (var xmlGameObject in XDocument.Load(filePath).Root.Elements("GameObject"))
        {
            var gameObject = CreateGameObject();
            foreach (var xmlComponent in xmlGameObject.Elements())
            {
                var type = xmlComponent.Name.LocalName;
                if (type == "Transform")
                {
                    var transform = gameObject.GetComponent<Transform>();
                    transform.position = ParseVector3(xmlComponent.Element("position").Value);
                    transform.rotation = ParseVector3(xmlComponent.Element("rotation").Value);
                    transform.scale = ParseVector3(xmlComponent.Element("scale").Value);
                }
                else if (type == "MeshRenderer")
                {
                    var modelPath = xmlComponent.Element("modelPath").Value;
                    var meshRenderer = new MeshRenderer(modelPath);
                    gameObject.AddComponent(meshRenderer);
                }
            }
        }
    }

    private Vector3 ParseVector3(string vectorString)
    {
        var values = vectorString.Split(',').Select(float.Parse).ToArray();
        return new Vector3(values[0], values[1], values[2]);
    }
}