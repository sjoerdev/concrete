public class MeshRenderer : Component
{
    public string ModelPath;
    public int VertexArrayID;
    public int VertexBufferID;

    public MeshRenderer(string modelPath)
    {
        ModelPath = modelPath;
    }

    public override void Start() {
        // Load the model and create VAO and VBO here
        // For example:
        // VertexArrayID = LoadModel(ModelPath);
    }

    public override void Update(float deltaTime)
    {
        // Render logic here
    }

    // Example function to load the model (implementation dependent)
    // private int LoadModel(string path) {
    //     // Load the model and return the VAO ID
    // }
}