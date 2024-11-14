using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Roguelike.Utility;

namespace MatchThree;

public class MainGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public IntVector2 ScreenSize = new(600, 1080);
    public int GameBoardPadding = 10;
    public IntVector2 GameBoardSize = new(560, 560);

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = ScreenSize.X;
        _graphics.PreferredBackBufferHeight = ScreenSize.Y;
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

        var xOffset = (ScreenSize.X - GameBoardSize.X) / 2;
        var yOffset = ScreenSize.Y - GameBoardSize.Y - GameBoardPadding;
        var boardRect = new Rectangle(xOffset, yOffset, GameBoardSize.X, GameBoardSize.Y);
        var gameBoard = new GameBoard(this, boardRect);
        Components.Add(gameBoard);

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