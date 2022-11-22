using System;
using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Collections;
using MonoGame.ImGui;
using World3DMonoGame._3D;
using World3DMonoGame.DebugGUI;
using World3DMonoGame.Utils;

namespace World3DMonoGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private GameTime _gameTime;
    private static Game1 _instance;
    SpriteBatch spriteBatch;

    // Camera
    private Basic3DCamera cam;

    private FpsCounter counter;

    private Model model;
    private Model map;
    
    private bool Orbit;

    private RenderPrimitives rp;
    
    private RenderTarget2D ImGUI_RenderTarget;    
    private RenderTarget2D Game_RenderTarget;
    
    public Game1()
    {
        _instance = this;
        _graphics = new GraphicsDeviceManager(this);
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
        _graphics.PreferMultiSampling = true;
        //Needed or ImGui font will draw really weirdly
        _graphics.PreferHalfPixelOffset = true;
        //Why not START off with enabling unlimited FPS?
        _graphics.SynchronizeWithVerticalRetrace = false;
        this.IsFixedTimeStep = false;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        Window.AllowUserResizing = true;
        IsMouseVisible = true;
        Window.ClientSizeChanged += (sender, args) =>
        {
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
            cam.ReCreateThePerspectiveProjectionMatrix(GraphicsDevice, cam.fieldOfViewDegrees);
            ImGUI_RenderTarget =
                new RenderTarget2D(GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
        };

        counter = new FpsCounter();
        
        
    }

    protected override void Initialize()
    {
        DebugGuiRenderer.Instance.Initialize();
        base.Initialize();
        Game_RenderTarget = new RenderTarget2D(GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
        ImGUI_RenderTarget =
            new RenderTarget2D(GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);

        //Camera Init
        cam = new Basic3DCamera(GraphicsDevice, this.Window);
        cam.Position = new Vector3(0, 0, -10);
        cam.LookAtDirection = Vector3.Forward;
        cam.CameraType(0);
        
        rp = new RenderPrimitives(GraphicsDevice);
        new RenderModel(GraphicsDevice);
        RenderModel.Instance.AddModel("MonoCube", model, cam.View);
        RenderModel.Instance.AddModel("map", map, cam.View);
        DebugGuiRenderer.Instance.AddFunction(() =>
        {
            // Enable docking of windows.
            ImGui.GetIO().ConfigFlags = (ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.DpiEnableScaleFonts);
            //START GAME RENDER WINDOW
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0,0)); // Dock window in top-left corner of screen.
            // minimum size of 50x50 & max size of screen width&height divided by 2.
            ImGui.SetNextWindowSizeConstraints(new System.Numerics.Vector2(50, 50),
                new System.Numerics.Vector2(Window.ClientBounds.Width/2f, Window.ClientBounds.Height/2f));
            // Don't let window be collapsed
            ImGui.Begin("Game Render", ImGuiWindowFlags.NoCollapse);
            // Weird problem where window won't render properly without this?
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new System.Numerics.Vector2(50, 50));
            ImGui.BeginChild("GameRender");
            Vector2 size = ImGui.GetWindowSize();
            // Update render target size if window size has changed.
            if (Game_RenderTarget.Width != size.X || Game_RenderTarget.Height != size.Y)
                Game_RenderTarget = new RenderTarget2D(GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            // Included MonoGame.ImGui library function that allows to bind and get the pointer of the render target.
            IntPtr pObj = DebugGuiRenderer.Instance.GuiRenderer.BindTexture(Game_RenderTarget);
            // Render the image to the window.
            ImGui.Image(pObj, size.ToNumerics());
            ImGui.EndChild();
            ImGui.End();
            //END GAME RENDER WINDOW
            ImGui.PopStyleVar();
            ImGui.Begin("Camera");
            ImGui.Text($"Look Direction: {cam.LookAtDirection.X}, {cam.LookAtDirection.Y}, {cam.LookAtDirection.Z}");
            ImGui.Text($"Camera XYZ: {cam.Position.X}, {cam.Position.Y}, {cam.Position.Z}");

            if (ImGui.Button("Reset Camera"))
            {
                cam.Position = new Vector3(0, 0, -10);
                cam.LookAtDirection = Vector3.Forward;
            }

            if (ImGui.Button("Reset Block"))
            {
                RenderModel.Instance.TransformModel("MonoCube",Matrix.CreateRotationX(0));
                RenderModel.Instance.TransformModel("MonoCube",Matrix.CreateRotationY(0));
                RenderModel.Instance.TransformModel("MonoCube",Matrix.CreateRotationZ(0));
            }

            if (ImGui.SliderFloat("Camera FOV", ref cam.fieldOfViewDegrees, 70f, 120f))
            {
                cam.ReCreateThePerspectiveProjectionMatrix(GraphicsDevice, cam.fieldOfViewDegrees);
            }

            var movementSpeed = cam.MovementUnitsPerSecond;
            if (ImGui.SliderFloat("Camera Speed", ref movementSpeed, 10f, 250f))
            {
                cam.MovementUnitsPerSecond = movementSpeed;
            }

            ImGui.End();
        });

        spriteBatch = new SpriteBatch(GraphicsDevice);
        

        /*

        // Create triangle
        
        // Create 3D Cube
        var cube = new[]
        {
            // Face 1 (Front)
            new VertexPositionColor(new Vector3(-20, -20, 20), Color.Red),
            new VertexPositionColor(new Vector3(20, -20, 20), Color.Green),
            new VertexPositionColor(new Vector3(20, 20, 20), Color.Blue),
            new VertexPositionColor(new Vector3(-20, 20, 20), Color.Gold),
            // Face 2 (Back)
            new VertexPositionColor(new Vector3(-20,-20,-20), Color.Red),
            new VertexPositionColor(new Vector3(-20,20,-20), Color.Green),
            new VertexPositionColor(new Vector3(20,20,-20), Color.Blue),
            new VertexPositionColor(new Vector3(20,-20,-20), Color.Gold),
            // Face 3 (Top)
            new VertexPositionColor(new Vector3(-20, 20, -20), Color.Red),
            new VertexPositionColor(new Vector3(-20, 20, 20), Color.Green),
            new VertexPositionColor(new Vector3(20, 20, 20), Color.Blue),
            new VertexPositionColor(new Vector3(20,20,-20), Color.Gold),
            // Face 4 (Bottom)
            new VertexPositionColor(new Vector3(-20, -20, -20), Color.Red),
            new VertexPositionColor(new Vector3(20, -20, -20), Color.Green),
            new VertexPositionColor(new Vector3(20, -20, 20), Color.Blue),
            new VertexPositionColor(new Vector3(-20, -20, 20), Color.Gold),
            // Face 5 (Right)
            new VertexPositionColor(new Vector3(20,-20,-20), Color.Red),
            new VertexPositionColor(new Vector3(20,20,-20), Color.Green),
            new VertexPositionColor(new Vector3(20,20,20), Color.Blue),
            new VertexPositionColor(new Vector3(20,-20,20), Color.Gold),
            // Face 6 (Left)
            new VertexPositionColor(new Vector3(-20, -20, -20), Color.Red),
            new VertexPositionColor(new Vector3(-20, -20, 20), Color.Green),
            new VertexPositionColor(new Vector3(-20, 20, 20), Color.Blue),
            new VertexPositionColor(new Vector3(-20,20,-20), Color.Gold),
        };
        
        rp.AddPrimitive(cube);
        */
    }

    protected override void LoadContent()
    {
        model = Content.Load<Model>("MonoCube");
        map = Content.Load<Model>("model");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        _gameTime = gameTime;
        
        cam.Update(gameTime);

        if (Keyboard.GetState().IsKeyDown(Keys.O))
        {
            Orbit = !Orbit;
        }
        
        if (Orbit)
        {
            var totalSeconds = gameTime.ElapsedGameTime.TotalSeconds;
            RenderModel.Instance.TransformModel("MonoCube", Matrix.CreateRotationY((1f * (float)totalSeconds)));
            RenderModel.Instance.TransformModel("MonoCube", Matrix.CreateRotationZ((3f * (float)totalSeconds)));
            RenderModel.Instance.TransformModel("MonoCube", Matrix.CreateRotationX((2f * (float)totalSeconds)));
        }

        rp.UpdateVerts();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render ImGui to a separate render target
        GraphicsDevice.SetRenderTarget(ImGUI_RenderTarget);
        GraphicsDevice.Clear(Color.Transparent);

        DebugGuiRenderer.Instance.RenderDebugGUI(GraphicsDevice, gameTime, counter);
        
        // Render game to separate render target as well, used currently for rendering game within ImGui window.
        GraphicsDevice.SetRenderTarget(Game_RenderTarget);
        GraphicsDevice.Clear(Color.Cyan);
        
        // Render 3D Objects/Models
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        // Very important to set to sampler state to wrap or textures will be warped & mangled.
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        RenderModel.Instance.DrawModels(cam.Projection, cam.View);
        GraphicsDevice.SetRenderTarget(null);
        
        // Render ImGui stuff
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null);
        spriteBatch.Draw(ImGUI_RenderTarget, Vector2.Zero, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
        spriteBatch.End();
        
        counter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        base.Draw(gameTime);
    }

    public static Game1 Instance => _instance;
}