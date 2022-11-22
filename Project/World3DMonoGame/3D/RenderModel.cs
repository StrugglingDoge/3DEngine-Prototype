using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assimp;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.ImGui.Extensions;
using World3DMonoGame.DebugGUI;

namespace World3DMonoGame._3D;

public class RenderModel
{
    // Need to keep an instance of the GraphicsDevice so that we can set the vertex buffer.
    private GraphicsDevice GraphicsDevice;
    private static RenderModel _instance;

    public static RenderModel Instance => _instance;
    
    private Dictionary<string, Model> models;
    private Dictionary<Model, Matrix[]> transforms;

    private float fogStart = 150f;
    private float fogEnd = 1000f;
    private Vector3 fogColor = Color.Blue.ToVector3();
    private bool fogEnabled = false;
    
    public RenderModel(GraphicsDevice gDevice)
    {
        GraphicsDevice = gDevice;
        _instance = this;
        models = new Dictionary<string, Model>();
        transforms = new Dictionary<Model, Matrix[]>();
        DebugGuiRenderer.Instance.AddFunction(() =>
        {
            ImGui.Begin("Model Renderer");
            ImGui.Text($"Fog: ");
            ImGui.Checkbox("Enable Fog", ref fogEnabled);
            var fogNum = fogColor.ToNumerics();
            ImGui.ColorEdit3("Fog Color", ref fogNum);
            fogColor = fogNum.ToXnaVector3();
            ImGui.SliderFloat("Fog Start", ref fogStart, 0f, 650f);
            ImGui.SliderFloat("Fog End", ref fogEnd, 650f, 2000f);
            ImGui.End();
            ImGui.Begin("Render Techniques");
            foreach (var model in models.Values)
            {
                if (ImGui.CollapsingHeader($"{model.Root.Name}"))
                {
                    foreach (ModelMesh mesh in model.Meshes)
                    {
                        ImGui.Indent();
                        int count = 0;
                        foreach (var parts in mesh.MeshParts)
                        {
                            if (ImGui.CollapsingHeader($"Part {count}"))
                            {
                                ImGui.Text($"Vertices: {parts.NumVertices}");
                                ImGui.Text($"Buffer Usage: {parts.VertexBuffer.BufferUsage}");
                                ImGui.Text($"Vertex Count: {parts.VertexBuffer.VertexCount}");
                                ImGui.Text($"Vertex Offset: {parts.VertexOffset}");
                            }

                            count++;
                        }

                        foreach (Effect effect in mesh.Effects)
                        {
                            ImGui.Text($"Technique: {effect.CurrentTechnique.Name}");
                        }
                        ImGui.Unindent();
                    }
                }
            }
            ImGui.End();
        });
    }

    public void AddModel(string identifier, Model model, Matrix viewMatrix)
    {
        models.Add(identifier, model);
    }

    public Model GetModel(string modelName)
    {
        foreach (var model in models.Values)
        {
            if (models[modelName] == model)
            {
                return model;
            }
        }

        return null;
    }

    public void TransformModel(string modelName, Matrix matrix)
    {
        // To apply a matrix to the transform, simply multiply the transforms in the model with the provided one.
        var model = GetModel(modelName);
        for (int i = 0; i < transforms[model].Length; i++)
        {
            transforms[model][i] *= matrix;
        }
    }

    public void DrawModels(Matrix projectionMatrix, Matrix viewMatrix, float alpha = 1f)
    {
        foreach (var model in models.Values)
        {
            // Need model transforms to update the positions.
            if (!transforms.ContainsKey(model))
            {
                // Transforms are stored into a list to save on resources and prevent performance issues.
                transforms.Add(model, new Matrix[model.Bones.Count]);
                model.CopyAbsoluteBoneTransformsTo(transforms[model]);
            }

            foreach (ModelMesh mesh in model.Meshes)
            {
                var world = transforms[model][mesh.ParentBone.Index];
                foreach (Effect effect in mesh.Effects)
                {
                    // TODO: Find a way to draw lighting onto models compiled with AlphaTestEffect.
                    if (effect is AlphaTestEffect)
                    {
                        var eff = (AlphaTestEffect) effect;
                        eff.View = viewMatrix;
                        eff.FogEnabled = fogEnabled;
                        eff.FogStart = fogStart;
                        eff.FogEnd = fogEnd;
                        eff.FogColor = fogColor;
                        eff.Projection = projectionMatrix;
                        eff.World = world;
                    }

                    if (effect is BasicEffect)
                    {
                        var eff = (BasicEffect) effect;
                        eff.EnableDefaultLighting();
                        eff.World = world;
                        eff.View = viewMatrix;
                        eff.Projection = projectionMatrix;
                        eff.LightingEnabled = true; // turn on the lighting subsystem.
                        eff.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0, 0); // a red light
                        eff.DirectionalLight0.Direction = new Vector3(1, 0, 0);  // coming along the x-axis
                        eff.DirectionalLight0.SpecularColor = new Vector3(0, 1, 0); // with green highlights
                    }
                    
                    effect.CurrentTechnique.Passes[0].Apply();
                }
                mesh.Draw();
            }
        }
    }

}