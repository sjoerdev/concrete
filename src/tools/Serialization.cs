global using Ceras;

namespace Concrete;

public static class Serialization
{
    private static SerializerConfig config = new()
    {
        DefaultTargets = TargetMember.None,
    };

    public static void SaveScene(string path, Scene scene)
    {
        var ceras = new CerasSerializer(config);
        var bytes = ceras.Serialize(scene);
        File.WriteAllBytes(path, bytes);
    }

    public static Scene LoadScene(string path)
    {
        var ceras = new CerasSerializer(config);
        var bytes = File.ReadAllBytes(path);
        var scene = ceras.Deserialize<Scene>(bytes);
        return scene;
    }
}