using System.Numerics;
using System.Drawing;
using System.Reflection;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;

namespace Concrete;

public static unsafe class Editor
{
    private static int selectedGameObjectIdentifier;
    private static GameObject selectedGameObject
    {
        get => SceneManager.loadedScene.FindGameObject(selectedGameObjectIdentifier);
        set => selectedGameObjectIdentifier = value.id;
    }

    public static SceneProjection sceneProjection = new();
    public static Framebuffer sceneWindowFramebuffer = new();
    public static Framebuffer gameWindowFramebuffer = new();
    
    private static bool dockbuilderInitialized = false;
    private static bool sceneWindowFocussed = false;
    private static bool gameWindowFocussed = false;

    public static void Update(float deltaTime)
    {
        float sceneWindowAspect = (float)sceneWindowFramebuffer.size.X / (float)sceneWindowFramebuffer.size.Y;
        sceneProjection.UpdateProjection(sceneWindowAspect);
        if (sceneWindowFocussed) sceneProjection.ApplyMovement(deltaTime);
    }

    public static void Render(float deltaTime)
    {
        if (ImGui.BeginMainMenuBar())
        {
            float buttonWidth = 64;
            float spacing = ImGui.GetStyle().ItemSpacing.X;
            float half = Engine.window.Size.X / 2;
            ImGui.SetCursorPosX(half - buttonWidth * 1.5f - spacing);
            var size = new Vector2(buttonWidth, 0);

            var stopped = SceneManager.playerState == PlayerState.stopped;
            var playing = SceneManager.playerState == PlayerState.playing;
            var paused = SceneManager.playerState == PlayerState.paused;

            ImGui.BeginDisabled(playing || paused);
            if (ImGui.Button("play", size))
            {
                SceneManager.Play();
                ImGui.FocusWindow(ImGui.FindWindowByName("Game"), ImGuiFocusRequestFlags.None);
            }
            ImGui.EndDisabled();

            ImGui.BeginDisabled(stopped);
            if (ImGui.Button(paused ? "continue" : "pause", size))
            {
                if (paused) SceneManager.Continue();
                else SceneManager.Pause();
            }
            ImGui.EndDisabled();

            ImGui.BeginDisabled(stopped);
            if (ImGui.Button("stop", size)) SceneManager.Stop();
            ImGui.EndDisabled();

            ImGui.EndMainMenuBar();
        }

        int dockspace = ImGui.DockSpaceOverViewport((ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.NoWindowMenuButton);
        if (!dockbuilderInitialized)
        {
            int leftdock, rightdock = 0;
            ImGui.DockBuilderSplitNode(dockspace, ImGuiDir.Left, 0.25f, &leftdock, &rightdock);
            int topleftdock, bottomleftdock = 0;
            ImGui.DockBuilderSplitNode(leftdock, ImGuiDir.Up, 0.5f, &bottomleftdock, &topleftdock);
            ImGui.DockBuilderDockWindow("Scene", rightdock);
            ImGui.DockBuilderDockWindow("Game", rightdock);
            ImGui.DockBuilderDockWindow("Hierarchy", topleftdock);
            ImGui.DockBuilderDockWindow("Inspector", bottomleftdock);
            ImGui.DockBuilderFinish(dockspace);
            dockbuilderInitialized = true;
        }
        
        ImGui.Begin("Scene", ImGuiWindowFlags.NoScrollbar);
        sceneWindowFocussed = ImGui.IsWindowFocused();

        sceneWindowFramebuffer.Resize(ImGui.GetContentRegionAvail());
        sceneWindowFramebuffer.Bind();
        sceneWindowFramebuffer.Clear(Color.DarkGray);
        SceneManager.Render(deltaTime, sceneProjection.projection);
        sceneWindowFramebuffer.Unbind();

        ImGui.Image((nint)sceneWindowFramebuffer.colorTexture, sceneWindowFramebuffer.size, Vector2.UnitY, Vector2.UnitX);

        // imguizmo
        if (selectedGameObject != null)
        {
            var position = ImGui.GetWindowPos();
            var size = ImGui.GetWindowSize();
            var rect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            Engine.opengl.Viewport(rect);

            ImGuizmo.SetDrawlist();
            ImGuizmo.SetRect(position.X, position.Y, size.X, size.Y);

            Matrix4x4 worldModelMatrix = selectedGameObject.transform.GetWorldModelMatrix();
            ImGuizmo.Manipulate(ref sceneProjection.projection.view, ref sceneProjection.projection.proj, ImGuizmoOperation.Translate, ImGuizmoMode.World, ref worldModelMatrix);
            if (ImGuizmo.IsUsing()) selectedGameObject.transform.worldPosition = worldModelMatrix.Translation;

            Engine.opengl.Viewport(Engine.window.Size);
        }
            
        ImGui.End();

        ImGui.Begin("Game", ImGuiWindowFlags.NoScrollbar);
        gameWindowFocussed = ImGui.IsWindowFocused();

        gameWindowFramebuffer.Resize(ImGui.GetContentRegionAvail());
        gameWindowFramebuffer.Bind();
        gameWindowFramebuffer.Clear(Color.DarkGray);
        SceneManager.Render(deltaTime, SceneManager.loadedScene.FindAnyCamera().Project());
        gameWindowFramebuffer.Unbind();

        ImGui.Image((nint)gameWindowFramebuffer.colorTexture, gameWindowFramebuffer.size, Vector2.UnitY, Vector2.UnitX);
        ImGui.End();

        ImGui.Begin("Hierarchy");
        foreach (var gameObject in SceneManager.loadedScene.gameObjects) if (gameObject.transform.parent == null) DrawHierarchyMember(gameObject);
        ImGui.InvisibleButton("", ImGui.GetContentRegionAvail());
        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
            if (!payload.IsNull)
            {
                var dragged = SceneManager.loadedScene.FindGameObject(*(int*)payload.Data);
                if (dragged != null) dragged.transform.parent = null;
            }
            ImGui.EndDragDropTarget();
        }
        ImGui.End();

        ImGui.Begin("Inspector");
        if (selectedGameObject != null)
        {
            ImGui.PushID(selectedGameObject.id);
            ImGui.Checkbox("", ref selectedGameObject.enabled);
            ImGui.PopID();
            ImGui.SameLine();
            ImGui.InputText("", ref selectedGameObject.name, 100);
            ImGui.Separator();
            foreach (var component in selectedGameObject.components) DrawComponent(component);
        }
        ImGui.End();
    }

