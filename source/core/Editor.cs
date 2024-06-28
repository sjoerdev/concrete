using System.Numerics;
using System.Drawing;
using System.Reflection;
using Hexa.NET.ImGui;

namespace GameEngine;

public unsafe class Editor
{
    Framebuffer sceneWindowFramebuffer = null;
    GameObject selectedGameObject = null;
    SceneProjection sceneProjection = null;

    bool sceneWindowFocussed = false;
    bool dockbuilderInitialized = false;

    public Editor()
    {
        sceneWindowFramebuffer = new Framebuffer();
        sceneProjection = new SceneProjection();
        ImGui.GetIO().Handle->IniFilename = null;
        ImGui.GetIO().ConfigFlags = ImGuiConfigFlags.DockingEnable;
    }

    public void Update(float deltaTime)
    {
        float sceneWindowAspect = (float)sceneWindowFramebuffer.size.X / (float)sceneWindowFramebuffer.size.Y;
        sceneProjection.UpdateProjection(sceneWindowAspect);
        if (sceneWindowFocussed) sceneProjection.ApplyMovement(deltaTime);
    }

    public void Render(float deltaTime)
    {
        int dockspace = ImGui.DockSpaceOverViewport((ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.NoWindowMenuButton);
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
        sceneWindowFocussed = ImGui.IsWindowFocused();

        sceneWindowFramebuffer.Resize(ImGui.GetContentRegionAvail());
        sceneWindowFramebuffer.Enable();
        sceneWindowFramebuffer.Clear(Color.DarkGray);
        Engine.sceneManager.Render(deltaTime, sceneProjection.projection);
        sceneWindowFramebuffer.Disable();

        ImGui.Image((nint)sceneWindowFramebuffer.colorTexture, sceneWindowFramebuffer.size, Vector2.UnitY, Vector2.UnitX);
        ImGui.End();

        ImGui.Begin("Hierarchy");
        foreach (var gameObject in Engine.sceneManager.loadedScene.gameObjects) if (gameObject.transform.parent == null) DrawHierarchyMember(gameObject);
        ImGui.InvisibleButton("", ImGui.GetContentRegionAvail());
        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
            if (!payload.IsNull)
            {
                var dragged = Engine.sceneManager.loadedScene.FindGameObject(*(int*)payload.Data);
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

    public void DrawHierarchyMember(GameObject gameObject)
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
                var dragged = Engine.sceneManager.loadedScene.FindGameObject(*(int*)payload.Data);
                if (dragged != null && !dragged.transform.children.Contains(gameObject.transform)) dragged.transform.parent = gameObject.transform;
            }
            ImGui.EndDragDropTarget();
        }

        if (open) foreach (var child in gameObject.transform.children) DrawHierarchyMember(child.gameObject);

        ImGui.TreePop();
        ImGui.PopID();
    }

    public void DrawComponent(Component component)
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

    private void DrawField(FieldInfo field, Component component)
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

    private void DrawProperty(PropertyInfo property, Component component)
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