using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mana.Graphics.Geometry;
using Mana.Graphics.Shaders;
using Mana.Graphics.Textures;
using Mana.IMGUI;
using Mana.Utilities;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Color = Mana.Graphics.Color;
using Vector3 = System.Numerics.Vector3;

namespace Mana.Example
{
    public class RenderingGame : ExampleGame
    {
        private static Logger _log = Logger.Create();

        private ShaderProgram _shader;

        private Mesh _mesh;

        private Vector3 _cameraPosition = new Vector3(0, 15, -10);
        private Vector3 _cameraTarget = new Vector3(0, 9, 0);

        private Vector3 _lightPos = new Vector3(0, 0, 0);
        private Color _lightColor = Color.Red;

        private Matrix4x4 _modelMatrix = Matrix4x4.Identity;
        private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

        private Model _nanosuit;
        private TextureCubeMap _skybox0;
        private ShaderProgram _skyboxShader;
        private Mesh _skyboxMesh;

        public override void Initialize()
        {
            base.Initialize();
            
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(1.0f, Window.AspectRatio, 0.1f, 10_000.0f);

            var sw = Stopwatch.StartNew();

            _shader = AssetManager.Load<ShaderProgram>("./Shaders/shader.json", true);
            _mesh = MeshGenerator.CreateMesh(RenderContext, MeshGenerator.GenerateCubeData(Vector3.Zero, 1f, 1f, 1f), Matrix4x4.Identity);

            _skyboxMesh = MeshGenerator.CreateMesh(RenderContext, MeshGenerator.GenerateSkyboxCube(), Matrix4x4.Identity);

            Window.Maximize();

            _skybox0 = AssetManager.Load<TextureCubeMap>("./Textures/skybox1/skybox1.json");
            _skyboxShader = AssetManager.Load<ShaderProgram>("./Shaders/skybox.json");
            _nanosuit = AssetManager.Load<Model>("./Models/Nanosuit/nanosuit.obj");

            sw.Stop();

            _log.Info("Assets loaded in: " + sw.Elapsed.TotalMilliseconds + " ms");
        }

        public override void Update(float time, float deltaTime)
        {
            if (Input.WasKeyPressed(Keys.F11) || (Input.Alt && Input.WasKeyPressed(Keys.Enter)))
                ((ManaWindow)Window).ToggleFullscreen();
        }

        public override void Render(float time, float deltaTime)
        {
            _viewMatrix = Matrix4x4.CreateLookAt(_cameraPosition, _cameraTarget, Vector3Helper.Up);
            _modelMatrix = Matrix4x4.CreateRotationY(0f * 3f);
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(1.0f, Window.AspectRatio, 0.1f, 10_000.0f);

            _shader.SetUniform("model", ref _modelMatrix);
            _shader.SetUniform("view", ref _viewMatrix);
            _shader.SetUniform("projection", ref _projectionMatrix);

            _shader.TrySetUniform("lightPos", _lightPos);
            _shader.TrySetUniform("lightColor", _lightColor.ToVector3());
            _shader.TrySetUniform("cameraPos", _cameraPosition);

            RenderContext.Clear(Color.Black);

            if (_skybox0 != null)
            {
                var viewMatrixNoTranslation = _viewMatrix;
                viewMatrixNoTranslation.Translation = Vector3.Zero;

                _skyboxShader.SetUniform("view", ref viewMatrixNoTranslation);
                _skyboxShader.SetUniform("projection", ref _projectionMatrix);
                _skyboxShader.SetUniform("skybox", 0);

                _skybox0.Bind(0, RenderContext);
                RenderContext.DepthMask = false;
                _skyboxMesh.Render(RenderContext, _skyboxShader);
                RenderContext.DepthMask = true;
            }

            if (_nanosuit != null)
                RenderContext.Render(_nanosuit, _shader);

            _shader.SetUniform("model", Matrix4x4.CreateTranslation(_lightPos));

            _mesh.Render(RenderContext, _shader);

            ImGuiHelper.BeginGlobalDocking(false);
            ImGui.End();

            ImGui.Begin("Controls");
            {
                ImGui.DragFloat3("Camera Position", ref _cameraPosition);
                ImGui.DragFloat3("Camera Target", ref _cameraTarget);

                ImGui.Separator();

                ImGuiHelper.ColorEdit4("Light Color", ref _lightColor);
                ImGui.DragFloat3("Light Position", ref _lightPos);
            }
            ImGui.End();

            if (ImGui.Button("Quit"))
            {
                Quit();
            }

            ImGui.Separator();

            if (Window.VSync == VSyncMode.On)
            {
                if (ImGui.Button("Disable VSync"))
                {
                    Window.VSync = VSyncMode.Off;
                    _log.Info("VSync disabled.");
                }
            }
            else
            {
                if (ImGui.Button("Enable VSync"))
                {
                    Window.VSync = VSyncMode.On;
                    _log.Info("VSync enabled.");
                }
            }

            if (ImGui.Button("Set Fullscreen"))
            {
                Window.FullscreenState = FullscreenState.Fullscreen;
            }

            if (ImGui.Button("Set Windowed"))
            {
                Window.FullscreenState = FullscreenState.Windowed;
            }

            ImGui.End();

            ImGui.ShowMetricsWindow();
        }
    }
}