    public static void DrawHierarchyMember(GameObject gameObject)
    {
        int id = gameObject.id;
        ImGui.PushID(id);

        var flags = ImGuiTreeNodeFlags.OpenOnArrow;
        if (gameObject.transform.children.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
        if (selectedGameObject == gameObject) flags |= ImGuiTreeNodeFlags.Selected;
        bool open = ImGui.TreeNodeEx(gameObject.name, flags);
        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen()) selectedGameObject = gameObject;

        if (ImGui.BeginDragDropSource())
        {
            ImGui.SetDragDropPayload(nameof(GameObject), &id, sizeof(int));
            ImGui.Text(gameObject.name);
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
            if (!payload.IsNull)
            {
                var dragged = SceneManager.loadedScene.FindGameObject(*(int*)payload.Data);
                if (dragged != null && !dragged.transform.children.Contains(gameObject.transform)) dragged.transform.parent = gameObject.transform;
            }
            ImGui.EndDragDropTarget();
        }

        if (open) foreach (var child in gameObject.transform.children) DrawHierarchyMember(child.gameObject);

        ImGui.TreePop();
        ImGui.PopID();
    }

    public static void DrawComponent(Component component)
    {
        var type = component.GetType();
        if (ImGui.CollapsingHeader(type.Name))
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields) DrawField(field, component);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite);
            foreach (var property in properties) DrawProperty(property, component);
        }
    }

    private static void DrawField(FieldInfo field, Component component)
    {
        bool show = false;
        string showname = null;
        foreach (var attribute in field.GetCustomAttributes()) if (attribute is ShowAttribute showAttribute)
        {
            show = true;
            showname = showAttribute.name;
            break;
        }
        if (!show) return;

        var type = field.FieldType;
        var name = showname == null ? field.Name : showname;
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

    private static void DrawProperty(PropertyInfo property, Component component)
    {
        bool show = false;
        string showname = null;
        foreach (var attribute in property.GetCustomAttributes()) if (attribute is ShowAttribute showAttribute)
        {
            show = true;
            showname = showAttribute.name;
            break;
        }
        if (!show) return;

        var type = property.PropertyType;
        var name = showname == null ? property.Name : showname;
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
}