namespace Concrete;

public static class ProjectManager
{
    public static string loadedProjectFilePath = null;
    public static ProjectData loadedProjectData = null;

    public static string projectRoot => Directory.GetParent(Path.GetFullPath(loadedProjectFilePath)).FullName;

    public static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static string concreteDataPath = Path.Combine(documentsPath, "Concrete");
    public static string lastProjectMemoryPath = Path.Combine(concreteDataPath, "LastProject.txt");
    public static string tempProjectPath = Path.Combine(concreteDataPath, "TempProject");

    public static void CreateAndLoadTempProject()
    {
        Debug.Log("Creating and loading a temporary project.");

        // make empty temp project directory
        if (Directory.Exists(tempProjectPath)) Directory.Delete(tempProjectPath, true);
        Directory.CreateDirectory(tempProjectPath);

        // create and load and remember temp project
        string projectfilepath = Path.Combine(tempProjectPath, "project.json");
        ProjectSerializer.NewProjectFile(projectfilepath);
        LoadProjectDir(tempProjectPath, true);
        Directory.CreateDirectory(Path.Combine(tempProjectPath, "Scenes"));
        Directory.CreateDirectory(Path.Combine(tempProjectPath, "Scripts"));
    }

    // ----

    public static void NewProjectDir(string dir)
    {
        var filepath = Path.Combine(dir, "project.json");
        ProjectSerializer.NewProjectFile(filepath);
        LoadProjectDir(dir);
        Directory.CreateDirectory("Scenes");
        Directory.CreateDirectory("Scripts");
    }

    public static void SaveProjectDir(string dir)
    {
        if (dir != projectRoot)
        {
            CopyDirectory(projectRoot, dir);
            LoadProjectDir(dir);
        }
        else
        {
            Debug.Log("Project is already up to date.");
        }
    }

    // the scripts assembly needs to be loaded before this function if the project contains scenes that contain scripts
    // its needed for deserializing a scene containing scripts, the scene deserializer needs to know about the script types
    // the editor needs to manually make a call to compile scripts before ever calling this function
    // the player loads a Scripts.dll into memory as a file that gets placed in the exported game directory by the editor when building the game
    // this function is however allowed to be called without scripts assembly being loaded if the project dir doesnt contain scenes with scripts
    // for example when loading an empty temp project, or when creating a new project and loading that
    public static void LoadProjectDir(string dir, bool isTemp = false)
    {
        var path = Path.Combine(dir, "project.json");
        if (!File.Exists(path)) File.Create(path);

        // load project
        loadedProjectFilePath = path;
        loadedProjectData = ProjectSerializer.LoadProjectFile(path);
        IPlatform.Current.SetWindowTitle("Concrete Engine [" + Path.GetFullPath(loadedProjectFilePath) + "]");

        // initialize asset database
        AssetDatabase.Rebuild();

        // if the project has a scene, and that scene contains gameobject, and those gameobjects have scripts
        // then the scripts assembly needs to be loaded into memory before deserializing the scene, or it will crash

        // try to load startup scene
        if (loadedProjectData.firstScene != "")
        {
            string sceneRelativePath = AssetDatabase.GetPath(Guid.Parse(loadedProjectData.firstScene));
            string sceneFullPath = Path.Combine(projectRoot, sceneRelativePath);
            SceneManager.LoadScene(sceneFullPath);
        }
        else
        {
            SceneManager.CreateAndLoadNewScene();
        }
        
        if (!isTemp)
        {
            // remember project
            if (File.Exists(lastProjectMemoryPath)) File.Delete(lastProjectMemoryPath);
            File.WriteAllText(lastProjectMemoryPath, path);
            Debug.Log("Remembered the newly loaded project.");
        }

        // rebuild shared ref for scripts
        AfterProjectLoad(dir);
    }

    private static void CopyDirectory(string source, string dest)
    {
        // ensure existence
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException($"directory not found: {source}");

        // create destination
        Directory.CreateDirectory(dest);

        // copy files
        foreach (string file in Directory.GetFiles(source))
        {
            string destFile = Path.Combine(dest, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        // copy dirs
        foreach (string subdir in Directory.GetDirectories(source))
        {
            string dubdirdest = Path.Combine(dest, Path.GetFileName(subdir));
            CopyDirectory(subdir, dubdirdest);
        }
    }

    // ----

    static void AfterProjectLoad(string dir)
    {
        // create hidden concrete folder
        string hidden = Path.Combine(dir, ".concrete");
        if (!Directory.Exists(hidden)) Directory.CreateDirectory(hidden);
        new DirectoryInfo(hidden).Attributes |= FileAttributes.Hidden;

        // rebuild csproj file
        string csproj = Path.Combine(dir, "project.csproj");
        if (File.Exists(csproj)) File.Delete(csproj);
        string[] properties =
        [
            "<OutputType>library</OutputType>",
            "<TargetFramework>net10.0</TargetFramework>",
            "<ImplicitUsings>enable</ImplicitUsings>",
            "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>",
            "<DebugType>embedded</DebugType>",
            "<SatelliteResourceLanguages>none</SatelliteResourceLanguages>",
            "<BaseOutputPath>.concrete/bin/</BaseOutputPath>",
            "<BaseIntermediateOutputPath>.concrete/obj/</BaseIntermediateOutputPath>",
            "<RestoreOutputPath>.concrete/obj/</RestoreOutputPath>",
        ];
        Dotnet.New(csproj, properties);

        // add the editor's shared assembly as a reference for script autocomplete
        Dotnet.AddDll(csproj, Path.GetFullPath("Shared.dll"));

        // make sure gitignore exists
        string[] ignores = ["*.csproj", "bin/", "obj/", ".idea/", ".vscode/", ".vs/", ".concrete/"];
        string gitignore_contents = "";
        foreach (var ignore in ignores)
        {
            bool last = ignore == ignores[ignores.Length - 1];
            gitignore_contents += last ? ignore : ignore + "\n";
        }
        string gitignore_path = Path.Combine(dir, ".gitignore");
        if (!File.Exists(gitignore_path)) File.WriteAllText(gitignore_path, gitignore_contents);
    }
}