using System.Numerics;
using System.Xml.Linq;

namespace Project;

public class Scene
{
    public List<GameObject> gameObjects = [];

    public Scene(string path)
    {
        LoadFile(path);
    }

    public void Start()
    {
        foreach (var gameObject in gameObjects) gameObject.Start();
    }

    public void Update(float deltaTime)
    {
        foreach (var gameObject in gameObjects) gameObject.Update(deltaTime);
    }

    public GameObject CreateGameObject()
    {
        var gameObject = new GameObject();
        gameObjects.Add(gameObject);
        return gameObject;
    }

    private void LoadFile(string filePath)
    {
        var xml = XDocument.Load(filePath);

        foreach (var xmlGameObject in xml.Root.Elements("GameObject"))
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