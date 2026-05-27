using System.Numerics;
using System.Drawing;

using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;

namespace Concrete;

public static unsafe class SceneWindow
{
    public static SceneCamera sceneCamera = new();
    private static bool sceneWindowFocussed = false;
    private static ImGuizmoOperation guizmoOperation = ImGuizmoOperation.Translate;
    private static ImGuizmoMode guizmoMode = ImGuizmoMode.Local;
    
    public static void Draw(float deltaTime)
    {
        ImGui.Begin("\uf009 Scene", ImGuiWindowFlags.NoScrollbar);
        sceneWindowFocussed = ImGui.IsWindowFocused();

        // update scene camera movement
        if (sceneWindowFocussed) sceneCamera.ApplyMovement(deltaTime);
        
        // render scene to framebuffer
        SceneRenderWindow.framebuffer.Resize(ImGui.GetContentRegionAvail());
        SceneRenderWindow.framebuffer.Bind();
        SceneRenderWindow.framebuffer.Clear(Color.Transparent);

        // render mesh objects in the scene
        SceneManager.RenderSceneObjects(deltaTime, sceneCamera.view, sceneCamera.proj);

        // draw the grid
        GridRenderer.Render(deltaTime, sceneCamera.view, sceneCamera.proj);

        SceneRenderWindow.framebuffer.Unbind();

        // record corner position
        var scenecornerpos = ImGui.GetCursorPos();

        // show framebuffer as image
        var imtexref = new ImTextureRef(null, new ImTextureID(SceneRenderWindow.framebuffer.colorTexture));
        ImGui.Image(imtexref, SceneRenderWindow.framebuffer.size, Vector2.UnitY, Vector2.UnitX);

        // imguizmo
        if (HierarchyWindow.selectedGameObject != null)
        {
            var position = ImGui.GetWindowPos();
            var size = ImGui.GetWindowSize();
            var rect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            IPlatform.Current.GetGL().Viewport(rect);

            ImGuizmo.SetDrawlist();
            ImGuizmo.SetRect(position.X, position.Y, size.X, size.Y);

            Matrix4x4 worldModelMatrix = HierarchyWindow.selectedGameObject.transform.GetWorldModelMatrix();

            Matrix4x4 sview, sproj;
            sview = sceneCamera.view;
            sproj = sceneCamera.proj;
            ImGuizmo.Manipulate(ref sview, ref sproj, guizmoOperation, guizmoMode, ref worldModelMatrix);
            if (ImGuizmo.IsUsing()) HierarchyWindow.selectedGameObject.transform.SetWorldModelMatrix(worldModelMatrix);

            IPlatform.Current.GetGL().Viewport(new Size((int)IPlatform.Current.GetWindowSize().X, (int)IPlatform.Current.GetWindowSize().Y));
        }

        {
            var buttonsize = new Vector2(0, 0);
            var padding = ImGui.GetStyle().WindowPadding;
            var moving = guizmoOperation == ImGuizmoOperation.Translate;
            var rotating = guizmoOperation == ImGuizmoOperation.Rotate;
            var scaling = guizmoOperation == ImGuizmoOperation.Scale;
            var local = guizmoMode == ImGuizmoMode.Local;
            var world = guizmoMode == ImGuizmoMode.World;

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.7f, 0.7f, 0.7f, 1));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.9f, 0.9f, 0.9f, 1));
            ImGui.SetCursorPos(scenecornerpos + padding);
            ImGui.BeginDisabled(moving);
            if (ImGui.Button("\uf047 move", buttonsize)) guizmoOperation = ImGuizmoOperation.Translate;
            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.BeginDisabled(rotating);
            if (ImGui.Button("\uf2f1 rotate", buttonsize)) guizmoOperation = ImGuizmoOperation.Rotate;
            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.BeginDisabled(scaling);
            if (ImGui.Button("\uf31e scale", buttonsize)) guizmoOperation = ImGuizmoOperation.Scale;
            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGuiP.SeparatorEx(ImGuiSeparatorFlags.Vertical, 1);
            ImGui.SameLine();
            ImGui.BeginDisabled(local);
            if (ImGui.Button("\uf1b2 local", buttonsize)) guizmoMode = ImGuizmoMode.Local;
            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.BeginDisabled(world);
            if (ImGui.Button("\uf0ac world", buttonsize)) guizmoMode = ImGuizmoMode.World;
            ImGui.EndDisabled();
            ImGui.PopStyleColor(3);
        }

        ImGui.End();
    }
}