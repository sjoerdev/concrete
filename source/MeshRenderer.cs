using System;

namespace Project;

public class MeshRenderer : Component
{
    public string modelPath;
    int vao;
    int vbo;

    public MeshRenderer(string modelPath)
    {
        this.modelPath = modelPath;
    }

    public override void Start()
    {
        // vao = LoadModel(modelPath);
    }

    public override void Update(float deltaTime)
    {
        // render vao
    }
}