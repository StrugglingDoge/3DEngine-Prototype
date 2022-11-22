using System;
using System.Collections.Generic;
using Assimp;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.ImGui;
using World3DMonoGame._3D;
using World3DMonoGame.Utils;

namespace World3DMonoGame.DebugGUI;

public class DebugGuiRenderer
{

    private static DebugGuiRenderer _instance;
    private List<Action> funcToRun;
    public ImGuiRenderer GuiRenderer; //This is the ImGuiRenderer

    public void Initialize()
    {
        funcToRun = new List<Action>();
        GuiRenderer = new ImGuiRenderer(Game1.Instance).Initialize().RebuildFontAtlas();
    }

    public void AddFunction(Action action)
    {
        if(!funcToRun.Contains(action))
            funcToRun.Add(action);
    }
    
    public void RenderDebugGUI(GraphicsDevice graphicsDevice, GameTime gameTime, FpsCounter counter)
    {
        GuiRenderer.BeginLayout(gameTime);
        foreach (Action action in funcToRun)
        {
            action.Invoke();
        }
        GuiRenderer.EndLayout();
    }
    
    public static DebugGuiRenderer Instance => (_instance ??= new DebugGuiRenderer());

}