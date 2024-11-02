using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MatchThree;

public class MainGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        var inputManager = new InputManager(this);
        Components.Add(inputManager);
        Services.AddService(inputManager);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Services.AddService(_spriteBatch);

        var gameBoard = new GameBoard(this);
        Components.Add(gameBoard);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override bool BeginDraw()
    {
        GraphicsDevice.Clear(Color.Black);
        Services.GetService<SpriteBatch>().Begin();
        return base.BeginDraw();
    }

    protected override void Draw(GameTime gameTime)
    {

        base.Draw(gameTime);
    }

    protected override void EndDraw()
    {
        Services.GetService<SpriteBatch>().End();
        base.EndDraw();
    }
}