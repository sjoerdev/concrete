using System.Numerics;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

public class Scene
{
    public List<GameObject> GameObjects = new List<GameObject>();

    public Scene(string path)
    {
        LoadFile(path);
    }

    public void Start()
    {
        foreach (var gameObject in GameObjects)
        {
            gameObject.Start();
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var gameObject in GameObjects)
        {
            gameObject.Update(deltaTime);
        }
    }

    public GameObject CreateGameObject()
    {
        var gameObject = new GameObject();
        GameObjects.Add(gameObject);
        return gameObject;
    }

    private void LoadFile(string filePath)
    {
        var xml = XDocument.Load(filePath);

        foreach (var xmlGameObject in xml.Root.Elements("GameObject"))
        {
            var gameObject = CreateGameObject();
            foreach (var component in xmlGameObject.Elements())
            {
                var type = component.Name.LocalName;
                if (type == "Transform")
                {
                    var transform = gameObject.GetComponent<Transform>();
                    transform.Position = ParseVector3(component.Element("Position").Value);
                    transform.Rotation = ParseVector3(component.Element("Rotation").Value);
                    transform.Scale = ParseVector3(component.Element("Scale").Value);
                }
                else if (type == "MeshRenderer")
                {
                    var modelPath = component.Element("ModelPath").Value;
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
