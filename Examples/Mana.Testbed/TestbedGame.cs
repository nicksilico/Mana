using System;
using System.Collections.Generic;
using ImGuiNET;
using Mana.Graphics;
using Mana.IMGUI;
using Mana.Testbed.Core;
using Mana.Testbed.Examples;
using Mana.Utilities;

namespace Mana.Testbed
{
    public class TestbedGame : Game
    {
        private Color _clearColor = Color.FromHtml("#131313");
        private TestbedExample _currentExample = null;

        private List<TestbedExample> _examples;
        
        public override void Initialize()
        {
            AddGameSystem(new ImGuiSystem());

#if DEBUG
            ConsoleHelper.SetWindowPosition(-1280, 0, 1280, 720);
            AssetManager.RootPath = "../../../Assets";
#else
            AssetManager.RootPath = "./Assets";
#endif
            
            Window.Maximize();
            
            _examples = new List<TestbedExample>
            {
                new RenderingExample(this),
            };
        }

        public override void Update(float time, float deltaTime)
        {
            _currentExample?.Update(time, deltaTime);
        }

        public override void Render(float time, float deltaTime)
        {
            RenderContext.Clear(_clearColor);

            ImGuiHelper.BeginGlobalDocking();

            _currentExample?.Render(time, deltaTime);
            
            DrawExamplesWindow();
            
            DrawMainMenuBar();
        }

        private void DrawMainMenuBar()
        {
            ImGui.BeginMainMenuBar();
            
            ImGui.EndMainMenuBar();
        }

        private void DrawExamplesWindow(int myParam)
        {
            var myLocalVariable = 100;

            Console.WriteLine(myLocalVariable + myParam);
            
            ImGui.Begin("Examples");

            if (ImGuiHelper.Button("None", _currentExample != null))
            {
                _currentExample?.Unload();
                _currentExample = null;
            }
            
            ImGui.Separator();
            
            for (int i = 0; i < _examples.Count; i++)
            {
                var example = _examples[i];
                var enabled = _currentExample != example;
                
                if (ImGuiHelper.Button(example.Name, enabled) && enabled)
                {
                    _currentExample?.Unload();
                    _currentExample = example;
                    _currentExample.InitializeImpl();
                }
            }
            
            ImGui.End();
        }
    }
}