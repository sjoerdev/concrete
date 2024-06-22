using System.Numerics;
using System.Drawing;
using System.Reflection;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Hexa.NET.ImGui;

namespace GameEngine;

unsafe class Engine
{
    public static GL opengl;
    public static IWindow window;
    public static IInputContext input;
    public static ImGuiController controller;
    
    public static Scene activeScene = null;
    public static List<Scene> scenes = [];

    public static Camera activeCamera = null;
    public static Framebuffer framebuffer = null;

    public static List<DirectionalLight> directionalLights = [];
    public static List<PointLight> pointLights = [];
    public static List<SpotLight> spotLights = [];

    bool dockbuilderInitialized = false;
    GameObject selectedGameObject = null;

    public Engine()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "untitled";
        window = Window.Create(options);
        window.Load += Start;
        window.Update += Update;
        window.Render += Render;
        window.FramebufferResize += Resize;
        window.Run();
        window.Dispose();
    }

    public void Start()
    {
        opengl = GL.GetApi(window);
        input = window.CreateInput();
        controller = new ImGuiController(opengl, window, input);
        framebuffer = new Framebuffer();
        
        ImGui.GetIO().Handle->IniFilename = null;
        ImGui.GetIO().ConfigFlags = ImGuiConfigFlags.DockingEnable;

        new Scene().SetActive();

        var camera = new GameObject();
        camera.AddComponent<Camera>().SetActive();
        camera.AddComponent<SpotLight>();
        camera.transform.localPosition = new Vector3(0, 1, -2);

        var model = new GameObject();
        model.AddComponent<MeshRenderer>().modelPath = "resources/models/testmodel.glb";
        
        activeScene?.Start();
    }

    public void Update(double deltaTime)
    {
        activeScene?.Update((float)deltaTime);
        controller.Update((float)deltaTime);
    }

    public void Render(double deltaTime)
    {
        opengl.Enable(EnableCap.DepthTest);
        opengl.ClearColor(Color.Black);
        opengl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        int dockspace = ImGui.DockSpaceOverViewport();
        if (!dockbuilderInitialized)
        {
            int leftdock, rightdock = 0;
            ImGui.DockBuilderSplitNode(dockspace, ImGuiDir.Left, 0.25f, &leftdock, &rightdock);
            int topleftdock, bottomleftdock = 0;
            ImGui.DockBuilderSplitNode(leftdock, ImGuiDir.Up, 0.5f, &bottomleftdock, &topleftdock);
            ImGui.DockBuilderDockWindow("Scene", rightdock);
            ImGui.DockBuilderDockWindow("Hierarchy", topleftdock);
            ImGui.DockBuilderDockWindow("Inspector", bottomleftdock);
            ImGui.DockBuilderFinish(dockspace);
            dockbuilderInitialized = true;
        }
        
        ImGui.Begin("Scene", ImGuiWindowFlags.NoScrollbar);
        framebuffer.Resize(ImGui.GetContentRegionAvail());
        framebuffer.Enable();
        framebuffer.Clear(Color.DarkGray);
        activeScene?.Render((float)deltaTime);
        framebuffer.Disable();
        ImGui.Image((nint)framebuffer.colorTexture, framebuffer.size, Vector2.UnitY, Vector2.UnitX);
        ImGui.End();

        ImGui.Begin("Hierarchy");
        foreach (var gameObject in activeScene.gameObjects) if (gameObject.transform.parent == null) DrawHierarchyMember(gameObject);
        ImGui.End();

        ImGui.Begin("Inspector");
        if (selectedGameObject != null)
        {
            ImGui.Text(selectedGameObject.name.ToString());
            ImGui.Separator();
            foreach (var component in selectedGameObject.components) DrawComponent(component);
        }
        ImGui.End();
        
        controller.Render();
    }

    public void DrawHierarchyMember(GameObject gameObject)
    {
        ImGui.PushID(gameObject.name);

        var flags = ImGuiTreeNodeFlags.OpenOnArrow;

        if (gameObject.transform.children.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
        if (selectedGameObject == gameObject) flags |= ImGuiTreeNodeFlags.Selected;

        bool open = ImGui.TreeNodeEx(gameObject.name, flags);

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen()) selectedGameObject = gameObject;

        if (open) foreach (var child in gameObject.transform.children) DrawHierarchyMember(child.gameObject);

        ImGui.TreePop();

        ImGui.PopID();
    }

    public void DrawComponent(Component component)
    {
        var type = component.GetType();
        ImGui.Text(type.Name);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields) DrawField(field, component);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite);
        foreach (var property in properties) DrawProperty(property, component);
        ImGui.Separator();
    }

    private void DrawField(FieldInfo field, Component component)
    {
        var type = field.FieldType;
        var name = field.Name;
        var curvalue = field.GetValue(component);

        if (type == typeof(int))
        {
            int value = (int)curvalue;
            if (ImGui.DragInt(name, ref value)) field.SetValue(component, value);
        }
        else if (type == typeof(float))
        {
            float value = (float)curvalue;
            if (ImGui.DragFloat(name, ref value, 0.1f)) field.SetValue(component, value);
        }
        else if (type == typeof(string))
        {
            string value = (string)curvalue;
            if (ImGui.InputText(name, ref value, 100)) field.SetValue(component, value);
        }
        else if (type == typeof(Vector3))
        {
            Vector3 value = (Vector3)curvalue;
            if (ImGui.DragFloat3(name, ref value, 0.1f)) field.SetValue(component, value);
        }
        else if (type == typeof(Vector2))
        {
            Vector2 value = (Vector2)curvalue;
            if (ImGui.DragFloat2(name, ref value, 0.1f)) field.SetValue(component, value);
        }
        else if (type == typeof(bool))
        {
            bool value = (bool)curvalue;
            if (ImGui.Checkbox(name, ref value)) field.SetValue(component, value);
        }
    }

    private void DrawProperty(PropertyInfo property, Component component)
    {
        var type = property.PropertyType;
        var name = property.Name;
        var curvalue = property.GetValue(component);

        if (type == typeof(int))
        {
            int value = (int)curvalue;
            if (ImGui.DragInt(name, ref value)) property.SetValue(component, value);
        }
        else if (type == typeof(float))
        {
            float value = (float)curvalue;
            if (ImGui.DragFloat(name, ref value, 0.1f)) property.SetValue(component, value);
        }
        else if (type == typeof(string))
        {
            string value = (string)curvalue;
            if (ImGui.InputText(name, ref value, 100)) property.SetValue(component, value);
        }
        else if (type == typeof(Vector3))
        {
            Vector3 value = (Vector3)curvalue;
            if (ImGui.DragFloat3(name, ref value, 0.1f)) property.SetValue(component, value);
        }
        else if (type == typeof(Vector2))
        {
            Vector2 value = (Vector2)curvalue;
            if (ImGui.DragFloat2(name, ref value, 0.1f)) property.SetValue(component, value);
        }
        else if (type == typeof(bool))
        {
            bool value = (bool)curvalue;
            if (ImGui.Checkbox(name, ref value)) property.SetValue(component, value);
        }
    }
    
    public void Resize(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}