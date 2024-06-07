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
        camera.transform.position = new Vector3(0, 1, -2);

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
        foreach (var gameObject in activeScene.gameObjects)
        {
            ImGui.PushID(gameObject.name);
            bool nodeOpen = ImGui.TreeNode(gameObject.name);
            if (ImGui.IsItemClicked()) selectedGameObject = gameObject;

            if (nodeOpen)
            {
                /*
                foreach (var child in gameObject.transform.children)
                {
                    ImGui.PushID(child.Name);
                    bool childNodeOpen = ImGui.TreeNode(child.Name);
                    if (ImGui.IsItemClicked())
                    {
                        selectedGameObject = child.gameObject;
                    }
                    if (childNodeOpen)
                    {
                        // RenderGameObjectNode(child);
                        ImGui.TreePop();
                    }
                    ImGui.PopID();
                }
                */
                ImGui.TreePop();
            }
            ImGui.PopID();
        }
        ImGui.End();

        ImGui.Begin("Inspector");
        if (selectedGameObject != null)
        {
            ImGui.Text($"Selected: {selectedGameObject.name}");
            ImGui.Separator();
            foreach (var component in selectedGameObject.components) DrawInspectorComponent(component);
        }
        ImGui.End();
            
        controller.Render();
    }

    public void DrawInspectorComponent(Component component)
    {
        ImGui.Text($"Component: {component.GetType().Name}");

        var type = component.GetType();
        var publicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var publicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in publicFields)
        {
            var fieldType = field.FieldType;
            var fieldName = field.Name;
            var fieldValue = field.GetValue(component);

            if (fieldType == typeof(int))
            {
                int value = (int)fieldValue;
                if (ImGui.InputInt(fieldName, ref value)) field.SetValue(component, value);
            }
            else if (fieldType == typeof(float))
            {
                float value = (float)fieldValue;
                if (ImGui.InputFloat(fieldName, ref value)) field.SetValue(component, value);
            }
            else if (fieldType == typeof(string))
            {
                string value = (string)fieldValue;
                if (ImGui.InputText(fieldName, ref value, 100)) field.SetValue(component, value);
            }
            else if (fieldType == typeof(Vector3))
            {
                Vector3 value = (Vector3)fieldValue;
                if (ImGui.InputFloat3(fieldName, ref value)) field.SetValue(component, value);
            }
        }

        foreach (var property in publicProperties)
        {
            if (!property.CanRead || !property.CanWrite) continue;

            var propertyType = property.PropertyType;
            var propertyName = property.Name;
            var propertyValue = property.GetValue(component);

            if (propertyType == typeof(int))
            {
                int value = (int)propertyValue;
                if (ImGui.InputInt(propertyName, ref value)) property.SetValue(component, value);
            }
            else if (propertyType == typeof(float))
            {
                float value = (float)propertyValue;
                if (ImGui.InputFloat(propertyName, ref value)) property.SetValue(component, value);
            }
            else if (propertyType == typeof(string))
            {
                string value = (string)propertyValue;
                if (ImGui.InputText(propertyName, ref value, 100)) property.SetValue(component, value);
            }
            else if (propertyType == typeof(Vector3))
            {
                Vector3 value = (Vector3)propertyValue;
                if (ImGui.InputFloat3(propertyName, ref value)) property.SetValue(component, value);
            }
        }
    }
    
    public void Resize(Vector2D<int> size)
    {
        opengl.Viewport(size);
    }
}