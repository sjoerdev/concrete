using System.Numerics;
using System.Xml.Linq;

namespace GameEngine;

public class Scene
{
    public List<GameObject> gameObjects = [];

    public Scene(string filePath = null)
    {
        Engine.scenes.Add(this);
        if (filePath != null) Deserialize(filePath);
    }

    public void SetActive()
    {
        Engine.activeScene = this;
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

    private void Deserialize(string filePath)
    {
        foreach (var xmlGameObject in XDocument.Load(filePath).Root.Elements("GameObject"))
        {
            var gameObject = new GameObject(this);
            foreach (var xmlComponent in xmlGameObject.Elements())
            {
                var type = xmlComponent.Name.LocalName;
                if (type == "Transform")
                {
                    var transform = gameObject.GetComponent<Transform>();
                    /*
                    transform.position = ParseVector3(xmlComponent.Element("position").Value);
                    transform.rotation = ParseVector3(xmlComponent.Element("rotation").Value);
                    transform.scale = ParseVector3(xmlComponent.Element("scale").Value);
                    */
                }
                else if (type == "MeshRenderer")
                {
                    var modelPath = xmlComponent.Element("modelPath").Value;
                    gameObject.AddComponent<MeshRenderer>().modelPath = modelPath;
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