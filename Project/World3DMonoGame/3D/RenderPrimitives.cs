using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace World3DMonoGame._3D;

public class RenderPrimitives
{
    
    // Need to keep an instance of the GraphicsDevice so that we can set the vertex buffer.
    private GraphicsDevice GraphicsDevice;
    // A Basic effect that is used if one is not provided for the vertices (no lighting)
    private BasicEffect DefaultEffect;
    // A List that contains all of the buffers to be drawn on screen.
    private List<VertexBuffer> RenderBuffer;
    // A List of Vertex arrays that contain the positions and color of the passed in verts.
    private List<VertexPositionColor[]> verticies;
    
    public RenderPrimitives(GraphicsDevice gDevice)
    {
        GraphicsDevice = gDevice;
        
        // Set up the basic effect, only need to do once.
        DefaultEffect = new BasicEffect(gDevice);
        DefaultEffect.Alpha = 1.0f;
        // Since these are primitives, we want to allow the vertices to have color values.
        DefaultEffect.VertexColorEnabled = true;
        DefaultEffect.LightingEnabled = false;

        // Initialize the actual vert list.
        verticies = new List<VertexPositionColor[]>();
    }

    public void AddPrimitive(VertexPositionColor[] verts)
    {
        //TODO: Add a way to pass in custom effects later.
        verticies.Add(verts);
    }

    public void UpdateVerts()
    {
        // Don't update if there isn't anything to be updated.
        if (verticies.Count < 1)
            return;
        // RenderBuffer array needs to be reset every update (We don't want to draw old verts)
        RenderBuffer = new List<VertexBuffer>();
        foreach (var vertarr in verticies)
        {
            VertexBuffer curBuff;
            // Create a buffer and set the data of the said buffer.   
            RenderBuffer.Add(curBuff = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), vertarr.Length, BufferUsage.WriteOnly));
            curBuff.SetData(vertarr);
        }
    }

    public void DrawVerts(Matrix projectionMatrix, Matrix viewMatrix, Matrix worldMatrix)
    {
        // Don't draw if there isn't anything to be drawn.
        if (verticies.Count < 1)
            return;
        // Update the effects matrices.
        DefaultEffect.Projection = projectionMatrix;
        DefaultEffect.View = viewMatrix;
        DefaultEffect.World = worldMatrix;

        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Loop through all of the created vertex buffers and draw
        foreach (var verBuf in RenderBuffer)
        {
            // Set the graphics device's vertex buffer to the current instance we need to draw.
            GraphicsDevice.SetVertexBuffer(verBuf);

            //Turn off culling so we see both sides of our rendered triangle
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            // Apply the basic effect to the vertex'.
            foreach (EffectPass pass in DefaultEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                // Finally, render the buffer.
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, verBuf.VertexCount);
            }
        }
    }
    
}